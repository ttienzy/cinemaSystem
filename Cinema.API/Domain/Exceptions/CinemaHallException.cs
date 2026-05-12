namespace Cinema.API.Domain.Exceptions;

public static class CinemaHallException
{
    public const string CINEMA_HALL_NOT_FOUND = "Cinema hall not found";
    public const string VALIDATION_FAILED = "Validation failed";

    public const string CINEMA_HALL_CREATED_SUCCESSFULLY = "Cinema hall created successfully";
    public const string CINEMA_HALL_UPDATED_SUCCESSFULLY = "Cinema hall updated successfully";
    public const string CINEMA_HALL_DELETED_SUCCESSFULLY = "Cinema hall deleted successfully";

    public static (string Code, string Message, string Field) CINEMA_HALL_IDS_REQUIRED
        => ("CINEMA_HALL_IDS_REQUIRED", "At least one cinema hall id is required", "CinemaHallIds");

    public const string CANNOT_DELETE_CINEMA_HALL_WITH_SEATS = "Cannot delete cinema hall with existing seats";
    public static (string Code, string Message, string Field) HALL_HAS_SEATS
        => ("HALL_HAS_SEATS", "Please delete all seats before deleting the hall", "CinemaHallId");
}
