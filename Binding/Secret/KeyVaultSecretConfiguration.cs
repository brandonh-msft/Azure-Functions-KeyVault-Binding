using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Functions.Extensions.KeyVault
{
    /// <summary></summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Host.Config.IExtensionConfigProvider" />
    [Extension(@"KeyVaultSecret")]
    public class KeyVaultSecretConfiguration : IExtensionConfigProvider
    {
        // This is static so as not to exhaust the connection pool when using input & output bindings
        private static readonly Dictionary<string, SecretClient> _cache = new Dictionary<string, SecretClient>();
        // This is static so the credential-determination logic only gets done once since it's highly unlikely to change during the course of a Function App's execution
        private static readonly TokenCredential _tokenCredential = new DefaultAzureCredential(
#if DEBUG
            // Tune these so the default credential picks from where you're logged into Azure and ready to hit the KV you want to test locally
            new DefaultAzureCredentialOptions
            {
                ExcludeAzureCliCredential = false,
                ExcludeAzurePowerShellCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeManagedIdentityCredential = true,
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true,
            }
#endif
            );

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            // Tell the Functions host that we want to add a new binding based on the KeyVaultSecret attribute class
            context.AddBindingRule<KeyVaultSecretAttribute>()
                // Let funchost know it's an Input binding, and how to convert it to the type the user specifies (eg: string). If you want the user to be able to use other types, you must add more 'BindToInput' calls that return those types as values. Here, I have to use a class implementing the IAsyncConverter because I need to call async methods to perform the conversion
                .BindToInput(KeyVaultSecretInputConverter.Instance)
                // Add a validator on the user's attribute implementation to make sure I can even do the conversion and blow up accordingly if I can't.
                .AddValidator((attrib, t) =>
                {
                    if (string.IsNullOrWhiteSpace(attrib.ResourceNameSetting))
                    {
                        throw new ArgumentException(nameof(attrib.ResourceNameSetting));
                    }

                    if (string.IsNullOrWhiteSpace(attrib.SecretIdSetting))
                    {
                        throw new ArgumentNullException(nameof(attrib.SecretIdSetting));
                    }

                    // if all is good, cache a SecretClient instance for the KeyVault resource
                    if (!_cache.ContainsKey(attrib.ResourceNameSetting))
                    {
                        _cache.Add(attrib.ResourceNameSetting, new SecretClient(new Uri($@"https://{attrib.ResourceNameSetting}.vault.azure.net"), _tokenCredential));
                    }
                });

            context.AddBindingRule<KeyVaultSecretAttribute>()
                .BindToCollector(KeyVaultSecretOutputConverter.Instance);
        }

        class KeyVaultSecretInputConverter : IAsyncConverter<KeyVaultSecretAttribute, string>
        {
            private KeyVaultSecretInputConverter() { }

            // Provide a static instance to the keyvault converter so the funchost doesn't have to spin it up over and over, potentially exhausting connections or getting rate-limited
            public static KeyVaultSecretInputConverter Instance { get; } = new KeyVaultSecretInputConverter();

            // "convert" means "take the attribute, and give me back the <T> (in this case string) the user's asking for." So here, it means "go hit the keyvault instance they've specified and get the value for the secret"
            public async Task<string> ConvertAsync(KeyVaultSecretAttribute attrib, CancellationToken cancellationToken)
            {
                var secretBundle = await _cache[attrib.ResourceNameSetting].GetSecretAsync(attrib.SecretIdSetting);
                return secretBundle.Value.Value;
            }
        }

        // Fortunately (or perhaps unfortunately in the case of this binding) every output binding boils down to an IAsyncCollector<T>. This means that ultimately every output binding could be use to "collect" *multiple* changes. While this doesn't make sense for our KV Secret output binding, simply using it as an 'out string mySecret' in the Function eventually pipes it down in to this code where we set the value of the secret reference in the attribute.
        class KeyVaultSecretOutputConverter : IAsyncConverter<KeyVaultSecretAttribute, IAsyncCollector<string>>
        {
            private KeyVaultSecretOutputConverter() { }

            // Provide a static instance to the keyvault converter so the funchost doesn't have to spin it up over and over, potentially exhausting connections or getting rate-limited
            public static KeyVaultSecretOutputConverter Instance { get; } = new KeyVaultSecretOutputConverter();

            public Task<IAsyncCollector<string>> ConvertAsync(KeyVaultSecretAttribute input, CancellationToken cancellationToken) => Task.FromResult(new KeyVaultCollector(input) as IAsyncCollector<string>);

            private class KeyVaultCollector : IAsyncCollector<string>
            {
                private readonly KeyVaultSecretAttribute _attrib;
                private readonly List<Task> _tasks = new List<Task>();

                public KeyVaultCollector(KeyVaultSecretAttribute attrib) => _attrib = attrib;

                public Task AddAsync(string item, CancellationToken cancellationToken = default)
                {
                    var currentTask = _cache[_attrib.ResourceNameSetting].SetSecretAsync(_attrib.SecretIdSetting, item, cancellationToken);
                    _tasks.Add(currentTask);
                    return currentTask;
                }

                public Task FlushAsync(CancellationToken cancellationToken = default) => Task.WhenAll(_tasks);
            }
        }
    }
}
