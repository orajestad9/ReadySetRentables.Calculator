# syntax=docker/dockerfile:1.6

# ===========================
# Build stage (.NET 10)
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Copy only the files that affect restore first
COPY ./src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj \
     ./src/ReadySetRentables.Calculator.Api/

# If you have these at repo root, copy them too (keeps restore reliable + cache-friendly)
# COPY Directory.Build.props ./
# COPY Directory.Build.targets ./
# COPY NuGet.Config ./
# COPY packages.lock.json ./

# 2) Restore with a cached NuGet folder (BuildKit)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore ./src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj

# 3) Now copy the rest of the source
COPY . .

# 4) Publish (also cache NuGet)
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish ./src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj \
      -c Release \
      -o /app/publish \
      /p:UseAppHost=false

# ===========================
# Runtime stage (.NET 10)
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ReadySetRentables.Calculator.Api.dll"]
