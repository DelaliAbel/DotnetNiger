# DotnetNiger.Client

Frontend Blazor WebAssembly pour la plateforme communautaire DotnetNiger.

## Stack

- .NET 8.0 (Blazor WebAssembly)
- Tailwind CSS 3
- TinyMCE
- Font Awesome 6

## Architecture

Application monopage communiquant avec le backend API sur `http://localhost:5000`. Authentification via OpenIddict (JWT).

Les services backend sont abstraits derriere des interfaces (`Services/Contracts/`) avec une implementation Mock (dev sans backend) et une implementation Api (production).

## Quick Start

```bash
dotnet restore && dotnet run
```

Ouvrir `http://localhost:5201`.

## Tailwind

```bash
npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css --watch
```
