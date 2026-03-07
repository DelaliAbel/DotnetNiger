# DotnetNiger

Plateforme communautaire microservices construite avec .NET 8.

## Services

- Gateway: `DotnetNiger.Gateway` (Ocelot)
- Identity: `DotnetNiger.Identity`
- Community: `DotnetNiger.Community`

## Ports par defaut

- Gateway: `http://localhost:5000`
- Identity: `http://localhost:5075`
- Community: `http://localhost:5269`

## Fonctionnalites principales

- API Gateway Ocelot (routing centralise)
- Authentification JWT
- Rate limiting, QoS, cache (Ocelot natif)
- Swagger aggregate au niveau gateway
- Logs Serilog

## Demarrage rapide

```bash
git clone https://github.com/akaletekoffilevis/DotnetNiger.git
cd DotnetNiger
dotnet restore
```

Lancer les services (selon vos scripts locaux):

```bash
./run.sh
# ou
./run.ps1
```

## URLs utiles

- Gateway Swagger: `http://localhost:5000/swagger`
- Identity Swagger: `http://localhost:5075/swagger`
- Community Swagger: `http://localhost:5269/swagger`

## Documentation

- `docs/INDEX.md`
- `docs/SETUP.md`
- `docs/ARCHITECTURE.md`
- `docs/API.md`
- `GATEWAY_SETUP.md`

## Notes

- La configuration du routing gateway est dans `DotnetNiger.Gateway/ocelot.json`.
- Les routes protegees utilisent `AuthenticationProviderKey: "Bearer"`.
- Les options de resilience et de limites sont configurees directement par route via Ocelot.
