namespace Roomfolio.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}
