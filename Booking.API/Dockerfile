# ============================================
# Stage 1: BUILD
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["Booking.API/Booking.API.csproj", "Booking.API/"]
COPY ["Cinema.Shared/Cinema.Shared.csproj", "Cinema.Shared/"]
COPY ["Cinema.EventBus/Cinema.EventBus.csproj", "Cinema.EventBus/"]
COPY ["Cinema.EventBusRabbitMQ/Cinema.EventBusRabbitMQ.csproj", "Cinema.EventBusRabbitMQ/"]

# Restore dependencies
RUN dotnet restore "Booking.API/Booking.API.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/Booking.API"
RUN dotnet build "Booking.API.csproj" -c Release -o /app/build

# Publish the project
RUN dotnet publish "Booking.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
# Serves both REST API and SignalR WebSocket on same port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Entry point
ENTRYPOINT ["dotnet", "Booking.API.dll"]