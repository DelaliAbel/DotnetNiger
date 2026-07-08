FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY DotnetNiger.Common/DotnetNiger.Common.csproj DotnetNiger.Common/
COPY DotnetNiger.Identity/DotnetNiger.Identity.csproj DotnetNiger.Identity/
RUN dotnet restore DotnetNiger.Identity/DotnetNiger.Identity.csproj

COPY . .
WORKDIR /src/DotnetNiger.Identity
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 5075
ENV ASPNETCORE_URLS=http://+:5075
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetNiger.Identity.dll"]
