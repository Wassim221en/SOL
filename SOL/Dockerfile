# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY FPS.sln ./
COPY FPS.API/FPS.API.csproj FPS.API/
COPY FPS.Application/FPS.Application.csproj FPS.Application/
COPY FPS.Domain/FPS.Domain.csproj FPS.Domain/
COPY FPS.Infrastructe/FPS.Infrastructe.csproj FPS.Infrastructe/
COPY FPS.Persistence/FPS.Persistence.csproj FPS.Persistence/
COPY FPS.IntegrationTests/FPS.IntegrationTests.csproj FPS.IntegrationTests/
COPY SignalR/SignalR.csproj SignalR/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build the application
WORKDIR /src/FPS.API
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y \
    curl \
    wget \
    apt-transport-https \
    software-properties-common \
    gnupg \
    && rm -rf /var/lib/apt/lists/*


RUN wget -q https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb \
    && dpkg -i packages-microsoft-prod.deb \
    && rm packages-microsoft-prod.deb

RUN apt-get update && apt-get install -y powershell \
    && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=publish /app/publish .
RUN pwsh playwright.ps1 install --with-deps
RUN apt-get update && apt-get install -y ffmpeg
# Expose port
EXPOSE 5487

# Set entry point
ENTRYPOINT ["dotnet", "FPS.API.dll"]
