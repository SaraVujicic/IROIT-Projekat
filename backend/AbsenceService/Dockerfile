FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Absence.Api/Absence.Api.csproj", "src/Absence.Api/"]
COPY ["src/Absence.Application/Absence.Application.csproj", "src/Absence.Application/"]
COPY ["src/Absence.Domain/Absence.Domain.csproj", "src/Absence.Domain/"]
COPY ["src/Absence.Infrastructure/Absence.Infrastructure.csproj", "src/Absence.Infrastructure/"]
RUN dotnet restore "src/Absence.Api/Absence.Api.csproj"

COPY src/ src/
WORKDIR "/src/src/Absence.Api"
RUN dotnet build "Absence.Api.csproj" -c Release -o /app/build
RUN dotnet publish "Absence.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Absence.Api.dll"]

