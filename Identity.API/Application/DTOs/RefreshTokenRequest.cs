using System.ComponentModel.DataAnnotations;

namespace Identity.API.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}


