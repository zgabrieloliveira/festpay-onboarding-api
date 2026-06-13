using Festpay.Onboarding.Domain.Exceptions;

namespace Festpay.Onboarding.Domain.Entities;

public class Transaction : EntityBase
{
    public Guid SourceAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsCancelled { get; private set; }

    private Transaction() { }

    public override void Validate()
    {
        if (SourceAccountId == Guid.Empty)
            throw new RequiredFieldException(nameof(SourceAccountId));

        if (DestinationAccountId == Guid.Empty)
            throw new RequiredFieldException(nameof(DestinationAccountId));

        if (SourceAccountId == DestinationAccountId)
            throw new SameSourceAndDestinationAccountException();

        if (Amount <= 0)
            throw new InvalidTransactionAmountException(Amount);
    }

    public void Cancel()
    {
        if (IsCancelled)
            throw new TransactionAlreadyCancelledException();

        IsCancelled = true;
    }

    public class Builder
    {
        private readonly Transaction _transaction = new();

        public Builder WithSourceAccount(Guid sourceAccountId)
        {
            _transaction.SourceAccountId = sourceAccountId;
            return this;
        }

        public Builder WithDestinationAccount(Guid destinationAccountId)
        {
            _transaction.DestinationAccountId = destinationAccountId;
            return this;
        }

        public Builder WithAmount(decimal amount)
        {
            _transaction.Amount = amount;
            return this;
        }

        public Transaction Build()
        {
            _transaction.IsCancelled = false; 
            _transaction.Validate();
            return _transaction;
        }
    }
}