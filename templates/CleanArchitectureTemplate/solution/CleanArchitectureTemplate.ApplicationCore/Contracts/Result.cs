namespace CleanArchitectureTemplate.ApplicationCore.Contracts;

public sealed class Result
{
    public bool Success { get; }
    public string? Error { get; }

    private Result(bool success, string? error = null)
    {
        Success = success;
        Error = error;
    }

    public static Result Ok() => new(true);

    public static Result Fail(string error) => new(false, error);

    public static implicit operator bool(Result result) => result.Success;

    public override string ToString() => Success ? "Success" : $"Error: {Error}";
}
