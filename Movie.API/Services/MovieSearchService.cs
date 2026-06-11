using Microsoft.EntityFrameworkCore;
using Movie.API.AI;
using Movie.API.Client;
using Movie.API.Data;
using Movie.API.Mappers;
using Pgvector.EntityFrameworkCore;
using MovieEntity = Movie.API.Entities.Movie;

namespace Movie.API.Services;

public class MovieSearchService : IMovieSearchService
{
    private const int MaxPageSize = 50;

    private readonly MovieDbContext _context;
    private readonly IMovieAIService _movieAIService;
    private readonly ILogger<MovieSearchService> _logger;

    public MovieSearchService(
        MovieDbContext context,
        IMovieAIService movieAIService,
        ILogger<MovieSearchService> logger)
    {
        _context = context;
        _movieAIService = movieAIService;
        _logger = logger;
    }

    public async Task<ApiResponse<MovieSearchResponseDto>> SearchAsync(
        string? query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery = NormalizeQuery(query);
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return ApiResponse<MovieSearchResponseDto>.SuccessResponse(
                CreateResponse(normalizedQuery, MovieSearchTypes.KeywordFallback, [], 0, normalizedPageNumber, normalizedPageSize));
        }

        var queryEmbedding = await _movieAIService.GenerateTextEmbeddingAsync(normalizedQuery, cancellationToken);
        if (queryEmbedding is not null)
        {
            var semanticResponse = await TrySemanticSearchAsync(
                normalizedQuery,
                queryEmbedding,
                normalizedPageNumber,
                normalizedPageSize,
                cancellationToken);

            if (semanticResponse is not null)
            {
                return ApiResponse<MovieSearchResponseDto>.SuccessResponse(semanticResponse);
            }
        }

        var keywordResponse = await KeywordSearchAsync(
            normalizedQuery,
            normalizedPageNumber,
            normalizedPageSize,
            cancellationToken);

        return ApiResponse<MovieSearchResponseDto>.SuccessResponse(keywordResponse);
    }

    private async Task<MovieSearchResponseDto?> TrySemanticSearchAsync(
        string query,
        Pgvector.Vector queryEmbedding,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await _context.Movies
            .AsNoTracking()
            .Where(movie => movie.Embedding != null)
            .CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            _logger.LogDebug("Movie semantic search fell back to keyword search because no movies have embeddings.");
            return null;
        }

        var offset = (pageNumber - 1) * pageSize;
        var rankedRows = await _context.Movies
            .AsNoTracking()
            .Where(movie => movie.Embedding != null)
            .Select(movie => new
            {
                movie.Id,
                Distance = movie.Embedding!.CosineDistance(queryEmbedding)
            })
            .OrderBy(row => row.Distance)
            .Skip(offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (rankedRows.Count == 0)
        {
            return CreateResponse(query, MovieSearchTypes.Semantic, [], totalCount, pageNumber, pageSize);
        }

        var rankedIds = rankedRows.Select(row => row.Id).ToHashSet();
        var movies = await _context.Movies
            .AsNoTracking()
            .Include(movie => movie.MovieGenres)
            .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.Showtimes)
            .Where(movie => rankedIds.Contains(movie.Id))
            .ToListAsync(cancellationToken);

        var moviesById = movies.ToDictionary(movie => movie.Id);
        var now = DateTime.UtcNow;
        var results = rankedRows
            .Where(row => moviesById.ContainsKey(row.Id))
            .Select(row => moviesById[row.Id].MovieMapToSearchResultDto(now, row.Distance))
            .ToList();

        return CreateResponse(query, MovieSearchTypes.Semantic, results, totalCount, pageNumber, pageSize);
    }

    private async Task<MovieSearchResponseDto> KeywordSearchAsync(
        string query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var pattern = $"%{EscapeLikePattern(query)}%";
        var moviesQuery = _context.Movies
            .AsNoTracking()
            .Where(movie =>
                EF.Functions.ILike(movie.Title, pattern, "\\") ||
                (movie.Description != null && EF.Functions.ILike(movie.Description, pattern, "\\")));

        var totalCount = await moviesQuery.CountAsync(cancellationToken);
        var movies = await moviesQuery
            .Include(movie => movie.MovieGenres)
            .ThenInclude(movieGenre => movieGenre.Genre)
            .Include(movie => movie.Showtimes)
            .OrderByDescending(movie => movie.ReleaseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var results = movies
            .Select(movie => movie.MovieMapToSearchResultDto(now, null))
            .ToList();

        return CreateResponse(query, MovieSearchTypes.KeywordFallback, results, totalCount, pageNumber, pageSize);
    }

    private static MovieSearchResponseDto CreateResponse(
        string query,
        string searchType,
        List<MovieSearchResultDto> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new MovieSearchResponseDto
        {
            Query = query,
            SearchType = searchType,
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static string NormalizeQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static string EscapeLikePattern(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }
}
