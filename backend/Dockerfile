# API do Hamgueria (ADR 0023). Contexto de build: backend/.
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:de4fe90a98845d75910b9769662465c84de1a934665c431279d1382b2bddc02b AS build
WORKDIR /src

# Restore first, so a code-only change reuses the package layer.
COPY src/SoftwareHamgueria.Domain/SoftwareHamgueria.Domain.csproj src/SoftwareHamgueria.Domain/
COPY src/SoftwareHamgueria.Application/SoftwareHamgueria.Application.csproj src/SoftwareHamgueria.Application/
COPY src/SoftwareHamgueria.Infrastructure/SoftwareHamgueria.Infrastructure.csproj src/SoftwareHamgueria.Infrastructure/
COPY src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj src/SoftwareHamgueria.API/
RUN dotnet restore src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj

COPY src/ src/
RUN dotnet publish src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:de0ee358cdd672f7017a003bed21e9ac9ed48a27c922a59b18175b6e82fa7fee AS runtime
WORKDIR /app
COPY --from=build /app .
# Never Development here: that environment loads the dev credentials (ADR 0024).
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "SoftwareHamgueria.API.dll"]
