namespace Movie.API.Infrastructure.Configuration;

public class SchedulingOptions
{
    public const string SectionName = "Scheduling";

    public int CleaningBufferMinutes { get; set; } = 20;
    public int TimelineStartHour { get; set; } = 8;
    public int TimelineEndHourNextDay { get; set; } = 2;
}
