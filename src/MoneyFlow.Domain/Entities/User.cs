using System.Net.Mail;
using MoneyFlow.Domain.Common;

namespace MoneyFlow.Domain.Entities;

public sealed class User : Entity
{
    public const int MaxEmailLength = 320;
    public const int MaxPasswordHashLength = 512;

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(Guid id, string email, string passwordHash, DateTimeOffset createdAt) : base(id)
    {
        Email = NormalizeEmail(email, nameof(email));
        PasswordHash = ValidatePasswordHash(passwordHash, nameof(passwordHash));
        CreatedAt = ValidateCreatedAt(createdAt, nameof(createdAt));
    }

    private static DateTimeOffset ValidateCreatedAt(DateTimeOffset value, string paramName)
    {
        if (value == default)
        {
            throw new ArgumentException("Creation time must be specified.", paramName);
        }
        return value;
    }

    private static string ValidatePasswordHash(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Password hash cannot be empty.", paramName);
        }

        if (value.Length > MaxPasswordHashLength)
        {
            throw new ArgumentException($"Password hash cannot exceed {MaxPasswordHashLength} characters.", paramName);
        }

        return value;
    }

    private static string NormalizeEmail(string value, string paramName)
    {
        var normalized = NormalizeRequired(value, paramName).ToLowerInvariant();
        if (normalized.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", paramName);
        }

        if (!MailAddress.TryCreate(normalized, out var address) || address.Address != normalized)
        {
            throw new ArgumentException("Invalid email address.", paramName);
        }
        return normalized.ToLowerInvariant();
    }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        return normalized;
    }
}
