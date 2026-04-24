FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Directorios con permisos correctos antes de cambiar al usuario no-root
RUN mkdir -p /app/documentos-electronicos/activos \
    && mkdir -p /app/documentos-electronicos/archivados \
    && mkdir -p /app/Certificados \
    && chown -R $APP_UID:$APP_UID /app/documentos-electronicos \
    && chown -R $APP_UID:$APP_UID /app/Certificados

USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/FacturaElectronica/FacturaElectronica.WebApi/FacturaElectronica.WebApi.csproj", "src/FacturaElectronica/FacturaElectronica.WebApi/"]
COPY ["src/FacturaElectronica/FacturaElectronica.Aplicacion/FacturaElectronica.Aplicacion.csproj", "src/FacturaElectronica/FacturaElectronica.Aplicacion/"]
COPY ["src/FacturaElectronica/FacturaElectronica.Dominio/FacturaElectronica.Dominio.csproj", "src/FacturaElectronica/FacturaElectronica.Dominio/"]
COPY ["src/FacturaElectronica/FacturaElectronica.Infraestructura/FacturaElectronica.Infraestructura.csproj", "src/FacturaElectronica/FacturaElectronica.Infraestructura/"]
COPY ["src/shared/SharedKernel/SharedKernel.csproj", "src/shared/SharedKernel/"]
COPY ["src/shared/SmartCore.Telemetry/SmartCore.Telemetry.csproj", "src/shared/SmartCore.Telemetry/"]
RUN dotnet restore "src/FacturaElectronica/FacturaElectronica.WebApi/FacturaElectronica.WebApi.csproj"
COPY . .
WORKDIR "/src/src/FacturaElectronica/FacturaElectronica.WebApi"
RUN dotnet build "FacturaElectronica.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FacturaElectronica.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FacturaElectronica.WebApi.dll"]
