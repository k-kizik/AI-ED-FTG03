using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Domain.Entities;

namespace LegalDocumentComparator.Application.UseCases.Auth.Register;

public class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<RegisterResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ValidationException(nameof(command.Email), "Email is required");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ValidationException(nameof(command.Password), "Password is required");

        if (command.Password.Length < 6)
            throw new ValidationException(nameof(command.Password), "Password must be at least 6 characters");

        var emailExists = await _userRepository.EmailExistsAsync(command.Email.ToLowerInvariant(), cancellationToken);
        
        if (emailExists)
            throw new ValidationException(nameof(command.Email), "Email already exists");

        var passwordHash = _authService.HashPassword(command.Password);
        var user = User.Create(command.Email.ToLowerInvariant(), passwordHash, command.Role);

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterResult
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
