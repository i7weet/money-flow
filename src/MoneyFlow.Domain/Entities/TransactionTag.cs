namespace MoneyFlow.Domain.Entities;

public sealed class TransactionTag
{
    private TransactionTag()
    {
    }

    public TransactionTag(Guid transactionId, Guid tagId)
    {
        if (transactionId == Guid.Empty)
        {
            throw new ArgumentException("Transaction id cannot be empty.", nameof(transactionId));
        }

        if (tagId == Guid.Empty)
        {
            throw new ArgumentException("Tag id cannot be empty.", nameof(tagId));
        }

        TransactionId = transactionId;
        TagId = tagId;
    }

    public Guid TransactionId { get; private set; }

    public Guid TagId { get; private set; }
}
