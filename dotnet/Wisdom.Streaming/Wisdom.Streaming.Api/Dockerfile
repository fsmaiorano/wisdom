FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                       "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Streaming/Wisdom.Streaming.Application/Wisdom.Streaming.Application.csproj",       "Wisdom.Streaming/Wisdom.Streaming.Application/"]
COPY ["Wisdom.Streaming/Wisdom.Streaming.Infrastructure/Wisdom.Streaming.Infrastructure.csproj", "Wisdom.Streaming/Wisdom.Streaming.Infrastructure/"]
COPY ["Wisdom.Streaming/Wisdom.Streaming.Api/Wisdom.Streaming.Api.csproj",                       "Wisdom.Streaming/Wisdom.Streaming.Api/"]

RUN dotnet restore "Wisdom.Streaming/Wisdom.Streaming.Api/Wisdom.Streaming.Api.csproj"

COPY . .

RUN dotnet publish "Wisdom.Streaming/Wisdom.Streaming.Api/Wisdom.Streaming.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Streaming.Api.dll"]
