using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.Application.Common.Interfaces.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsManager { get; }
}
