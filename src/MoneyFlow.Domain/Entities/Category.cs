using MoneyFlow.Domain.Common;
using MoneyFlow.Domain.Enums;

namespace MoneyFlow.Domain.Entities;

public sealed class Category : Entity
{
    public const int MaxNameLength = 128;

    public Guid UserId { get; private set; }

    public Guid? ParentCategoryId { get; private set; }

    public string Name { get; private set; }

    public CategoryType Type { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTimeOffset? ArchivedAt { get; private set; }

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
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        UserId = userId;

        Name = NormalizeName(name, nameof(name));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown category type.");
        Type = type;

        if (parentCategoryId == Guid.Empty)
            throw new ArgumentException("Parent category id cannot be empty.", nameof(parentCategoryId));
        if (parentCategoryId == id)
            throw new ArgumentException("Category cannot be its own parent.", nameof(parentCategoryId));
        ParentCategoryId = parentCategoryId;

        CreatedAt = ValidateCreatedAt(createdAt, nameof(createdAt));
    }

    public void Rename(string name)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Archived category cannot be renamed.");
        }

        Name = NormalizeName(name, nameof(name));
    }

    public void Archive(DateTimeOffset archivedAt)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("Category is already archived.");
        }

        if (archivedAt == default)
        {
            throw new ArgumentException(
                "Archive time must be specified.",
                nameof(archivedAt));
        }

        if (archivedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(archivedAt),
                "Archive time cannot be earlier than creation time.");
        }

        IsArchived = true;
        ArchivedAt = archivedAt;
    }

    private static string NormalizeName(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Value cannot be empty.", paramName);
        }

        if (normalized.Length > MaxNameLength)
        {
            throw new ArgumentException($"Value cannot be longer than {MaxNameLength} characters.", paramName);
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