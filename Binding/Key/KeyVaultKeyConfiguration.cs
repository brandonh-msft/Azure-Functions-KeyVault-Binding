using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Functions.Extensions.KeyVault
{

    /// <summary></summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Host.Config.IExtensionConfigProvider" />
    [Extension(@"KeyVaultKey")]
    public class KeyVaultKeyConfiguration : IExtensionConfigProvider
    {
        // This is static so as not to exhaust the connection pool when using input & output bindings
        private static readonly Dictionary<string, KeyClient> _cache = new Dictionary<string, KeyClient>();
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
            context.AddBindingRule<KeyVaultKeyAttribute>()
                // Let funchost know it's an Input binding, and how to convert it to the type the user specifies (eg: string). If you want the user to be able to use other types, you must add more 'BindToInput' calls that return those types as values. Here, I have to use a class implementing the IAsyncConverter because I need to call async methods to perform the conversion
                .BindToInput(KeyVaultKeyInputConverter.Instance)
                // Add a validator on the user's attribute implementation to make sure I can even do the conversion and blow up accordingly if I can't.
                .AddValidator((attrib, t) =>
                {
                    if (string.IsNullOrWhiteSpace(attrib.ResourceNameSetting))
                    {
                        throw new ArgumentException(nameof(attrib.ResourceNameSetting));
                    }

                    if (string.IsNullOrWhiteSpace(attrib.KeyIdSetting))
                    {
                        throw new ArgumentNullException(nameof(attrib.KeyIdSetting));
                    }

                    // if all is good, cache a SecretClient instance for the KeyVault resource
                    if (!_cache.ContainsKey(attrib.ResourceNameSetting))
                    {
                        _cache.Add(attrib.ResourceNameSetting, new KeyClient(new Uri($@"https://{attrib.ResourceNameSetting}.vault.azure.net"), _tokenCredential));
                    }
                });
        }


        class KeyVaultKeyInputConverter : IAsyncConverter<KeyVaultKeyAttribute, JsonWebKey>
        {
            private KeyVaultKeyInputConverter() { }

            // Provide a static instance to the keyvault converter so the funchost doesn't have to spin it up over and over, potentially exhausting connections or getting rate-limited
            public static KeyVaultKeyInputConverter Instance { get; } = new KeyVaultKeyInputConverter();

            // "convert" means "take the attribute, and give me back the <T> (in this case string) the user's asking for." So here, it means "go hit the keyvault instance they've specified and get the value for the secret"
            public async Task<JsonWebKey> ConvertAsync(KeyVaultKeyAttribute attrib, CancellationToken cancellationToken)
            {
                var keyBundle = await _cache[attrib.ResourceNameSetting].GetKeyAsync(attrib.KeyIdSetting);
                return keyBundle.Value.Key;
            }
        }
    }
}
