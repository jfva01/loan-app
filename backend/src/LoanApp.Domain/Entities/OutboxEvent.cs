namespace LoanApp.Domain.Entities
{
    public enum OutboxEventStatus {Pending, Sent, Failed}
    public enum ExternalOperation {Create, Update}

    public class OutboxEvent
    {
        public Guid Id { get; private set; }
        public string? Payload { get; private set; } // Json srializado
        public ExternalOperation Operation { get; private set; }
        public OutboxEventStatus Status { get; private set; }
        public int Attempts { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? ProcessedAtUtc { get; private set; }

        private OutboxEvent() { } // EF Core

        public OutboxEvent(string payload, ExternalOperation operation)
        {
            Id = Guid.NewGuid();
            Payload = payload;
            Operation = operation;
            Status = OutboxEventStatus.Pending;
            Attempts = 0;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public void MarkAsSent()
        {
            Status = OutboxEventStatus.Sent;
            ProcessedAtUtc = DateTime.UtcNow;
        }

        public void MarkAttemptFailed()
        {
            Status = OutboxEventStatus.Failed; // el BackgroundService decide si reintenta según la cantidad de intentos
            Attempts++;
        }
    }
}