using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;

namespace Focuswave.AuthService.Infrastructure.Vault;

internal class VaultService
{
    private readonly IVaultClient vaultClient;

    public VaultService(VaultOptions options)
    {
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(options.RoleId, options.SecretId);

        var vaultClientSettings = new VaultClientSettings(options.Address, authMethod)
        {
            ContinueAsyncTasksOnCapturedContext = false,
            // Namespace = "" // если ты используешь Vault Enterprise с namespace
        };

        vaultClient = new VaultClient(vaultClientSettings);
    }

    public Aff<Option<string>> GetSecretAsync(string path, string key)
    {
        return vaultClient
            .V1.Secrets.KeyValue.V2.ReadSecretAsync(path, null, "secret")
            .ToAff()
            .Bind(secret =>
                secret?.Data?.Data != null && secret.Data.Data.TryGetValue(key, out var value)
                    ? SuccessAff(Optional(value?.ToString()))
                    : FailAff<Option<string>>(Error.New("Not found"))
            );
    }
}
