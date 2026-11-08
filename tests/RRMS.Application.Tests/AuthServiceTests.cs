using Microsoft.Extensions.Logging;
using RRMS.Application.DTOs.Auth;
using RRMS.Application.Exceptions;
using RRMS.Application.Interfaces.Repositories;
using RRMS.Application.Interfaces.Services;
using RRMS.Application.Services;
using RRMS.Domain.Entities;
using RRMS.Domain.Enums;

namespace RRMS.Application.Tests;

public class AuthServiceTests
{
    private const string Password = "Str0ngPassw0rd!";

    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork;
    private readonly FakeJwtTokenService _jwt = new();
    private readonly FakeEmailService _email = new();
    private readonly CapturingLogger _logger = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        _unitOfWork = new FakeUnitOfWork(_users);
        _service = new AuthService(_unitOfWork, _jwt, _email, _logger);
    }

    private User AddUser(string email = "user@example.com", string password = Password)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4),
            Role = UserRole.Customer,
            CreatedAtUtc = DateTime.UtcNow
        };
        _users.Items.Add(user);
        return user;
    }

    // ---------- Login ----------

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = AddUser();

        var response = await _service.LoginAsync(
            new LoginRequest(Email: "user@example.com", Password: Password));

        Assert.Equal(FakeJwtTokenService.Token, response.Token);
        Assert.Same(user, _jwt.LastUser);
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmail()
    {
        AddUser("user@example.com");

        var response = await _service.LoginAsync(
            new LoginRequest(Email: "  USER@Example.COM  ", Password: Password));

        Assert.Equal(FakeJwtTokenService.Token, response.Token);
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentials()
    {
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(
                new LoginRequest(Email: "nobody@example.com", Password: Password)));

        Assert.Equal(0, _jwt.CallCount);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        AddUser();

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _service.LoginAsync(
                new LoginRequest(Email: "user@example.com", Password: "wrong-password")));

        Assert.Equal(0, _jwt.CallCount);
    }

    // ---------- Register ----------

    [Fact]
    public async Task RegisterAsync_CreatesCustomerAndReturnsToken()
    {
        var response = await _service.RegisterAsync(
            new RegisterRequest(Name: "John", Email: "john@example.com", Password: Password));

        var user = Assert.Single(_users.Items);
        Assert.Equal(UserRole.Customer, user.Role);
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(FakeJwtTokenService.Token, response.Token);
        Assert.Same(user, _jwt.LastUser);
        Assert.Equal(1, _unitOfWork.SaveCount);
    }

    [Fact]
    public async Task RegisterAsync_TrimsNameAndNormalizesEmail()
    {
        await _service.RegisterAsync(
            new RegisterRequest(Name: "  John  ", Email: "  John@Example.COM ", Password: Password));

        var user = Assert.Single(_users.Items);
        Assert.Equal("John", user.Name);
        Assert.Equal("john@example.com", user.Email);
    }

    [Fact]
    public async Task RegisterAsync_StoresBCryptHashInsteadOfPlainPassword()
    {
        await _service.RegisterAsync(
            new RegisterRequest(Name: "John", Email: "john@example.com", Password: Password));

        var user = Assert.Single(_users.Items);
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        AddUser("john@example.com");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RegisterAsync(
                new RegisterRequest(Name: "John", Email: "john@example.com", Password: Password)));

        Assert.Single(_users.Items);
        Assert.Equal(0, _unitOfWork.SaveCount);
        Assert.Empty(_email.Sent);
        Assert.Equal(0, _jwt.CallCount);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmailWithDifferentCase_ThrowsConflict()
    {
        AddUser("john@example.com");

        await Assert.ThrowsAsync<ConflictException>(
            () => _service.RegisterAsync(
                new RegisterRequest(Name: "John", Email: "  JOHN@example.com ", Password: Password)));
    }

    [Fact]
    public async Task RegisterAsync_SendsWelcomeEmail()
    {
        await _service.RegisterAsync(
            new RegisterRequest(Name: " John ", Email: "John@Example.com", Password: Password));

        var sent = Assert.Single(_email.Sent);
        Assert.Equal("john@example.com", sent.Email);
        Assert.Equal("John", sent.Name);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailFails_StillSucceedsAndLogsWarning()
    {
        _email.ExceptionToThrow = new InvalidOperationException("smtp down");

        var response = await _service.RegisterAsync(
            new RegisterRequest(Name: "John", Email: "john@example.com", Password: Password));

        Assert.Equal(FakeJwtTokenService.Token, response.Token);
        Assert.Single(_users.Items);
        Assert.Equal(1, _unitOfWork.SaveCount);

        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Same(_email.ExceptionToThrow, entry.Exception);
    }

    // ---------- Fakes ----------

        private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Items { get; } = new();

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(u => u.Email == email));

        public Task<bool> ExistsByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(u => u.Email == email));

        public Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(u => u.Id == id));

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            Items.Add(user);
            return Task.CompletedTask;
        }

        public void Update(User user)
        {
        }

        public void Delete(User user)
        {
            Items.Remove(user);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(IUserRepository users) => Users = users;

        public int SaveCount { get; private set; }

        public IUserRepository Users { get; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public const string Token = "fake-jwt-token";

        public int CallCount { get; private set; }

        public User? LastUser { get; private set; }

        public string GenerateToken(User user)
        {
            CallCount++;
            LastUser = user;
            return Token;
        }
    }

    private sealed class FakeEmailService : IEmailService
    {
        public List<(string Email, string Name)> Sent { get; } = new();

        public Exception? ExceptionToThrow { get; set; }

        public Task SendWelcomeEmailAsync(
            string email,
            string name,
            CancellationToken cancellationToken = default)
        {
            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            Sent.Add((email, name));
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingLogger : ILogger<AuthService>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Entries.Add((logLevel, exception));
    }
}