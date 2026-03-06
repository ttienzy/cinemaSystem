namespace Shared.Models.DataModels.DashboardDtos
{
    /// <summary>
    /// Dashboard top movie DTO — statistics by revenue and tickets sold.
    /// </summary>
    public class TopMovieDto
    {
        /// <summary>Movie ID.</summary>
        public Guid MovieId { get; set; }

        /// <summary>Movie Title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Poster URL.</summary>
        public string? PosterUrl { get; set; }

        /// <summary>Total revenue from this movie.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Total tickets sold.</summary>
        public int TotalTicketsSold { get; set; }

        /// <summary>Total number of showtimes.</summary>
        public int ShowtimeCount { get; set; }
    }
}
