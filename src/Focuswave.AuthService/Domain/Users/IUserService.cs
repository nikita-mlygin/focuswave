using Focuswave.AuthService.Domain.Users.Registration;

namespace Focuswave.AuthService.Domain.Users;

public interface IUserService
{
    Aff<User> CreateUserAsync(RegistrationUserCommand cmd, CancellationToken cancellationToken);
}
