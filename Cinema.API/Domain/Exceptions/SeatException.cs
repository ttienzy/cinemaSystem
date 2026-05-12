namespace Cinema.API.Domain.Exceptions;

public static class SeatException
{
    public const string SEAT_NOT_FOUND = "Seat not found";
    public const string VALIDATION_FAILED = "Validation failed";

    public const string SEAT_CREATED_SUCCESSFULLY = "Seat created successfully";
    public const string SEAT_UPDATED_SUCCESSFULLY = "Seat updated successfully";
    public const string SEAT_DELETED_SUCCESSFULLY = "Seat deleted successfully";

    public static (string Code, string Message, string Field) SEAT_EXISTS(string seatDisplayName)
        => ("SEAT_EXISTS", $"Seat {seatDisplayName} already exists in this hall", "Row,Number");

    public static (string Code, string Message, string Field) DUPLICATE_SEAT(string seatDisplayName)
        => ("DUPLICATE_SEAT", $"Duplicate seat {seatDisplayName} in request", "Seats");

    public static (string Code, string Message, string Field) NO_SEATS_TO_CREATE
        => ("NO_SEATS", "All seats already exist", "Seats");

    public static (string Code, string Message, string Field) NO_SEAT_IDS
        => ("NO_SEATS", "Seat IDs list is empty", "SeatIds");

    public const string SEAT_ALREADY_EXISTS = "Seat already exists";
    public const string SOME_SEATS_COULD_NOT_BE_CREATED = "Some seats could not be created";
    public const string NO_SEATS_TO_CREATE_MESSAGE = "No seats to create";
    public const string SEAT_POSITION_ALREADY_OCCUPIED = "Seat position already occupied";
    public const string NO_SEATS_TO_DELETE = "No seats to delete";
    public const string NO_SEATS_FOUND = "No seats found";
}
