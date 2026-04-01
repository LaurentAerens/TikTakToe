# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY src/TikTakToe.slnx ./
COPY src/TikTakToe/TikTakToe.csproj TikTakToe/
COPY src/TikTakToe.Tests/TikTakToe.Tests.csproj TikTakToe.Tests/

RUN dotnet restore TikTakToe.slnx

# Copy source
COPY src/TikTakToe/ TikTakToe/

# Build & publish
RUN dotnet publish TikTakToe/TikTakToe.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup appuser

COPY --from=build /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget -qO- http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "TikTakToe.dll"]
