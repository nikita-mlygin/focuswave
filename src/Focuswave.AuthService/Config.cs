using Duende.IdentityModel;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Test;
using Focuswave.AuthService.Domain.Users;
using Client = Duende.IdentityServer.Models.Client;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource()
            {
                Name = "verification",
                UserClaims = [JwtClaimTypes.Email, JwtClaimTypes.EmailVerified],
            },
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [new ApiScope("api", "Auth API"), new ApiScope("offline_access")];

    public static IEnumerable<ApiResource> ApiResources =>
        [new ApiResource("api", "Auth API") { Scopes = { "api" } }];

    public static IEnumerable<Client> Clients =>
        [
            new Client
            {
                ClientId = "client-app",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("secret".Sha256()) }, // TODO

                AllowedScopes =
                {
                    "api",
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "offline_access",
                },
                AllowOfflineAccess = true, // включает refresh-токены
                RefreshTokenUsage = TokenUsage.OneTimeOnly,
                RefreshTokenExpiration = TokenExpiration.Sliding,
                AbsoluteRefreshTokenLifetime = 2592000, // 30 дней
                SlidingRefreshTokenLifetime = 1296000, // 15 дней
            },
        ];

    public static List<TestUser> TestUsers =>
        [
            new TestUser()
            {
                SubjectId = "1",
                Username = "test@example.com",
                Password = "test",
            },
        ];
}
