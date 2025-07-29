using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.SecretsEngines.TOTP;

namespace Focuswave.Common.Infrastructure.Vault;

public class VaultService
{
    private readonly VaultClient vaultClient;

    public VaultService(VaultOptions options)
    {
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(options.RoleId, options.SecretId);

        var vaultClientSettings = new VaultClientSettings(options.Address, authMethod)
        {
            ContinueAsyncTasksOnCapturedContext = false,
        };

        vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task TestConnection()
    {
        var tokenInfo = await vaultClient.V1.Auth.Token.LookupSelfAsync();
        Console.WriteLine($"Vault token is valid. TTL: {tokenInfo.Data.TimeToLive}");
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

    public Aff<Option<List<string>>> GetSecretsAsync(string path, IEnumerable<string> keys)
    {
        return vaultClient
            .V1.Secrets.KeyValue.V2.ReadSecretAsync(path, null, "secret")
            .ToAff()
            .Bind(secret =>
            {
                var tmp = new List<string> { };

                if (secret?.Data?.Data == null)
                    return FailAff<Option<List<string>>>(Error.New("Not found"));

                bool allKeysFound = keys.All(key =>
                {
                    if (!secret.Data.Data.TryGetValue(key, out var value))
                        return false;

                    if (value?.ToString() is not string str)
                        return false;

                    tmp.Add(str);
                    return true;
                });

                return allKeysFound
                    ? SuccessAff(Some(tmp))
                    : FailAff<Option<List<string>>>(Error.New("Not found"));
            });
    }
}
