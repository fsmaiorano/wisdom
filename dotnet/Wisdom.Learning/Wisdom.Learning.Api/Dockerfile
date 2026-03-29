FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Wisdom.BuildingBlocks/Wisdom.BuildingBlocks.csproj",                                                   "Wisdom.BuildingBlocks/"]
COPY ["Wisdom.Learning/Wisdom.Learning.Domain/Wisdom.Learning.Domain.csproj",                                 "Wisdom.Learning/Wisdom.Learning.Domain/"]
COPY ["Wisdom.Learning/Wisdom.Learning.Application/Wisdom.Learning.Application.csproj",                       "Wisdom.Learning/Wisdom.Learning.Application/"]
COPY ["Wisdom.Learning/Wisdom.Learning.Infrastructure/Wisdom.Learning.Infrastructure.csproj",                 "Wisdom.Learning/Wisdom.Learning.Infrastructure/"]
COPY ["Wisdom.Learning/Wisdom.Learning.Infrastructure.Migration/Wisdom.Learning.Infrastructure.Migration.csproj", "Wisdom.Learning/Wisdom.Learning.Infrastructure.Migration/"]
COPY ["Wisdom.Learning/Wisdom.Learning.Api/Wisdom.Learning.Api.csproj",                                       "Wisdom.Learning/Wisdom.Learning.Api/"]

RUN dotnet restore "Wisdom.Learning/Wisdom.Learning.Api/Wisdom.Learning.Api.csproj"

COPY . .

RUN dotnet publish "Wisdom.Learning/Wisdom.Learning.Api/Wisdom.Learning.Api.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Wisdom.Learning.Api.dll"]
