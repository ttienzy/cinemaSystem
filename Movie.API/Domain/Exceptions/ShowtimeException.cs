namespace Movie.API.Domain.Exceptions;

public static class ShowtimeException
{
    public const string SHOWTIME_NOT_FOUND = "Showtime not found";
    public const string CINEMA_HALL_NOT_FOUND = "Cinema hall not found";
    public const string CINEMA_NOT_FOUND = "Cinema not found";
    public const string VALIDATION_FAILED = "Validation failed";
    public const string INTERNAL_SERVER_ERROR = "An unexpected error occurred while processing the showtime request";

    public const string SHOWTIME_CREATED_SUCCESSFULLY = "Showtime created successfully";
    public const string SHOWTIME_UPDATED_SUCCESSFULLY = "Showtime updated successfully";
    public const string SHOWTIME_DELETED_SUCCESSFULLY = "Showtime deleted successfully";

    public const string SHOWTIME_OVERLAP_MESSAGE = "Showtime overlaps with existing showtime in this cinema hall";
    public static (string Code, string Message, string Field) SHOWTIME_OVERLAP
        => ("SHOWTIME_OVERLAP", "The selected time slot conflicts with another showtime", "StartTime");

    public const string CANNOT_UPDATE_HAS_TICKETS = "Cannot reschedule a showtime that already has sold tickets";
    public const string CANNOT_DELETE_HAS_TICKETS = "Cannot delete a showtime that already has sold tickets";
    public static (string Code, string Message, string Field) SHOWTIME_HAS_BOOKINGS
        => ("SHOWTIME_HAS_BOOKINGS", "This showtime already has sold tickets", "Id");

    public const string CANNOT_UPDATE_PAST_SHOWTIME = "Cannot update past showtimes";
    public const string CANNOT_DELETE_PAST_SHOWTIME = "Cannot delete past showtimes";
    public static (string Code, string Message, string Field) PAST_SHOWTIME
        => ("PAST_SHOWTIME", "This showtime has already started or passed", "Id");

    public static (string Code, string Message, string Field) INVALID_START_TIME
        => ("INVALID_START_TIME", "Showtime must be scheduled at least 1 hour in advance", "StartTime");

    public static (string Code, string Message, string Field) START_TIME_MUST_BE_AFTER_TODAY
        => ("START_TIME_MUST_BE_AFTER_TODAY", "Showtime date must be later than today in local operating time", "StartTime");

    public static (string Code, string Message, string Field) START_TIME_BEFORE_OPENING_HOUR
        => ("START_TIME_BEFORE_OPENING_HOUR", "Showtime must start after 08:00 AM local operating time", "StartTime");

    public static (string Code, string Message, string Field) INVALID_CINEMA_HALL
        => ("INVALID_CINEMA_HALL", "Cinema hall does not exist", "CinemaHallId");

    public static (string Code, string Message, string Field) SHOWTIME_IDS_REQUIRED
        => ("SHOWTIME_IDS_REQUIRED", "At least one showtime id is required", "ShowtimeIds");

    public static (string Code, string Message, string Field) INVALID_TIME_RANGE
        => ("INVALID_TIME_RANGE", "'from' must be earlier than 'to'.", "from");
}
