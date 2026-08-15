namespace LoanApp.Domain.Entities
{
    public class LoanApplication
    {
        public Guid Id { get; private set; }
        public decimal RequestedAmount { get; private set; }
        public Guid CustomerId { get; private set; }

        private LoanApplication() { } // EF Core
        public LoanApplication(decimal requestedAmount, Guid customerId)
        {
            Id = Guid.NewGuid();
            RequestedAmount = requestedAmount;
            CustomerId = customerId;
        }

        public void UpdateAmount(decimal requestedAmount)
        {
            RequestedAmount = requestedAmount;
        }
    }
}