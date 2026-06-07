FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files
COPY GLMS.API/GLMS.API.csproj GLMS.API/
COPY GLMS/GLMS.csproj GLMS/

# Restore
RUN dotnet restore GLMS.API/GLMS.API.csproj

# Copy everything except appsettings.json files
COPY . .

# Remove conflicting appsettings files before publish
RUN rm -f GLMS/appsettings.json GLMS/appsettings.Development.json GLMS.API/appsettings.json GLMS.API/appsettings.Development.json || true

# Publish
RUN dotnet publish GLMS.API/GLMS.API.csproj -c Release -o /app/publish /p:NoWarn=CS8602

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.API.dll"]