FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",     "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Bff/Wisdom.Bff/Wisdom.Bff.csproj",               "Wisdom.Bff/Wisdom.Bff/"]

RUN dotnet restore "Wisdom.Bff/Wisdom.Bff/Wisdom.Bff.csproj"

COPY . .

RUN dotnet publish "Wisdom.Bff/Wisdom.Bff/Wisdom.Bff.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Bff.dll"]
