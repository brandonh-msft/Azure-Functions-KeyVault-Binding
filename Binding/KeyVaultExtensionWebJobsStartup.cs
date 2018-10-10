using Functions.Extensions.KeyVault.Key;
using Functions.Extensions.KeyVault.Secret;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(Functions.Extensions.KeyVault.KeyVaultExtensionWebJobsStartup))]

namespace Functions.Extensions.KeyVault
{
    class KeyVaultExtensionWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder
                .AddKeyVaultSecret()
                .AddKeyVaultKey();
        }
    }
}
