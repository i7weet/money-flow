using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public sealed class Category : Entity
{
    private Category()
    {
        Name = string.Empty;
    }

    public Category(
        Guid id,
        Guid userId,
        string name,
        CategoryType type,
        Guid? parentCategoryId,
        DateTimeOffset createdAt) : base(id)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        UserId = userId;
        Name = NormalizeRequired(name, nameof(name));
        Type = type;
        ParentCategoryId = parentCategoryId;
        CreatedAt = createdAt;
    }

    public Guid UserId { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public string Name { get; private set; }

    public CategoryType Type { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

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
