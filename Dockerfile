FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project files
COPY ["Udemy/Udemy.csproj", "Udemy/"]

# Restore dependencies
RUN dotnet restore "Udemy/Udemy.csproj"

# Copy application code
COPY . .

# Build application
WORKDIR "/src/Udemy"
RUN dotnet build "Udemy.csproj" -c Release -o /app/build

# Publish application
FROM build AS publish
RUN dotnet publish "Udemy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start application
ENTRYPOINT ["dotnet", "Udemy.dll"]
