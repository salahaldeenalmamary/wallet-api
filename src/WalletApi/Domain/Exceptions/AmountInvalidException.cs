namespace WalletApi.Domain.Exceptions;

public class AmountInvalidException : Exception
{
    public AmountInvalidException(string message = "The specified amount is invalid. Amount must be a positive non-zero value.")
        : base(message) { }
}
