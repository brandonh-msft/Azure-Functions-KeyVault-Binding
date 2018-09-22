using Functions.Extensions.KeyVault;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TestFunctions
{
    public static class KeyBindingTester
    {
        /* 
         * You can easily test this by GET /GetKey
         */

        [FunctionName(nameof(GetKey))]
        public static IActionResult GetKey([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, 
            [KeyVaultKey(@"MyKv", @"MySecretId")]JsonWebKey kvKey, 
            ILogger log)
        {
            return new OkObjectResult($@"Key value: {kvKey.ToString()}");
        }
    }
}
