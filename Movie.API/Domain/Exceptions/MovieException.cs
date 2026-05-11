using System.Reflection.Metadata;

namespace Movie.API.Domain.Exceptions;


public static class MovieException
{
    public const string MOVIE_NOT_FOUND = "Movie not found";
    public const string INVALID_MOVIE_STATUS = "Invalid movie status";
    public const string VALIDATION_FAILED = "Validation failed";

    // Success messages
    public const string MOVIE_CREATED_SUCCESSFULLY = "Movie created successfully";
    public const string MOVIE_UPDATED_SUCCESSFULLY = "Movie updated successfully";
    public const string MOVIE_DELETED_SUCCESSFULLY = "Movie deleted successfully";

    // Error details (Tuples for ErrorDetail)
    public static (string Code, string Message, string Field) MOVIE_INVALID_STATUS(string validStatuses) 
        => ("INVALID_MOVIE_STATUS", $"Status must be one of: {validStatuses}", "status");

    public const string CANNOT_DELETE_MOVIE_HAS_SHOWTIMES = "Cannot delete movie with existing showtimes";
    public static (string Code, string Message, string Field) MOVIE_HAS_SHOWTIMES 
        => ("MOVIE_HAS_SHOWTIMES", "This movie has scheduled showtimes", "MovieId");

    public static (string Code, string Message, string Field) INVALID_POSTER_FILE(string message) 
        => ("INVALID_POSTER_FILE", message, "PosterFile");

    public static (string Code, string Message, string Field) POSTER_UPLOAD_FAILED 
        => ("POSTER_UPLOAD_FAILED", "Poster upload failed.", "PosterFile");
}