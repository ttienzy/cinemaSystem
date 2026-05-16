FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notification.API/Notification.API.csproj", "Notification.API/"]
COPY ["Cinema.Contracts/Cinema.Contracts.csproj", "Cinema.Contracts/"]
COPY ["Cinema.Shared/Cinema.Shared.csproj", "Cinema.Shared/"]
RUN dotnet restore "Notification.API/Notification.API.csproj"
COPY . .
WORKDIR "/src/Notification.API"
RUN dotnet build "Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification.API.dll"]
