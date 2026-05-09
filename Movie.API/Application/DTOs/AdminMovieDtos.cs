namespace Movie.API.Application.DTOs;

public static class MovieStatuses
{
    public const string Showing = "Showing";
    public const string ComingSoon = "ComingSoon";
    public const string Archived = "Archived";

    public static readonly string[] All =
    [
        Showing,
        ComingSoon,
        Archived
    ];
}

public class MovieAdminListItemDto : MovieDto
{
    public int TotalShowtimes { get; set; }
    public int UpcomingShowtimesCount { get; set; }
    public DateTime? NextShowtimeAt { get; set; }
    public DateTime? LastShowtimeAt { get; set; }
}

public class MovieAdminSummaryDto
{
    public int TotalMovies { get; set; }
    public int ShowingMovies { get; set; }
    public int ComingSoonMovies { get; set; }
    public int ArchivedMovies { get; set; }
}
