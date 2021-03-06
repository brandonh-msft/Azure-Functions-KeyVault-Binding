using Azure.Security.KeyVault.Keys;
using Functions.Extensions.KeyVault;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace TestFunctions
{
    public static class KeyBindingTester
    {
        /* 
         * You can easily test this by GET /GetKey
         */

        [FunctionName(nameof(GetKey))]
#pragma warning disable IDE0060 // Remove unused parameter
        public static IActionResult GetKey([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, [KeyVaultKey(@"MyKv", @"MyKeyId")] JsonWebKey kvKey) => new OkObjectResult($@"Key value: {System.Text.Json.JsonSerializer.Serialize(kvKey)}");
#pragma warning restore IDE0060 // Remove unused parameter
    }
}
