FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Crear directorios con permisos correctos ANTES de cambiar al usuario no-root
RUN mkdir -p /app/documentos-electronicos/activos \
    && mkdir -p /app/documentos-electronicos/archivados \
    && mkdir -p /app/Certificados \
    && chown -R $APP_UID:$APP_UID /app/documentos-electronicos \
    && chown -R $APP_UID:$APP_UID /app/Certificados

USER $APP_UID
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MicroServicios/Src/FacturaElectronica/FacturaElectronica.WebApi/FacturaElectronica.WebApi.csproj", "MicroServicios/Src/FacturaElectronica/FacturaElectronica.WebApi/"]
COPY ["MicroServicios/Src/FacturaElectronica/FacturaElectronica.Aplicacion/FacturaElectronica.Aplicacion.csproj", "MicroServicios/Src/FacturaElectronica/FacturaElectronica.Aplicacion/"]
COPY ["MicroServicios/Src/FacturaElectronica/FacturaElectronica.Dominio/FacturaElectronica.Dominio.csproj", "MicroServicios/Src/FacturaElectronica/FacturaElectronica.Dominio/"]
COPY ["MicroServicios/Src/FacturaElectronica/FacturaElectronica.Infraestructura/FacturaElectronica.Infraestructura.csproj", "MicroServicios/Src/FacturaElectronica/FacturaElectronica.Infraestructura/"]
COPY ["SharedKernel/SharedKernel.csproj", "SharedKernel/"]
RUN dotnet restore "MicroServicios/Src/FacturaElectronica/FacturaElectronica.WebApi/FacturaElectronica.WebApi.csproj"
COPY . .
WORKDIR "/src/MicroServicios/Src/FacturaElectronica/FacturaElectronica.WebApi"
RUN dotnet build "FacturaElectronica.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FacturaElectronica.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FacturaElectronica.WebApi.dll"]
