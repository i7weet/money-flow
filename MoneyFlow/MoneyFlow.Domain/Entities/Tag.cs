using MoneyFlow.Domain.Common;

namespace MoneyFlow.Domain.Entities;

public sealed class Tag : Entity
{
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
        Name = NormalizeRequired(name, nameof(name));
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        return normalized;
    }
}
