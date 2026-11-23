FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                           "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.MediaWorker/Wisdom.MediaWorker.Application/Wisdom.MediaWorker.Application.csproj",     "Wisdom.MediaWorker/Wisdom.MediaWorker.Application/"]
COPY ["Wisdom.MediaWorker/Wisdom.MediaWorker.Infrastructure/Wisdom.MediaWorker.Infrastructure.csproj", "Wisdom.MediaWorker/Wisdom.MediaWorker.Infrastructure/"]
COPY ["Wisdom.MediaWorker/Wisdom.MediaWorker.Worker/Wisdom.MediaWorker.Worker.csproj",               "Wisdom.MediaWorker/Wisdom.MediaWorker.Worker/"]

RUN dotnet restore "Wisdom.MediaWorker/Wisdom.MediaWorker.Worker/Wisdom.MediaWorker.Worker.csproj"

COPY . .

RUN dotnet publish "Wisdom.MediaWorker/Wisdom.MediaWorker.Worker/Wisdom.MediaWorker.Worker.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends ffmpeg \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.MediaWorker.Worker.dll"]
