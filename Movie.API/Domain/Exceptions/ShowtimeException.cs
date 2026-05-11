namespace Movie.API.Domain.Exceptions;

public static class ShowtimeException
{
    public const string SHOWTIME_NOT_FOUND = "Showtime not found";
    public const string CINEMA_HALL_NOT_FOUND = "Cinema hall not found";
    public const string CINEMA_NOT_FOUND = "Cinema not found";
    public const string VALIDATION_FAILED = "Validation failed";

    // Success messages
    public const string SHOWTIME_CREATED_SUCCESSFULLY = "Showtime created successfully";
    public const string SHOWTIME_UPDATED_SUCCESSFULLY = "Showtime updated successfully";
    public const string SHOWTIME_DELETED_SUCCESSFULLY = "Showtime deleted successfully";

    // Error details (Tuples for ErrorDetail)
    public const string SHOWTIME_OVERLAP_MESSAGE = "Showtime overlaps with existing showtime in this cinema hall";
    public static (string Code, string Message, string Field) SHOWTIME_OVERLAP 
        => ("SHOWTIME_OVERLAP", "The selected time slot conflicts with another showtime", "StartTime");

    public const string CANNOT_DELETE_HAS_TICKETS = "Cannot delete a showtime that already has sold tickets";
    public static (string Code, string Message, string Field) SHOWTIME_HAS_BOOKINGS 
        => ("SHOWTIME_HAS_BOOKINGS", "This showtime already has sold tickets", "Id");

    public const string CANNOT_DELETE_PAST_SHOWTIME = "Cannot delete past showtimes";
    public static (string Code, string Message, string Field) PAST_SHOWTIME 
        => ("PAST_SHOWTIME", "This showtime has already started or passed", "Id");
}
