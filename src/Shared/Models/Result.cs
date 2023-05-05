namespace Shared.Models;

public class Result
{
    public Result(string code, string message)
    {
        ErrorCode = code;
        ErrorMessage = message;
    }

    public bool IsOK => string.IsNullOrEmpty(ErrorCode) || ErrorCode == "0";
    
    public string ErrorCode { get; }

    public string ErrorMessage { get; }
}