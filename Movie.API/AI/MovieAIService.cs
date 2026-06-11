using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using Pgvector;

namespace Movie.API.AI;

public class MovieAIService : IMovieAIService
{
    private readonly MovieAIOptions _options;
    private readonly ILogger<MovieAIService> _logger;

    public MovieAIService(
        IOptions<MovieAIOptions> options,
        ILogger<MovieAIService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !string.IsNullOrWhiteSpace(_options.EmbeddingModel) &&
        _options.EmbeddingDimensions > 0;

    public string BuildMovieEmbeddingInput(string title, string? description)
    {
        return NormalizeEmbeddingInput($"{title}. {description}");
    }

    public Task<Vector?> GenerateMovieEmbeddingAsync(
        string title,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var input = BuildMovieEmbeddingInput(title, description);
        return GenerateTextEmbeddingAsync(input, cancellationToken);
    }

    public async Task<Vector?> GenerateTextEmbeddingAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        var normalizedInput = NormalizeEmbeddingInput(input);
        if (string.IsNullOrWhiteSpace(normalizedInput))
        {
            return null;
        }

        if (!IsEnabled)
        {
            _logger.LogDebug("Movie AI embedding generation skipped because OpenAI configuration is incomplete.");
            return null;
        }

        try
        {
            var client = new EmbeddingClient(_options.EmbeddingModel, _options.ApiKey);
            var generationOptions = new EmbeddingGenerationOptions
            {
                Dimensions = _options.EmbeddingDimensions
            };

            var response = await client.GenerateEmbeddingAsync(
                normalizedInput,
                generationOptions,
                cancellationToken);

            var values = response.Value.ToFloats().ToArray();
            if (values.Length != _options.EmbeddingDimensions)
            {
                _logger.LogWarning(
                    "OpenAI returned an embedding with {ActualDimensions} dimensions, but Movie.API expected {ExpectedDimensions}.",
                    values.Length,
                    _options.EmbeddingDimensions);

                return null;
            }

            return new Vector(values);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Movie AI embedding generation failed for model {EmbeddingModel}.",
                _options.EmbeddingModel);

            return null;
        }
    }

    private static string NormalizeEmbeddingInput(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(
            ' ',
            value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
