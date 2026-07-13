FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["EmployeeManagement.sln", "./"]
COPY ["src/EmployeeManagement.API/EmployeeManagement.API.csproj", "src/EmployeeManagement.API/"]
COPY ["src/EmployeeManagement.Application/EmployeeManagement.Application.csproj", "src/EmployeeManagement.Application/"]
COPY ["src/EmployeeManagement.Domain/EmployeeManagement.Domain.csproj", "src/EmployeeManagement.Domain/"]
COPY ["src/EmployeeManagement.Infrastructure/EmployeeManagement.Infrastructure.csproj", "src/EmployeeManagement.Infrastructure/"]

# Copy everything
COPY . .
RUN dotnet restore "src/EmployeeManagement.API/EmployeeManagement.API.csproj"

WORKDIR "/src/src/EmployeeManagement.API"
RUN dotnet build "EmployeeManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmployeeManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeManagement.API.dll"]
