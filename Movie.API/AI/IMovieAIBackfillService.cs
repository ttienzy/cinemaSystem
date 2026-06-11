using Movie.API.Client;

namespace Movie.API.AI;

public interface IMovieAIBackfillService
{
    Task<ApiResponse<MovieEmbeddingRebuildResponseDto>> RebuildMissingEmbeddingsAsync(
        int limit,
        CancellationToken cancellationToken = default);
}
