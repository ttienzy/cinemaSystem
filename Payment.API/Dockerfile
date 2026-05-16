# ============================================
# Stage 1: BUILD
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Payment.API/Payment.API.csproj", "Payment.API/"]
COPY ["Cinema.Shared/Cinema.Shared.csproj", "Cinema.Shared/"]
COPY ["Cinema.Contracts/Cinema.Contracts.csproj", "Cinema.Contracts/"]

# Restore dependencies
RUN dotnet restore "Payment.API/Payment.API.csproj"

# Copy source code
COPY . .
WORKDIR "/src/Payment.API"

# Build
RUN dotnet build "Payment.API.csproj" -c Release -o /app/build

# Publish the project
RUN dotnet publish "Payment.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

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

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

# Entry point
ENTRYPOINT ["dotnet", "Payment.API.dll"]
