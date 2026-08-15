namespace LoanApp.Domain.Rules
{
    // Lo que el motor de reglas necesita evaluar. No es la entidad Customer,
    // es el input crudo del formulario, antes de decidir si se crea o actualiza un Customer.
    public record RuleContext(string State, string Ssn);
}