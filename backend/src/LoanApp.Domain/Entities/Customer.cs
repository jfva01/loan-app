namespace LoanApp.Domain.Entities
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Address { get; private set; } = string.Empty;
        public string State { get; private set; } = string.Empty;
        public string CompanyName { get; private set; } = string.Empty;
        public string Ssn { get; private set; } = string.Empty;

        private Customer() { } // EF Core

        public Customer(string firstName, string lastName, string address, string state, string companyName, string ssn)
        {
            Id = Guid.NewGuid();
            FirstName = firstName;
            LastName = lastName;
            Address = address;
            State = state;
            CompanyName = companyName;
            Ssn = ssn;
        }

        public void UpdateFrom(string firstName, string lastName, string address, string state, string companyName)
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