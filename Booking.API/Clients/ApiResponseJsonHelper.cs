using System.Text.Json;
using Booking.API.Client;

namespace Booking.API.Clients;

internal static class ApiResponseJsonHelper
{
    public static T? DeserializeApiResponse<T>(string content, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
        return apiResponse is null ? default : apiResponse.Data;
    }
}
