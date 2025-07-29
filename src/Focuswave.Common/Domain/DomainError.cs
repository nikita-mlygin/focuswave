using System.Security.Permissions;

namespace Focuswave.Common.Domain;

public record DomainError : Expected
{
    public DomainError(int code, string message)
        : base(message, code) { }
}
