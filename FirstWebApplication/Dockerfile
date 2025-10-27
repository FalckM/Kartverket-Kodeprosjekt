# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image med både .NET SDK og Node.js
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release

# Installer Node.js og npm (trengs for Tailwind CSS)
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Kopier csproj og restore .NET avhengigheter
COPY ["FirstWebApplication/FirstWebApplication.csproj", "FirstWebApplication/"]
RUN dotnet restore "FirstWebApplication/FirstWebApplication.csproj"

# Kopier package.json og installer npm-pakker
COPY ["FirstWebApplication/package.json", "FirstWebApplication/"]
WORKDIR "/src/FirstWebApplication"
RUN npm install

# Kopier resten av filene (inkludert Views for Tailwind scanning)
WORKDIR /src
COPY . .
WORKDIR "/src/FirstWebApplication"

# Bygg Tailwind CSS med eksplisitt content-scanning
RUN npx tailwindcss --input ./wwwroot/css/input.css --output ./wwwroot/css/site.css --minify --content './Views/**/*.cshtml' --content './Pages/**/*.cshtml'

# Bygg .NET-prosjektet
RUN dotnet build "FirstWebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish fase
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FirstWebApplication.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FirstWebApplication.dll"]