using FluentAssertions;
using LegalDocumentComparator.Domain.Entities;
using LegalDocumentComparator.Domain.Enums;

namespace LegalDocumentComparator.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidInputs_ReturnsUserWithCorrectProperties()
    {
        var user = User.Create("Test@Example.com", "hash123", UserRole.User);

        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().Be("hash123");
        user.Role.Should().Be(UserRole.User);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.LastLoginAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithManagerRole_SetsRoleCorrectly()
    {
        var user = User.Create("manager@example.com", "hash", UserRole.Manager);

        user.Role.Should().Be(UserRole.Manager);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        var act = () => User.Create(email!, "hash", UserRole.User);

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidPasswordHash_ThrowsArgumentException(string? hash)
    {
        var act = () => User.Create("user@example.com", hash!, UserRole.User);

        act.Should().Throw<ArgumentException>().WithParameterName("passwordHash");
    }

    [Fact]
    public void Create_NormalizesEmailToLowercase()
    {
        var user = User.Create("USER@EXAMPLE.COM", "hash", UserRole.User);

        user.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void UpdateLastLogin_SetsLastLoginAtToNow()
    {
        var user = User.Create("user@example.com", "hash", UserRole.User);

        user.UpdateLastLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdatePassword_WithValidHash_UpdatesPasswordHash()
    {
        var user = User.Create("user@example.com", "oldHash", UserRole.User);

        user.UpdatePassword("newHash");

        user.PasswordHash.Should().Be("newHash");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdatePassword_WithInvalidHash_ThrowsArgumentException(string? hash)
    {
        var user = User.Create("user@example.com", "hash", UserRole.User);

        var act = () => user.UpdatePassword(hash!);

        act.Should().Throw<ArgumentException>().WithParameterName("newPasswordHash");
    }

    [Fact]
    public void Create_GeneratesUniqueIds()
    {
        var user1 = User.Create("a@example.com", "hash", UserRole.User);
        var user2 = User.Create("b@example.com", "hash", UserRole.User);

        user1.Id.Should().NotBe(user2.Id);
    }
}
