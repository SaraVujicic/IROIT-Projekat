FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RequestService.API/RequestService.API.csproj", "RequestService.API/"]
COPY ["RequestService.Application/RequestService.Application.csproj", "RequestService.Application/"]
COPY ["RequestService.Domain/RequestService.Domain.csproj", "RequestService.Domain/"]
COPY ["RequestService.Infrastructure/RequestService.Infrastructure.csproj", "RequestService.Infrastructure/"]
RUN dotnet restore "./RequestService.API/RequestService.API.csproj"
COPY . .
WORKDIR "/src/RequestService.API"
RUN dotnet build "./RequestService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RequestService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RequestService.API.dll"]
