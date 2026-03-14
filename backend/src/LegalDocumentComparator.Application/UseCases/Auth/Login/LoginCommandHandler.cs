using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;

namespace LegalDocumentComparator.Application.UseCases.Auth.Login;

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginCommandHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ValidationException(nameof(command.Email), "Email is required");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new ValidationException(nameof(command.Password), "Password is required");

        var user = await _userRepository.GetByEmailAsync(command.Email.ToLowerInvariant(), cancellationToken);
        
        if (user == null || !_authService.VerifyPassword(command.Password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password");
        }

        user.UpdateLastLogin();
        await _userRepository.UpdateAsync(user, cancellationToken);

        var token = _authService.GenerateJwtToken(user.Id, user.Email, user.Role);

        return new LoginResult
        {
            Token = token,
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}
