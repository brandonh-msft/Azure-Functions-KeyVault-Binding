using KeyVaultInputBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TestFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [KeyVaultSecret(@"MyKv", @"MySecretId")]string secretValue, ILogger log)
        {
            return new OkObjectResult($@"Secret: {secretValue}");
        }
    }
}
