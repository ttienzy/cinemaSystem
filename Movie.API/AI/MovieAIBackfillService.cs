using Microsoft.EntityFrameworkCore;
using Movie.API.Client;
using Movie.API.Data;

namespace Movie.API.AI;

public class MovieAIBackfillService : IMovieAIBackfillService
{
    private const int MaxLimit = 200;

    private readonly MovieDbContext _context;
    private readonly IMovieAIService _movieAIService;
    private readonly ILogger<MovieAIBackfillService> _logger;

    public MovieAIBackfillService(
        MovieDbContext context,
        IMovieAIService movieAIService,
        ILogger<MovieAIBackfillService> logger)
    {
        _context = context;
        _movieAIService = movieAIService;
        _logger = logger;
    }

    public async Task<ApiResponse<MovieEmbeddingRebuildResponseDto>> RebuildMissingEmbeddingsAsync(
        int limit,
        CancellationToken cancellationToken = default)
    {
        var normalizedLimit = Math.Clamp(limit, 1, MaxLimit);
        var response = new MovieEmbeddingRebuildResponseDto
        {
            RequestedLimit = normalizedLimit
        };

        if (!_movieAIService.IsEnabled)
        {
            response.RemainingWithoutEmbedding = await CountMissingEmbeddingsAsync(cancellationToken);
            return ApiResponse<MovieEmbeddingRebuildResponseDto>.SuccessResponse(
                response,
                "Movie AI embedding rebuild skipped because OpenAI configuration is incomplete.");
        }

        var movies = await _context.Movies
            .Where(movie => movie.Embedding == null)
            .OrderByDescending(movie => movie.ReleaseDate)
            .Take(normalizedLimit)
            .ToListAsync(cancellationToken);

        response.Candidates = movies.Count;

        foreach (var movie in movies)
        {
            cancellationToken.ThrowIfCancellationRequested();
            response.Processed++;

            try
            {
                var embedding = await _movieAIService.GenerateMovieEmbeddingAsync(
                    movie.Title,
                    movie.Description,
                    cancellationToken);

                if (embedding is null)
                {
                    response.Failed++;
                    continue;
                }

                movie.SetEmbedding(embedding);
                response.Updated++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                response.Failed++;
                _logger.LogWarning(
                    exception,
                    "Movie AI embedding rebuild failed for movie {MovieId}.",
                    movie.Id);
            }
        }

        if (response.Updated > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        response.RemainingWithoutEmbedding = await CountMissingEmbeddingsAsync(cancellationToken);
        return ApiResponse<MovieEmbeddingRebuildResponseDto>.SuccessResponse(response);
    }

    private Task<int> CountMissingEmbeddingsAsync(CancellationToken cancellationToken)
    {
        return _context.Movies
            .Where(movie => movie.Embedding == null)
            .CountAsync(cancellationToken);
    }
}
