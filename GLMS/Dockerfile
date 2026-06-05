FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY GLMS/GLMS.csproj GLMS/
RUN dotnet restore GLMS/GLMS.csproj
COPY . .
RUN dotnet publish GLMS/GLMS.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GLMS.dll"]