namespace WalletApi.Domain.Exceptions;

public class BalanceIsEmptyException : Exception
{
    public BalanceIsEmptyException(string message = "The wallet balance is empty.")
        : base(message) { }
}
