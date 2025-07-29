using Focuswave.AuthService.Domain.Users;
using Focuswave.AuthService.Domain.Users.Registration;
using Microsoft.AspNetCore.Identity;

namespace Focuswave.AuthService.Application.Users;

internal class UserService(UserManager<User> userManager) : IUserService
{
    private readonly UserManager<User> userManager = userManager;

    Aff<User> IUserService.CreateUserAsync(
        RegistrationUserCommand cmd,
        CancellationToken cancellationToken
    )
    {
        var user = new User { UserName = cmd.Email, Email = cmd.Email };

        return cancellationToken.IsCancellationRequested
            ? FailAff<User>(Error.New("Cancelled operation"))
            : userManager.CreateAsync(user, cmd.Password).ToAff().Map(x => user);
    }
}
