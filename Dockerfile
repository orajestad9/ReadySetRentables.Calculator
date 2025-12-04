# ===========================
# Build stage (.NET 10)
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything into the build context
COPY . .

# Restore only the API project
RUN dotnet restore ./src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj

# Build and publish the API project
RUN dotnet publish ./src/ReadySetRentables.Calculator.Api/ReadySetRentables.Calculator.Api.csproj \
    -c Release \
    -o /app/publish

# ===========================
# Runtime stage (.NET 10)
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Assembly name should match the project name
ENTRYPOINT ["dotnet", "ReadySetRentables.Calculator.Api.dll"]
