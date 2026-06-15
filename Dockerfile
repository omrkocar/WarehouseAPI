FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["WarehouseAPI/WarehouseAPI.csproj", "WarehouseAPI/"]
RUN dotnet restore "WarehouseAPI/WarehouseAPI.csproj"
COPY . .
RUN dotnet publish "WarehouseAPI/WarehouseAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WarehouseAPI.dll"]