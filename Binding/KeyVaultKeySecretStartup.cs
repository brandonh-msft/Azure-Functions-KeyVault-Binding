using Functions.Extensions.KeyVault;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(KeyVaultKeySecretStartup))]

namespace Functions.Extensions.KeyVault
{
    public class KeyVaultKeySecretStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<KeyVaultKeyConfiguration>();
            builder.AddExtension<KeyVaultSecretConfiguration>();
        }
    }
}
