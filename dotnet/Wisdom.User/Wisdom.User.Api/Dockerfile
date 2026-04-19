FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                           "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.User/Wisdom.User.Domain/Wisdom.User.Domain.csproj",                                     "Wisdom.User/Wisdom.User.Domain/"]
COPY ["Wisdom.User/Wisdom.User.Application/Wisdom.User.Application.csproj",                           "Wisdom.User/Wisdom.User.Application/"]
COPY ["Wisdom.User/Wisdom.User.Infrastructure/Wisdom.User.Infrastructure.csproj",                     "Wisdom.User/Wisdom.User.Infrastructure/"]
COPY ["Wisdom.User/Wisdom.User.Infrastructure.Migration/Wisdom.User.Infrastructure.Migration.csproj", "Wisdom.User/Wisdom.User.Infrastructure.Migration/"]
COPY ["Wisdom.User/Wisdom.User.Api/Wisdom.User.Api.csproj",                                           "Wisdom.User/Wisdom.User.Api/"]

RUN dotnet restore "Wisdom.User/Wisdom.User.Api/Wisdom.User.Api.csproj"

COPY . .

RUN find /src -name "*.csproj" -not -path "*/obj/*" | xargs -I{} dirname {} | sort -u | xargs -I{} mkdir -p {}/bin/Debug

RUN dotnet publish "Wisdom.User/Wisdom.User.Api/Wisdom.User.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.User.Api.dll"]
