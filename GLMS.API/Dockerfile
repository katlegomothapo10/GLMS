FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for better caching
COPY GLMS.API/GLMS.API.csproj GLMS.API/
COPY GLMS/GLMS.csproj GLMS/

# Restore dependencies
RUN dotnet restore GLMS.API/GLMS.API.csproj

# Copy all source code but exclude appsettings.json files
COPY GLMS/ GLMS/
COPY GLMS.API/ GLMS.API/

# Remove appsettings.json files from the copied source to avoid conflicts
RUN rm -f GLMS/appsettings.json GLMS/appsettings.Development.json GLMS.API/appsettings.json GLMS.API/appsettings.Development.json 2>/dev/null || true

# Publish the application
RUN dotnet publish GLMS.API/GLMS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.API.dll"]