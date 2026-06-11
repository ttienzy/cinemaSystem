namespace Movie.API.Client;

public static class MovieSearchTypes
{
    public const string Semantic = "Semantic";
    public const string KeywordFallback = "KeywordFallback";
}

public class MovieSearchResponseDto
{
    public string Query { get; set; } = string.Empty;
    public string SearchType { get; set; } = MovieSearchTypes.KeywordFallback;
    public List<MovieSearchResultDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class MovieSearchResultDto : MovieDto
{
    public double? Distance { get; set; }
    public double? SimilarityScore { get; set; }
}
