# Homokozo

A sample project to explore various things.
Contains the following applications:

## Homokozo.Data

Private web api using gRPC, allows access to a configured database.

## Homokozo.Api

Public api.

## Homokozo.App

Frontend. Phoenix?

# Authorization

Okta

# Configuration


## Azure

# Development

## Postgres

Run `docker-compose up -d` in the root directory.
Ensure the container is running: `docker ps -a`
Connect to postgres: `docker exec -it 141bc0121fc7 psql -U user -d homokozo_db`
Query users: `SELECT * FROM "Users";`

## EF

`dotnet ef migrations add InitialCreate`
`dotnet ef database update`
