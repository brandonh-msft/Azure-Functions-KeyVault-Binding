using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;

namespace Functions.Extensions.KeyVault
{
    /// <summary></summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Host.Config.IExtensionConfigProvider" />
    public class KeyVaultSecretConfiguration : IExtensionConfigProvider
    {
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<KeyVaultSecretAttribute>()
                .BindToInput(KeyVaultSecretConverter.Instance)
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
        }


        class KeyVaultSecretConverter : IAsyncConverter<KeyVaultSecretAttribute, string>
        {
            private KeyVaultSecretConverter() { }

            public static KeyVaultSecretConverter Instance { get; } = new KeyVaultSecretConverter();

            private static readonly HttpClient _httpClient = new HttpClient();
            private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();

            private static readonly KeyVaultClient _kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(_tokenProvider.KeyVaultTokenCallback), _httpClient);

            public async Task<string> ConvertAsync(KeyVaultSecretAttribute attrib, CancellationToken cancellationToken)
            {
                var secretBundle = await _kvClient.GetSecretAsync($@"https://{attrib.ResourceNameSetting}.vault.azure.net/secrets/{attrib.SecretIdSetting}");
                return secretBundle.Value;
            }
        }
    }
}
