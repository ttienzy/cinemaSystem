namespace Application.Common.Exceptions
{
    /// <summary>
    /// Thrown when a database concurrency conflict occurs.
    /// Maps to 409 Conflict.
    /// </summary>
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException() 
            : base("The record has been modified or deleted by another user. Please reload the data.") { }

        public ConcurrencyException(string message) : base(message) { }
    }
}
