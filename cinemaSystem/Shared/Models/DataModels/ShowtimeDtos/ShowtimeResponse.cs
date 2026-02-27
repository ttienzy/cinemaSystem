namespace Shared.Models.DataModels.ShowtimeDtos
{
    /// <summary>
    /// Backward-compatibility alias for ShowtimeDetailResponse.
    /// Use ShowtimeDetailResponse for new code; this ensures existing controllers compile.
    /// </summary>
    public class ShowtimeResponse : ShowtimeDetailResponse { }
}
