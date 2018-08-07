# An Azure Function binding for KeyVault
1. Create a new Azure Function instance in Azure
1. Create a new KeyVault instance in Azure
1. Ensure the Azure Function has 'Managed Service Identity' turned on
1. Add the Azure Function (by resource name) to the Key Vault's Access Policy list with 'Secret | Get' permissions
	Fill out only the 'Principal' part, not the Application part of the form
1. Use the KeyVault binding in your Azure Function like:
```csharp
public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [KeyVaultSecret(@"MyKv", @"MySecretId")]string secretValue, ILogger log)
```

where `MyKv` and `MySecretId` are defined in your app settings like:
```json
"MyKv": "kv23958612",
"MySecretId": "fooSecret"
```

6. Run your function & you will see the `secretValue` parameter populated with the value from the `MyKv` Key Vault for the secret `MySecretId`

## Coming Soon
- Input binding for Keys
- Output binding for Secrets
- Output binding for Keys