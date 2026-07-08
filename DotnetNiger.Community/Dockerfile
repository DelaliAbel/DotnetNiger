FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY DotnetNiger.Common/DotnetNiger.Common.csproj DotnetNiger.Common/
COPY DotnetNiger.Community/DotnetNiger.Community.csproj DotnetNiger.Community/
RUN dotnet restore DotnetNiger.Community/DotnetNiger.Community.csproj

COPY . .
WORKDIR /src/DotnetNiger.Community
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 5050
ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetNiger.Community.dll"]
