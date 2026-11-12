# 1. Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy solution and project files first to cache dependencies
COPY *.sln ./
COPY src/SmartAttendance.API/*.csproj ./src/SmartAttendance.API/
COPY src/SmartAttendance.Application/*.csproj ./src/SmartAttendance.Application/
COPY src/SmartAttendance.Infrastructure/*.csproj ./src/SmartAttendance.Infrastructure/
COPY src/SmartAttendance.Shared/*.csproj ./src/SmartAttendance.Shared/
COPY src/SmartAttendance.Domain/*.csproj ./src/SmartAttendance.Domain/

RUN dotnet restore src/SmartAttendance.API/SmartAttendance.API.csproj

# Copy the rest of the code and build
COPY . .
WORKDIR /app/src/SmartAttendance.API
RUN dotnet publish -c Release -o /app/out

# 2. Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Expose ports
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartAttendance.API.dll"]
