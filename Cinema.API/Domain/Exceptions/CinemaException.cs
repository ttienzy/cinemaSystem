namespace Cinema.API.Domain.Exceptions;

public static class CinemaException
{
    public const string CINEMA_NOT_FOUND = "Cinema not found";
    public const string VALIDATION_FAILED = "Validation failed";

    public const string CINEMA_CREATED_SUCCESSFULLY = "Cinema created successfully";
    public const string CINEMA_UPDATED_SUCCESSFULLY = "Cinema updated successfully";
    public const string CINEMA_DELETED_SUCCESSFULLY = "Cinema deleted successfully";

    public static (string Code, string Message, string Field) INVALID_CINEMA_STATUS(string validStatuses)
        => ("INVALID_CINEMA_STATUS", $"Status must be one of: {validStatuses}", "status");

    public const string CANNOT_DELETE_CINEMA_HAS_HALLS = "Cannot delete cinema with existing halls";
    public static (string Code, string Message, string Field) CINEMA_HAS_HALLS
        => ("CINEMA_HAS_HALLS", "This cinema has cinema halls", "CinemaId");
}
