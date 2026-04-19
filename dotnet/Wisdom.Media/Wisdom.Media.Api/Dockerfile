FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                           "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Media/Wisdom.Media.Domain/Wisdom.Media.Domain.csproj",                                  "Wisdom.Media/Wisdom.Media.Domain/"]
COPY ["Wisdom.Media/Wisdom.Media.Application/Wisdom.Media.Application.csproj",                        "Wisdom.Media/Wisdom.Media.Application/"]
COPY ["Wisdom.Media/Wisdom.Media.Infrastructure/Wisdom.Media.Infrastructure.csproj",                  "Wisdom.Media/Wisdom.Media.Infrastructure/"]
COPY ["Wisdom.Media/Wisdom.Media.Infrastructure.Migration/Wisdom.Media.Infrastructure.Migration.csproj", "Wisdom.Media/Wisdom.Media.Infrastructure.Migration/"]
COPY ["Wisdom.Media/Wisdom.Media.Api/Wisdom.Media.Api.csproj",                                        "Wisdom.Media/Wisdom.Media.Api/"]

RUN dotnet restore "Wisdom.Media/Wisdom.Media.Api/Wisdom.Media.Api.csproj"

COPY . .

RUN find /src -name "*.csproj" -not -path "*/obj/*" | xargs -I{} dirname {} | sort -u | xargs -I{} mkdir -p {}/bin/Debug

RUN dotnet publish "Wisdom.Media/Wisdom.Media.Api/Wisdom.Media.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Media.Api.dll"]
