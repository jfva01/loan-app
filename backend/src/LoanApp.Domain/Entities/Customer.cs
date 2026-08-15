namespace LoanApp.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? Address { get; private set; }
        public string? State { get; private set; }
        public string? CompanyName { get; private set; }
        public DateTime Ssn { get; private set; }

        private Customer() { } // EF Core

        public Customer(string firstName, string lastName, string address, string state, string companyName, DateTime ssn)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            State = state;
            CompanyName = companyName;
            Ssn = ssn;
        }

        public void UpdateFrom(string firstName, string lastName, string address, string state, string companyName, DateTime ssn)
        {
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            State = state;
            CompanyName = companyName;
            // SSN no se actualiza: es la clave de identidad del returning customer, no se puede cambiar.
        }
    }
}