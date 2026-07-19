FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj DotnetNiger.Identity.Web/
RUN dotnet restore DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj
COPY . .
WORKDIR /src/DotnetNiger.Identity.Web
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 5100
ENV ASPNETCORE_URLS=http://+:5100
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotnetNiger.Identity.Web.dll"]
