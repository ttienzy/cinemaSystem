using System.Text.Json;
using Cinema.Shared.Models;

namespace Booking.API.Infrastructure.Integrations.Clients;

internal static class ApiResponseJsonHelper
{
    public static T? DeserializeApiResponse<T>(string content, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
        return apiResponse.Data;
    }
}
