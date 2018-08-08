using System;
using Microsoft.Azure.WebJobs.Description;

namespace Functions.Extensions.KeyVault
{
    /// <summary>Enables binding to KeyVault to get and set Key Vault Keys.
    /// To retrieve the value of a Key from Key Vault, parameter should be 'JsonWebKey x'</summary>
    /// <seealso cref="System.Attribute" />
    // denote the attribute is a function binding with [Binding]
    [Binding, AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public class KeyVaultKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyVaultKeyAttribute"/> class.
        /// </summary>
        /// <param name="resourceNameSetting">The name of the application setting (eg: MyKeyVault) which holds the name of the Key Vault resource in Azure (eg: my-key-vault)</param>
        /// <param name="keyIdSetting">The name of the application setting which holds the id of the key whose value should be fetched.</param>
        /// <remarks>You must add the appropriate access policies to your KeyVault instance for your Function App's Managed Service Identity in order for this binding to work.</remarks>
        // I prefer to make *required* inputs to the binding be constructor parameters, that way users can't skip them
        public KeyVaultKeyAttribute(string resourceNameSetting, string keyIdSetting)
        {
            this.ResourceNameSetting = resourceNameSetting;
            this.KeyIdSetting = keyIdSetting;
        }

        /// <summary>
        /// Gets the name of the application setting (eg: MyKeyVault) which holds the name of the Key Vault resource in Azure (eg: my-key-vault).
        /// Default: "KeyVaultResourceName"
        /// </summary>
        [AppSetting(Default = @"KeyVaultResourceName")]
        public string ResourceNameSetting { get; }

        /// <summary>
        /// Gets the name of the application setting which holds the id of the key whose value should be fetched.
        /// </summary>
        [AppSetting]
        public string KeyIdSetting { get; }
    }
}
