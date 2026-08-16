using System.Data;
using System.Net.Cache;
using System.Text.Json;
using LoanApp.Application.Abstractions;
using LoanApp.Application.Rules;
using LoanApp.Domain.Entities;
using LoanApp.Domain.Rules;

namespace LoanApp.Application.Services;

public class LoanApplicationService
{
    private readonly IRuleEngine _ruleEngine;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoanApplicationService(
        IRuleEngine ruleEngine,
        ICustomerRepository customerRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _ruleEngine = ruleEngine;
        _customerRepository = customerRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanApplicationResult> SubmitAsync(SubmitLoanApplicationRequest request, CancellationToken ct = default)
    {
        var ruleResult = _ruleEngine.Evaluate(new RuleContext(request.State, request.Ssn));

        if(ruleResult.IsDenied)
            return new LoanApplicationResult(false, ruleResult.Reason, null);

        var existingCustomer = await _customerRepository.GetBySsnAsync(request.Ssn, ct);
        var isReturningCustomer = existingCustomer is not null;

        Customer customer;
        LoanApplication application;

        if (isReturningCustomer)
        {
            customer = existingCustomer!;
            customer.UpdateFrom(request.FirstName, request.LastName, request.Address, request.State, request.CompanyName);

            var existingApplication = await _customerRepository.GetApplicationByCustomerIdAsync(customer.Id, ct);

            // Si el cliente existe, su aplicación también debe existir
            // siempre se crean juntos, nunca por separado
            if(existingApplication is null)
                throw new InvalidOperationException($"Customer {customer.Id} - {customer.FirstName} {customer.LastName} exists without an application.");

            existingApplication.UpdateAmount(request.RequestedAmount);
            application = existingApplication;
        }
        else
        {
            customer = new Customer(request.FirstName, request.LastName, request.Address, request.State, request.CompanyName, request.Ssn);
            application = new LoanApplication(request.RequestedAmount, customer.Id);

            _customerRepository.AddCustomer(customer);
            _customerRepository.AddApplication(application);
        }

        var payload = new LoanApplicationPayload(
            customer.Id, customer.FirstName!, customer.LastName!,
            customer.Address!, customer.State!, customer.CompanyName!,
            application.Id, application.RequestedAmount);

        var payloadJson = JsonSerializer.Serialize(payload);
        var operation = isReturningCustomer ? ExternalOperation.Update : ExternalOperation.Create;

        _outboxRepository.Add(new OutboxEvent(payloadJson, operation));

        // Un solo SaveChangesAsync: Cliente/Aplicación (insert o update) + OutboxEvent
        // se confirman o revierten juntos
        await _unitOfWork.SaveChangesAsync(ct);

        return new LoanApplicationResult(true, null, application.Id);
    }
}