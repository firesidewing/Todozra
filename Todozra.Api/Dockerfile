# Base runtime image for ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy and restore backend
COPY ["Todozra/Todozra.csproj", "Todozra/"]
RUN dotnet restore "Todozra/Todozra.csproj"

# Copy everything (backend + frontend source)
COPY . .
WORKDIR "/src/Todozra"
RUN dotnet build "./Todozra.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Backend publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Todozra.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Frontend build stage (Vue + Bun)
# Adjust the path "Todozra.Client" to your actual frontend folder name
FROM oven/bun:1 AS frontend-build
WORKDIR /src

# Copy frontend package files first for better layer caching
COPY Todozra.Client/package.json Todozra.Client/bun.lockb* ./Todozra.Client/

# Copy the rest of the frontend
COPY Todozra.Client ./Todozra.Client

WORKDIR /src/Todozra.Client
RUN bun install
RUN bun run build

# Final runtime image
FROM base AS final
WORKDIR /app

# Copy backend publish output
COPY --from=publish /app/publish .

# Copy frontend build output into wwwroot (served by ASP.NET)
# If your Vite outDir is different, adjust this path.
COPY --from=frontend-build /src/Todozra.Client/dist ./wwwroot

ENTRYPOINT ["dotnet", "Todozra.dll"]