namespace Domain.Common
{
    /// <summary>
    /// Represents a business rule violation in the domain layer.
    /// Should be caught at the Application layer and translated to 400/422 responses.
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }

        public DomainException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
