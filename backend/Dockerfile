# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["backend/RetailOps.Api/RetailOps.Api.csproj", "backend/RetailOps.Api/"]
COPY ["backend/RetailOps.Application/RetailOps.Application.csproj", "backend/RetailOps.Application/"]
COPY ["backend/RetailOps.Domain/RetailOps.Domain.csproj", "backend/RetailOps.Domain/"]
COPY ["backend/RetailOps.Infrastructure/RetailOps.Infrastructure.csproj", "backend/RetailOps.Infrastructure/"]
RUN dotnet restore "backend/RetailOps.Api/RetailOps.Api.csproj"
COPY . .
WORKDIR "/src/backend/RetailOps.Api"
RUN dotnet build "RetailOps.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "RetailOps.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RetailOps.Api.dll"]
