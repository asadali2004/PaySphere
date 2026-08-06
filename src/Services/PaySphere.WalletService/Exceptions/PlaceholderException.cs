namespace PaySphere.WalletService.Exceptions;

internal sealed class PlaceholderException : System.Exception
{
    public PlaceholderException(string? message = null) : base(message) { }
}
