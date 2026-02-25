namespace Application.Common.Exceptions
{
    /// <summary>Thrown when the caller lacks permission. Maps to 403.</summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("You do not have permission to perform this action.") { }
        public ForbiddenException(string message) : base(message) { }
    }
}
