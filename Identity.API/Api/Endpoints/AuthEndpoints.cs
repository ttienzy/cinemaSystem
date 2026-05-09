using Cinema.Shared.Extensions;
using Cinema.Shared.Helpers;
using Cinema.Shared.Models;
using Identity.API.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Identity.API.Api.Endpoints;

public static class AuthEndpoints
{
    
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost("/register", Register)
            .WithName("Register")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithOpenApi();

        group.MapPost("/refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .WithOpenApi();

        group.MapPost("/revoke-token", RevokeToken)
            .RequireAuthorization()
            .WithName("RevokeToken")
            .WithOpenApi();

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithOpenApi();

        group.MapGet("/test", TestAuth)
            .WithName("TestAuth")
            .WithOpenApi();
    }

    private static async Task<IResult> Register(RegisterRequest request, IAuthService authService, HttpContext context)
    {
        var response = await authService.RegisterAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> Login(LoginRequest request, IAuthService authService, HttpContext context)
    {
        var response = await authService.LoginAsync(request);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> RefreshToken(RefreshTokenRequest request, IAuthService authService, HttpContext context)
    {
        var response = await authService.RefreshTokenAsync(request.RefreshToken);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> RevokeToken(RefreshTokenRequest request, IAuthService authService, HttpContext context)
    {
        var response = await authService.RevokeTokenAsync(request.RefreshToken);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static async Task<IResult> GetCurrentUser(ClaimsPrincipal user, IAuthService authService, HttpContext context)

    {
        var x = context.Request.Headers["Authorization"].ToString();
        var userId = JwtHelper.GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            var unauthorizedResponse = ApiResponse<UserInfoResponse>.UnauthorizedResponse();
            unauthorizedResponse.SetTraceId(context);
            return unauthorizedResponse.ToResult();
        }
        //var userId = "a53a4bc3-8fa0-4e8e-9048-a2c99e6a48ce";

        var response = await authService.GetUserInfoAsync(userId);
        response.SetTraceId(context);
        return response.ToResult();
    }

    private static IResult TestAuth(ClaimsPrincipal user)
    {
        return Results.Ok(new
        {
            message = "Hello World!",
        });
    }
}


