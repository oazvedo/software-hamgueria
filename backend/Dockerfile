# API do Hamgueria (ADR 0023). Contexto de build: backend/.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so a code-only change reuses the package layer.
COPY src/SoftwareHamgueria.Domain/SoftwareHamgueria.Domain.csproj src/SoftwareHamgueria.Domain/
COPY src/SoftwareHamgueria.Application/SoftwareHamgueria.Application.csproj src/SoftwareHamgueria.Application/
COPY src/SoftwareHamgueria.Infrastructure/SoftwareHamgueria.Infrastructure.csproj src/SoftwareHamgueria.Infrastructure/
COPY src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj src/SoftwareHamgueria.API/
RUN dotnet restore src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj

COPY src/ src/
RUN dotnet publish src/SoftwareHamgueria.API/SoftwareHamgueria.API.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# Never Development here: that environment loads the dev credentials (ADR 0024).
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
ENTRYPOINT ["dotnet", "SoftwareHamgueria.API.dll"]
