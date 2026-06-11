using Pgvector;

namespace Movie.API.AI;

public interface IMovieAIService
{
    bool IsEnabled { get; }

    string BuildMovieEmbeddingInput(string title, string? description);

    Task<Vector?> GenerateMovieEmbeddingAsync(
        string title,
        string? description,
        CancellationToken cancellationToken = default);

    Task<Vector?> GenerateTextEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default);
}
