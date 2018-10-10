using System;
using Microsoft.Azure.WebJobs;

namespace Functions.Extensions.KeyVault.Secret
{
    /// <summary></summary>
    public static class KeyVaultSecretExtensions
    {
        /// <summary>
        /// Adds the KeyVaultSecret extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">builder</exception>
        public static IWebJobsBuilder AddKeyVaultSecret(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
               .AddExtension<KeyVaultSecretConfiguration>();

            return builder;
        }
    }
}
