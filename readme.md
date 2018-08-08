[![Build status](https://bc3tech.visualstudio.com/Projects/_apis/build/status/KeyVault%20Binding%20CI)](https://bc3tech.visualstudio.com/Projects/_build/latest?definitionId=38)

# An Azure Functions (2.0) binding for KeyVault
1. Create a new Azure Function instance in Azure
1. Create a new KeyVault instance in Azure
1. Ensure the Azure Function has 'Managed Service Identity' turned on
1. Add the Azure Function (by resource name) to the Key Vault's Access Policy list with 'Secret | Get' permissions
	Fill out only the 'Select Principal' part, not the 'Authorized application' part of the form

    You can get more detail on setting this up by reading [this blog post from Functions PM, Jeff Hollan](https://medium.com/statuscode/getting-key-vault-secrets-in-azure-functions-37620fd20a0b).
1. Use the KeyVault binding in your Azure Function by:

Adding the nuget package to your project

~~~
Install-Package BC3Technologies.Azure.Functions.Extensions.KeyVault -IncludePrerelease
~~~

Then referencing it in your Function definition

```csharp
public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, [KeyVaultSecret(@"MyKv", @"MySecretId")]string secretValue, ILogger log)
```

where `MyKv` and `MySecretId` are defined in your app settings like:
```json
"MyKv": "kv23958612",
"MySecretId": "fooSecret"
```

6. Run your function & you will see the `secretValue` parameter populated with the value from the `MyKv` Key Vault for the secret `MySecretId`

- You can also use `[KeyVaultSecret(@"MyKv", @"MySecretId")]**out** string myNewKeyValue` to _set_ the value of `fooSecret` in KeyVault
- You can get a JsonWebKey for a Key Vault **key** by using `[KeyVaultKey("kvresourcesetting","keynamesetting")]JsonWebKey myKey`