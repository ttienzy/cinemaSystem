using Microsoft.AspNetCore.Http;

namespace Movie.API.Application.Services;

public interface IFileStorageService
{
    Task<StoredFileResult> UploadMoviePosterAsync(IFormFile file, CancellationToken cancellationToken = default);
}

public sealed record StoredFileResult(string Url);
