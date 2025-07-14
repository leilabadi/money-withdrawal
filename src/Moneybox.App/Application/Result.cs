namespace Moneybox.App.Application;

public class Result
{
    public bool IsSuccessful { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccessful, string? errorMessage = null)
    {
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true);

    public static Result Failure(string errorMessage) => new(false, errorMessage);
}
