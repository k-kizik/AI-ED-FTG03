using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.Application.UseCases.Auth.Register;

public class RegisterCommand
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
}

public class RegisterResult
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
}
