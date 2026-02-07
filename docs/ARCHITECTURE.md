# Architecture

DotnetNiger est un projet microservices avec une API Gateway.

```
Client
   |
   v
Gateway (YARP)
   |-- Identity service
   |-- Community service
   v
SQL Server + Redis
```

## Services

- Gateway: routage, swagger aggregation, auth/limiting si active
- Identity: auth, users, tokens
- Community: posts, comments, likes, follow

## Couches (Clean Architecture)

- Api: controllers, middleware
- Application: services, DTOs, validators
- Domain: entities, interfaces
- Infrastructure: data, repositories, external

## Flux simple

1. Client -> Gateway
2. Gateway -> Service cible
3. Service -> DB/Cache
4. Reponse -> Client

## Donnees

- Chaque service a sa base de donnees
- Redis pour cache et perf
