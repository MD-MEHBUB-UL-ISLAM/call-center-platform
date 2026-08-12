# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first so NuGet restore is cached across builds
COPY CallCenterPlatform.sln .
COPY src/CallCenter.Domain/CallCenter.Domain.csproj src/CallCenter.Domain/
COPY src/CallCenter.Application/CallCenter.Application.csproj src/CallCenter.Application/
COPY src/CallCenter.Infrastructure/CallCenter.Infrastructure.csproj src/CallCenter.Infrastructure/
COPY src/CallCenter.Api/CallCenter.Api.csproj src/CallCenter.Api/
RUN dotnet restore CallCenterPlatform.sln

COPY . .
RUN dotnet publish src/CallCenter.Api/CallCenter.Api.csproj -c Release -o /app/publish --no-restore

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
# Railway sets PORT at runtime; Program.cs reads it and binds Kestrel accordingly.
ENTRYPOINT ["dotnet", "CallCenter.Api.dll"]
