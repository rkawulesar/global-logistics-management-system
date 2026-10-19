FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["GLMS_Web/GLMS_Web.csproj", "GLMS_Web/"]

RUN dotnet restore "./GLMS_Web/GLMS_Web.csproj"

COPY . .

WORKDIR "/src/GLMS_Web"

RUN dotnet build "./GLMS_Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./GLMS_Web.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "GLMS_Web.dll"]