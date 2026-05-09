namespace Cinema.API.Application.DTOs;

public static class CinemaStatuses
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly string[] All =
    [
        Active,
        Inactive
    ];
}

public class CinemaAdminOverviewDto : CinemaDto
{
    public List<CinemaHallDto> CinemaHalls { get; set; } = new();
}

public class CinemaAdminSummaryDto
{
    public int TotalCinemas { get; set; }
    public int ActiveCinemas { get; set; }
    public int InactiveCinemas { get; set; }
    public int TotalHalls { get; set; }
    public int TotalSeats { get; set; }
}
