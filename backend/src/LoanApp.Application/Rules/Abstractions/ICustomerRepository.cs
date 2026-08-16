using LoanApp.Domain.Entities;

namespace LoanApp.Application.Abstractions;

public interface ICustomerRepository
{
    Task<Customer?> GetBySsnAsync(string ssn, CancellationToken ct = default);
    Task<LoanApplication?> GetApplicationByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    void AddCustomer(Customer customer);
    void AddApplication(LoanApplication application);
}