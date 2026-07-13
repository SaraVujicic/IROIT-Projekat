FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY *.sln .
COPY src/LeaveBalanceService.Domain/*.csproj ./src/LeaveBalanceService.Domain/
COPY src/LeaveBalanceService.Application/*.csproj ./src/LeaveBalanceService.Application/
COPY src/LeaveBalanceService.Infrastructure/*.csproj ./src/LeaveBalanceService.Infrastructure/
COPY src/LeaveBalanceService.API/*.csproj ./src/LeaveBalanceService.API/
COPY tests/LeaveBalanceService.Tests.Unit/*.csproj ./tests/LeaveBalanceService.Tests.Unit/
COPY tests/LeaveBalanceService.Tests.Integration/*.csproj ./tests/LeaveBalanceService.Tests.Integration/

# Restore packages
RUN dotnet restore

# Copy all files
COPY . .

# Build and run tests
RUN dotnet build -c Release --no-restore
# RUN dotnet test -c Release --no-build

# Publish
RUN dotnet publish src/LeaveBalanceService.API/LeaveBalanceService.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LeaveBalanceService.API.dll"]
