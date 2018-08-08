using System;
using System.IO;
using Functions.Extensions.KeyVault;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TestFunctions
{
    public static class SecretBindingTester
    {
        /* 
         * You can easily test this by first GET /GetSecret, then POST to the /SetSecret function a new value (no decorating, just the string to set it to) then GET /GetSecret again
         */

        [FunctionName(nameof(GetSecret))]
        public static IActionResult GetSecret([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [KeyVaultSecret(@"MyKv", @"MySecretId")]string secretValue, ILogger log)
        {
            return new OkObjectResult($@"Secret: {secretValue}");
        }

        [FunctionName(nameof(SetSecret))]
        public static IActionResult SetSecret([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]HttpRequest req, [KeyVaultSecret(@"MyKv", @"MySecretId")]out string secretValue, ILogger log)
        {
            // can't use the async overload here & async/await, because async methods can't have 'out' params.
            // This output binding doesn't make sense to use with an IAsyncCollector<string> because why would you set the same secret's value multiple times throughout a run?
            secretValue = new StreamReader(req.Body).ReadToEnd();

            return new OkObjectResult($@"[{Environment.GetEnvironmentVariable(@"MySecretId")}]: {secretValue}");
        }
    }
}
