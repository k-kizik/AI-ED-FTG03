using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.Application.Common.Interfaces.Services;

public interface IAuthService
{
    string GenerateJwtToken(Guid userId, string email, UserRole role);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
