using LoanApp.Api.Contracts;
using LoanApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LoanApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly LoanApplicationService _loanApplicationService;

    public ApplicationsController(LoanApplicationService loanApplicationService)
    {
        _loanApplicationService = loanApplicationService;
    }

    [HttpPost]
    public async Task<ActionResult<SubmitApplicationResponseDto>> Submit(
        [FromBody] SubmitApplicationRequestDto request,
        CancellationToken ct)
    {
        var result = await _loanApplicationService.SubmitAsync(
            new SubmitLoanApplicationRequest(
                request.FirstName,
                request.LastName,
                request.Address,
                request.State,
                request.CompanyName,
                request.Ssn,
                request.RequestedAmount
            ), ct
        );

        var response = new SubmitApplicationResponseDto(result.Approved, result.DenialReason, result.ApplicationId);

        // En caso de aprovado o denegado retorna un 200, denegado es un resultado
        // válido del negocio. El front decidirá qué página mostrar según el campo 
        // Approved, no según el estado.
        return Ok(response);
    }
}