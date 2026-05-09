using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Movie.API.Infrastructure.Storage;

public class CloudinaryFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly Cloudinary _cloudinary;
    private readonly CloudinaryOptions _options;

    public CloudinaryFileStorageService(Cloudinary cloudinary, IOptions<CloudinaryOptions> options)
    {
        _cloudinary = cloudinary;
        _options = options.Value;
    }

    public async Task<StoredFileResult> UploadMoviePosterAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        await using var stream = file.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = _options.MoviePosterFolder,
            PublicId = $"{CreateSlug(Path.GetFileNameWithoutExtension(file.FileName))}-{Guid.NewGuid():N}",
            Overwrite = false,
            UniqueFilename = false,
            UseFilename = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (uploadResult.Error is not null || uploadResult.SecureUrl is null)
        {
            throw new InvalidOperationException(uploadResult.Error?.Message ?? "Cloudinary upload did not return a secure URL.");
        }

        return new StoredFileResult(uploadResult.SecureUrl.AbsoluteUri);
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("Poster file cannot be empty.");
        }

        var maxBytes = _options.MaxPosterSizeInMb * 1024 * 1024L;
        if (file.Length > maxBytes)
        {
            throw new ArgumentException($"Poster file size cannot exceed {_options.MaxPosterSizeInMb} MB.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ArgumentException("Poster file must be a JPG, PNG, or WEBP image.");
        }
    }

    private static string CreateSlug(string fileName)
    {
        var normalizedFileName = fileName
            .Trim()
            .ToLowerInvariant();

        var sanitizedCharacters = normalizedFileName
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();

        var slug = string.Join(
            "-",
            new string(sanitizedCharacters)
                .Split('-', StringSplitOptions.RemoveEmptyEntries));

        return string.IsNullOrWhiteSpace(slug) ? "poster" : slug;
    }
}
