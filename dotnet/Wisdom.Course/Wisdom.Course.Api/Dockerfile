FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                           "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Course/Wisdom.Course.Domain/Wisdom.Course.Domain.csproj",                               "Wisdom.Course/Wisdom.Course.Domain/"]
COPY ["Wisdom.Course/Wisdom.Course.Application/Wisdom.Course.Application.csproj",                     "Wisdom.Course/Wisdom.Course.Application/"]
COPY ["Wisdom.Course/Wisdom.Course.Infrastructure/Wisdom.Course.Infrastructure.csproj",               "Wisdom.Course/Wisdom.Course.Infrastructure/"]
COPY ["Wisdom.Course/Wisdom.Course.Infrastructure.Migration/Wisdom.Course.Infrastructure.Migration.csproj", "Wisdom.Course/Wisdom.Course.Infrastructure.Migration/"]
COPY ["Wisdom.Course/Wisdom.Course.Api/Wisdom.Course.Api.csproj",                                     "Wisdom.Course/Wisdom.Course.Api/"]
# Course API embeds Media use cases for lesson video upload
COPY ["Wisdom.Media/Wisdom.Media.Domain/Wisdom.Media.Domain.csproj",                                  "Wisdom.Media/Wisdom.Media.Domain/"]
COPY ["Wisdom.Media/Wisdom.Media.Application/Wisdom.Media.Application.csproj",                        "Wisdom.Media/Wisdom.Media.Application/"]
COPY ["Wisdom.Media/Wisdom.Media.Infrastructure/Wisdom.Media.Infrastructure.csproj",                  "Wisdom.Media/Wisdom.Media.Infrastructure/"]

RUN dotnet restore "Wisdom.Course/Wisdom.Course.Api/Wisdom.Course.Api.csproj"

COPY . .

RUN find /src -name "*.csproj" -not -path "*/obj/*" | xargs -I{} dirname {} | sort -u | xargs -I{} mkdir -p {}/bin/Debug

RUN dotnet publish "Wisdom.Course/Wisdom.Course.Api/Wisdom.Course.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Course.Api.dll"]
