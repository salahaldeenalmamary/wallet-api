namespace WalletApi.Domain.Exceptions;

public class InsufficientFundsException : Exception
{
    public InsufficientFundsException(string message = "Insufficient funds in the wallet to complete this operation.")
        : base(message) { }
}
