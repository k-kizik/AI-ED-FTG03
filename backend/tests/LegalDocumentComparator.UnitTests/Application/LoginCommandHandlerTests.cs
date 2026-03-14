using FluentAssertions;
using LegalDocumentComparator.Application.Common.Exceptions;
using LegalDocumentComparator.Application.Common.Interfaces.Repositories;
using LegalDocumentComparator.Application.Common.Interfaces.Services;
using LegalDocumentComparator.Application.UseCases.Auth.Login;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(_userRepoMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsLoginResult()
    {
        var user = User.Create("user@example.com", "hashedPassword", UserRole.User);
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _authServiceMock.Setup(a => a.VerifyPassword("password123", user.PasswordHash)).Returns(true);
        _authServiceMock.Setup(a => a.GenerateJwtToken(user.Id, user.Email, user.Role)).Returns("jwt-token");

        var result = await _sut.Handle(new LoginCommand { Email = "user@example.com", Password = "password123" }, default);

        result.Token.Should().Be("jwt-token");
        result.Email.Should().Be("user@example.com");
        result.UserId.Should().Be(user.Id);
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_UpdatesLastLogin()
    {
        var user = User.Create("user@example.com", "hash", UserRole.User);
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _authServiceMock.Setup(a => a.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _authServiceMock.Setup(a => a.GenerateJwtToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<UserRole>()))
            .Returns("token");

        await _sut.Handle(new LoginCommand { Email = "user@example.com", Password = "pass" }, default);

        _userRepoMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.LastLoginAt != null), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyEmail_ThrowsValidationException(string? email)
    {
        var act = () => _sut.Handle(new LoginCommand { Email = email!, Password = "pass" }, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyPassword_ThrowsValidationException(string? password)
    {
        var act = () => _sut.Handle(new LoginCommand { Email = "user@example.com", Password = password! }, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentEmail_ThrowsValidationException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new LoginCommand { Email = "nobody@example.com", Password = "pass" }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsValidationException()
    {
        var user = User.Create("user@example.com", "hash", UserRole.User);
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _authServiceMock.Setup(a => a.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var act = () => _sut.Handle(new LoginCommand { Email = "user@example.com", Password = "wrongpass" }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_LookupUsesLowercasedEmail()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.Handle(new LoginCommand { Email = "USER@EXAMPLE.COM", Password = "pass" }, default);

        await act.Should().ThrowAsync<ValidationException>();
        _userRepoMock.Verify(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }
}
