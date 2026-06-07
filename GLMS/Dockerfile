FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj
COPY GLMS/GLMS.csproj GLMS/

# Restore
RUN dotnet restore GLMS/GLMS.csproj

# Copy everything
COPY . .

# Publish
RUN dotnet publish GLMS/GLMS.csproj -c Release -o /app/publish /p:NoWarn=CS8602

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.dll"]