namespace Movie.API.Infrastructure.Configuration;

public class CloudinaryOptions
{
    public const string SectionName = "Cloudinary";

    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string MoviePosterFolder { get; set; } = "cinema-system/movies/posters";
    public int MaxPosterSizeInMb { get; set; } = 10;
}
