FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Retail/Retail.WebApi/Retail.WebApi.csproj", "src/Retail/Retail.WebApi/"]
COPY ["src/Retail/Retail.Application/Retail.Application.csproj", "src/Retail/Retail.Application/"]
COPY ["src/Retail/Retail.Domain/Retail.Domain.csproj", "src/Retail/Retail.Domain/"]
COPY ["src/shared/SharedKernel/SharedKernel.csproj", "src/shared/SharedKernel/"]
COPY ["src/Retail/Retail.Infrastructure/Retail.Infrastructure.csproj", "src/Retail/Retail.Infrastructure/"]
RUN dotnet restore "src/Retail/Retail.WebApi/Retail.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Retail/Retail.WebApi"
RUN dotnet build "Retail.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Retail.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Retail.WebApi.dll"]
