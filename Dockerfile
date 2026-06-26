# Use .NET 9 SDK as build environment
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files first for better layer caching
COPY ["PRN232.GradingEngine.Api/PRN232.GradingEngine.Api.csproj", "PRN232.GradingEngine.Api/"]
COPY ["PRN232.GradingEngine.Application/PRN232.GradingEngine.Application.csproj", "PRN232.GradingEngine.Application/"]
COPY ["PRN232.GradingEngine.Infrastructure/PRN232.GradingEngine.Infrastructure.csproj", "PRN232.GradingEngine.Infrastructure/"]
COPY ["PRN232.Domain/PRN232.Domain.csproj", "PRN232.Domain/"]

# Restore dependencies
RUN dotnet restore "PRN232.GradingEngine.Api/PRN232.GradingEngine.Api.csproj"

# Copy the rest of the source code
COPY . .

# Build and publish the API project
WORKDIR "/src/PRN232.GradingEngine.Api"
RUN dotnet publish "PRN232.GradingEngine.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use .NET 9 SDK as the runtime image because the Grading Engine runs student solutions 
# by spawning a child process running "dotnet build" which requires the full SDK.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose HTTP API port
EXPOSE 8080

ENTRYPOINT ["dotnet", "PRN232.GradingEngine.Api.dll"]
