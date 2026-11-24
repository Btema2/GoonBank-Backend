# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["GoonBank/GoonBank.csproj", "GoonBank/"]
RUN dotnet restore "GoonBank/GoonBank.csproj"

COPY GoonBank/ GoonBank/
WORKDIR "/src/GoonBank"
RUN dotnet publish "GoonBank.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Ensure SQLite storage directory exists
RUN mkdir -p /app/data

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8000
EXPOSE 8000

ENTRYPOINT ["dotnet", "GoonBank.dll"]
