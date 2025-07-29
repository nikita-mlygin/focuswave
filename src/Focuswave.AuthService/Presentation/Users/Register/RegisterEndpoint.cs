using FastEndpoints;
using Focuswave.AuthService.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Focuswave.AuthService.Presentation.Users.Register;

public record RegisterRequest(string Email, string Password); // TODO отдельный файл

public class RegisterEndpoint(UserManager<User> userManager, ILogger<RegisterEndpoint> logger)
    : Endpoint<RegisterRequest> // TODO разнести по слоям
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly ILogger<RegisterEndpoint> logger = logger;

    public override void Configure()
    {
        Post("api/user/create");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        var user = new User { UserName = req.Email, Email = req.Email };
        var result = await _userManager.CreateAsync(user, req.Password);

        if (!result.Succeeded)
        {
            logger.LogWarning(
                "Error while reg: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description))
            );

            await Send.ErrorsAsync(422, ct);
            return;
        }

        await Send.OkAsync(null, ct);
    }
}
