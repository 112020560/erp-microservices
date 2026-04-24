FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Catalogs/Catalogs.WebApi/Catalogs.WebApi.csproj", "src/Catalogs/Catalogs.WebApi/"]
COPY ["src/Catalogs/Catalogs.Application/Catalogs.Application.csproj", "src/Catalogs/Catalogs.Application/"]
COPY ["src/Catalogs/Catalogs.Domain/Catalogs.Domain.csproj", "src/Catalogs/Catalogs.Domain/"]
COPY ["src/shared/SharedKernel/SharedKernel.csproj", "src/shared/SharedKernel/"]
COPY ["src/Catalogs/Catalogs.Infrastructure/Catalogs.Infrastructure.csproj", "src/Catalogs/Catalogs.Infrastructure/"]
RUN dotnet restore "src/Catalogs/Catalogs.WebApi/Catalogs.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Catalogs/Catalogs.WebApi"
RUN dotnet build "Catalogs.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Catalogs.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalogs.WebApi.dll"]
