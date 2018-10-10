using System;
using Microsoft.Azure.WebJobs;

namespace Functions.Extensions.KeyVault.Key
{
    /// <summary></summary>
    public static class KeyVaultKeyExtensions
    {
        /// <summary>
        /// Adds the KeyVaultSecret extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to configure.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">builder</exception>
        public static IWebJobsBuilder AddKeyVaultKey(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
               .AddExtension<KeyVaultKeyConfiguration>();

            return builder;
        }
    }
}
