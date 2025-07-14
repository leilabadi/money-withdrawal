namespace Moneybox.App.Domain.Model;

public record ValidationResult
{
    public bool IsValid { get; }
    public string? ErrorMessage { get; }

    private ValidationResult(bool isValid, string? errorMessage = null)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Success() => new(true);

    public static ValidationResult Failure(string errorMessage) => new(false, errorMessage);
}
