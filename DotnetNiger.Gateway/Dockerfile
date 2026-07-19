FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY DotnetNiger.Gateway/DotnetNiger.Gateway.csproj DotnetNiger.Gateway/
RUN dotnet restore DotnetNiger.Gateway/DotnetNiger.Gateway.csproj
COPY . .
WORKDIR /src/DotnetNiger.Gateway
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetNiger.Gateway.dll"]
