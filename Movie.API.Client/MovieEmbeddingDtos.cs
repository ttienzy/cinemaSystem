namespace Movie.API.Client;

public class MovieEmbeddingRebuildResponseDto
{
    public int RequestedLimit { get; set; }
    public int Candidates { get; set; }
    public int Processed { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public int RemainingWithoutEmbedding { get; set; }
}
