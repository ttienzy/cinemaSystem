namespace Application.Common.Exceptions
{
    /// <summary>Thrown when a conflict exists with current state. Maps to 409.</summary>
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
