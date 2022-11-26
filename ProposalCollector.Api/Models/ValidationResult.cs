namespace ProposalCollector.Api.Models;

public record ValidationResult(bool IsValid, string? ErrorMessage)
{
    public static ValidationResult Success => new ValidationResult(true, null);
}
