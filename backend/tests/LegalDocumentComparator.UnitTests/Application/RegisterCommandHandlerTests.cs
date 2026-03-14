using FluentAssertions;
using LegalDocumentComparator.Application.UseCases.Auth.Register;
using Moq;

namespace LegalDocumentComparator.UnitTests.Application;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly RegisterCommandHandler _sut;

    public RegisterCommandHandlerTests()
    {
        _authServiceMock.Setup(a => a.HashPassword(It.IsAny<string>())).Returns("hashedPwd");
        _sut = new RegisterCommandHandler(_userRepoMock.Object, _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidInputs_ReturnsRegisterResult()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(new RegisterCommand { Email = "user@example.com", Password = "secure123" }, default);

        result.Email.Should().Be("user@example.com");
        result.Role.Should().Be("User");
        result.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidInputs_CallsAddAsync()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.Handle(new RegisterCommand { Email = "user@example.com", Password = "secure123" }, default);

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithManagerRole_SetsRoleCorrectly()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(new RegisterCommand
        {
            Email = "mgr@example.com",
            Password = "secure123",
            Role = UserRole.Manager
        }, default);

        result.Role.Should().Be("Manager");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyEmail_ThrowsValidationException(string? email)
    {
        var act = () => _sut.Handle(new RegisterCommand { Email = email!, Password = "secure123" }, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithEmptyPassword_ThrowsValidationException(string? password)
    {
        var act = () => _sut.Handle(new RegisterCommand { Email = "user@example.com", Password = password! }, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("a")]
    [InlineData("12345")]
    public async Task Handle_WithPasswordTooShort_ThrowsValidationException(string password)
    {
        var act = () => _sut.Handle(new RegisterCommand { Email = "user@example.com", Password = password }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*6 characters*");
    }

    [Fact]
    public async Task Handle_WithDuplicateEmail_ThrowsValidationException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.Handle(new RegisterCommand { Email = "user@example.com", Password = "secure123" }, default);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowercase()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.Handle(new RegisterCommand { Email = "USER@EXAMPLE.COM", Password = "secure123" }, default);

        result.Email.Should().Be("user@example.com");
    }
}
