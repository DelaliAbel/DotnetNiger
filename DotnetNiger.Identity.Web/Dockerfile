FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj", "DotnetNiger.Identity.Web/"]
RUN dotnet restore "DotnetNiger.Identity.Web/DotnetNiger.Identity.Web.csproj"

COPY . .
WORKDIR "/src/DotnetNiger.Identity.Web"

RUN dotnet build "DotnetNiger.Identity.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DotnetNiger.Identity.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:5100
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5100

RUN mkdir -p /app/logs

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5100/health || exit 1

ENTRYPOINT ["dotnet", "DotnetNiger.Identity.Web.dll"]
