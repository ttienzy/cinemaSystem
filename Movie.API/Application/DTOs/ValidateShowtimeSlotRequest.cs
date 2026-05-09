using System.ComponentModel.DataAnnotations;

namespace Movie.API.Application.DTOs;

public class ValidateShowtimeSlotRequest
{
    [Required]
    public Guid MovieId { get; set; }

    [Required]
    public Guid CinemaHallId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public Guid? ExcludeShowtimeId { get; set; }
}

public class ShowtimeConflictValidationResponse
{
    public bool IsAvailable { get; set; }
    public Guid MovieId { get; set; }
    public Guid CinemaHallId { get; set; }
    public DateTime ProposedStartTime { get; set; }
    public DateTime ProposedEndTime { get; set; }
    public DateTime ProposedCleaningEnd { get; set; }
    public int CleaningBufferMinutes { get; set; }
    public List<ShowtimeConflictItemDto> Conflicts { get; set; } = new();
}

public class ShowtimeConflictItemDto
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime CleaningEndTime { get; set; }
}
