namespace Payment.API.Domain.Exceptions;

public class SePayException : Exception
{
    public SePayException(string message) : base(message) { }

    public SePayException(string message, Exception innerException)
        : base(message, innerException) { }
}


