# ============================================
# Stage 1: BUILD
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["Cinema.API/Cinema.API.csproj", "Cinema.API/"]
COPY ["Cinema.Shared/Cinema.Shared.csproj", "Cinema.Shared/"]

# Restore dependencies
RUN dotnet restore "Cinema.API/Cinema.API.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/Cinema.API"
RUN dotnet build "Cinema.API.csproj" -c Release -o /app/build

# Publish the project
RUN dotnet publish "Cinema.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Stage 2: RUNTIME
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Switch to non-root user "app" (built-in .NET 8)
USER app

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose port 8080 (default for .NET 8)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Entry point
ENTRYPOINT ["dotnet", "Cinema.API.dll"]