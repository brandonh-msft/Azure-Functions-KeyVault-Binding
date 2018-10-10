using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;


namespace Functions.Extensions.KeyVault.Secret
{
    /// <summary></summary>
    /// <seealso cref="Microsoft.Azure.WebJobs.Hosting.IWebJobsStartup" />
    public class KeyVaultSecretWebJobsStartup : IWebJobsStartup
    {
        /// <summary>
        /// Configures the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public void Configure(IWebJobsBuilder builder)
        {
        }
    }
}
