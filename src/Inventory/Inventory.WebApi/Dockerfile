FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Inventory/Inventory.WebApi/Inventory.WebApi.csproj", "src/Inventory/Inventory.WebApi/"]
COPY ["src/Inventory/Inventory.Application/Inventory.Application.csproj", "src/Inventory/Inventory.Application/"]
COPY ["src/Inventory/Inventory.Domain/Inventory.Domain.csproj", "src/Inventory/Inventory.Domain/"]
COPY ["src/shared/SharedKernel/SharedKernel.csproj", "src/shared/SharedKernel/"]
COPY ["src/Inventory/Inventory.Infrastructure/Inventory.Infrastructure.csproj", "src/Inventory/Inventory.Infrastructure/"]
RUN dotnet restore "src/Inventory/Inventory.WebApi/Inventory.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Inventory/Inventory.WebApi"
RUN dotnet build "Inventory.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Inventory.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Inventory.WebApi.dll"]
