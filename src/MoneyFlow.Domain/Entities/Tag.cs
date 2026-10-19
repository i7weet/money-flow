using MoneyFlow.Domain.Common;

namespace MoneyFlow.Domain.Entities;

public sealed class Tag : Entity
{
    public const int MaxNameLength = 64;
    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private Tag()
    {
        Name = string.Empty;
    }

    public Tag(Guid id, Guid userId, string name, DateTimeOffset createdAt) : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        UserId = userId;
        Name = NormalizeName(name, nameof(name));
        CreatedAt = ValidateCreatedAt(createdAt, nameof(createdAt));
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name, nameof(name));
    }

    private static string NormalizeName(string? value, string paramName)
    {
        var normalized = value?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentException($"Tag name cannot exceed {MaxNameLength} characters", paramName);
        }

        return normalized;
    }

    private static DateTimeOffset ValidateCreatedAt(DateTimeOffset value, string paramName)
    {
        if (value == default)
        {
            throw new ArgumentException("Creation time must be specified.", paramName);
        }

        return value;
    }
}
