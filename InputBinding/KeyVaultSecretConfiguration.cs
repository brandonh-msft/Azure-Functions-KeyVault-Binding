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
            // Tell the Functions host that we want to add a new binding based on the KeyVaultSecret attribute class
            context.AddBindingRule<KeyVaultSecretAttribute>()
                // Let funchost know it's an Input binding, and how to convert it to the type the user specifies (eg: string). If you want the user to be able to use other types, you must add more 'BindToInput' calls that return those types as values. Here, I have to use a class implementing the IAsyncConverter because I need to call async methods to perform the conversion
                .BindToInput(KeyVaultSecretConverter.Instance)
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
        }


        class KeyVaultSecretConverter : IAsyncConverter<KeyVaultSecretAttribute, string>
        {
            private KeyVaultSecretConverter() { }

            // Provide a static instance to the keyvault converter so the funchost doesn't have to spin it up over and over, potentially exhausting connections or getting rate-limited
            public static KeyVaultSecretConverter Instance { get; } = new KeyVaultSecretConverter();

            // Similar to above; make these static, particularly the HttpClient, so as not to exhaust the connection pool
            private static readonly HttpClient _httpClient = new HttpClient();
            private static readonly AzureServiceTokenProvider _tokenProvider = new AzureServiceTokenProvider();

            private static readonly KeyVaultClient _kvClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(_tokenProvider.KeyVaultTokenCallback), _httpClient);

            // "convert" means "take the attribute, and give me back the <T> (in this case string) the user's asking for." So here, it means "go hit the keyvault instance they've specified and get the value for the secret"
            public async Task<string> ConvertAsync(KeyVaultSecretAttribute attrib, CancellationToken cancellationToken)
            {
                var secretBundle = await _kvClient.GetSecretAsync($@"https://{attrib.ResourceNameSetting}.vault.azure.net/secrets/{attrib.SecretIdSetting}");
                return secretBundle.Value;
            }
        }
    }
}
