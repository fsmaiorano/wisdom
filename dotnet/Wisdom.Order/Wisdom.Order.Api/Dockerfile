FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                           "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Order/Wisdom.Order.Domain/Wisdom.Order.Domain.csproj",                                  "Wisdom.Order/Wisdom.Order.Domain/"]
COPY ["Wisdom.Order/Wisdom.Order.Application/Wisdom.Order.Application.csproj",                        "Wisdom.Order/Wisdom.Order.Application/"]
COPY ["Wisdom.Order/Wisdom.Order.Infrastructure/Wisdom.Order.Infrastructure.csproj",                  "Wisdom.Order/Wisdom.Order.Infrastructure/"]
COPY ["Wisdom.Order/Wisdom.Order.Infrastructure.Migration/Wisdom.Order.Infrastructure.Migration.csproj", "Wisdom.Order/Wisdom.Order.Infrastructure.Migration/"]
COPY ["Wisdom.Order/Wisdom.Order.Api/Wisdom.Order.Api.csproj",                                        "Wisdom.Order/Wisdom.Order.Api/"]

RUN dotnet restore "Wisdom.Order/Wisdom.Order.Api/Wisdom.Order.Api.csproj"

COPY . .

RUN find /src -name "*.csproj" -not -path "*/obj/*" | xargs -I{} dirname {} | sort -u | xargs -I{} mkdir -p {}/bin/Debug

RUN dotnet publish "Wisdom.Order/Wisdom.Order.Api/Wisdom.Order.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Order.Api.dll"]
