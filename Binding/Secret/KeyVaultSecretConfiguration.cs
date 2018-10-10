using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
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
        // Make these static, particularly the HttpClient, so as not to exhaust the connection pool when using input & output bindings
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();

        private static readonly KeyVaultClient _kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(_tokenProvider.KeyVaultTokenCallback), _httpClient);

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
                var secretBundle = await _kvClient.GetSecretAsync($@"https://{attrib.ResourceNameSetting}.vault.azure.net/secrets/{attrib.SecretIdSetting}");
                return secretBundle.Value;
            }
        }

        // Fortunately (or perhaps unfortunately in the case of this bilnding) every output binding boils down to an IAsyncCollector<T>. This means that ultimately every output binding could be use to "collect" *multiple* changes. While this doesn't make sense for our KV Secret output binding, simply using it as an 'out string mySecret' in the Function eventually pipes it down in to this code where we set the value of the secret reference in the attribute.
        class KeyVaultSecretOutputConverter : IAsyncConverter<KeyVaultSecretAttribute, IAsyncCollector<string>>
        {
            private KeyVaultSecretOutputConverter() { }

            // Provide a static instance to the keyvault converter so the funchost doesn't have to spin it up over and over, potentially exhausting connections or getting rate-limited
            public static KeyVaultSecretOutputConverter Instance { get; } = new KeyVaultSecretOutputConverter();

            public Task<IAsyncCollector<string>> ConvertAsync(KeyVaultSecretAttribute input, CancellationToken cancellationToken)
            {
                return Task.FromResult(new KeyVaultCollector(input) as IAsyncCollector<string>);
            }

            private class KeyVaultCollector : IAsyncCollector<string>
            {
                private readonly KeyVaultSecretAttribute _attrib;

                private Task _currentTask;

                public KeyVaultCollector(KeyVaultSecretAttribute attrib)
                {
                    _attrib = attrib;
                }

                public Task AddAsync(string item, CancellationToken cancellationToken = default(CancellationToken))
                {
                    _currentTask = _kvClient.SetSecretAsync($@"https://{_attrib.ResourceNameSetting}.vault.azure.net", _attrib.SecretIdSetting, item);
                    return _currentTask;
                }

                public async Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
                {
                    await _currentTask;
                }
            }
        }
    }
}
