namespace Movie.API.Domain.Exceptions;

public class GenreException
{
    public const string GENRE_NOT_FOUND = "Genre not found";
    public const string GENRE_NAME_ALREADY_EXISTS = "Genre name already exists";
    public const string GENRE_NAME_IS_REQUIRED = "Genre name is required";
    public const string GENRE_NAME_IS_TOO_SHORT = "Genre name is too short";
    public const string GENRE_NAME_IS_TOO_LONG = "Genre name is too long";
    public const string GENRE_NAME_IS_NOT_VALID = "Genre name is not valid";
    public const string GENRE_NAME_IS_EMPTY = "Genre name is empty";
    public const string GENRE_NAME_IS_NULL = "Genre name is null";
    public const string GENRE_NAME_IS_INVALID = "Genre name is invalid";

    public static (string, string, string) GENRE_INVALID_ID(string id) => ("Genre ID is invalid", $"Genre with ID '{id}' does not exist", "GenreIds");
    public static (string, string, string) GENRE_INVALID_RELEASE_DATE() => ("INVALID_RELEASE_DATE", "Release date cannot be more than 2 years in the future", "ReleaseDate");
    
    public const string GENRE_CANNOT_DELETE_HAS_MOVIES = "Genre cannot be deleted because it has movies";
    public static (string, string, string) GENRE_HAS_MOVIES = ("GENRE_HAS_MOVIES", GENRE_CANNOT_DELETE_HAS_MOVIES, "GenreId");


    //Successful
    public const string GENRE_CREATED_SUCCESSFULLY = "Genre created successfully";
    public const string GENRE_UPDATED_SUCCESSFULLY = "Genre updated successfully";
    public const string GENRE_DELETED_SUCCESSFULLY = "Genre deleted successfully";
}