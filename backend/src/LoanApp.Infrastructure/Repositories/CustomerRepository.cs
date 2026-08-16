using LoanApp.Application.Abstractions;
using LoanApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanApp.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetBySsnAsync(string ssn, CancellationToken ct = default) => 
        _context.Customers.FirstOrDefaultAsync(c => c.Ssn == ssn, ct);

    public Task<LoanApplication?> GetApplicationByCustomerIdAsync (Guid customerId, CancellationToken ct = default) =>
        _context.Applications.FirstOrDefaultAsync(a => a.CustomerId == a.CustomerId, ct);

    public void AddCustomer(Customer customer) => _context.Customers.Add(customer);

    public void AddApplication(LoanApplication application) => _context.Applications.Add(application);
}