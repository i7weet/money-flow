using MoneyFlow.Domain.Common;

namespace MoneyFlow.Domain.Entities;

public sealed class User : Entity
{
    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
    }

    public User(Guid id, string email, string passwordHash, DateTimeOffset createdAt) : base(id)
    {
        Email = NormalizeEmail(email, nameof(email));
        PasswordHash = NormalizeRequired(passwordHash, nameof(passwordHash));
        CreatedAt = createdAt;
    }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private static string NormalizeRequired(string value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        return normalized;
    }

    private static string NormalizeEmail(string value, string paramName)
    {
        var normalized = NormalizeRequired(value, paramName);
        return normalized.ToLowerInvariant();
    }
}
