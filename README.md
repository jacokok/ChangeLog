# ChangeLog

Generate liquibase stored procedure changeLogs for mssql

## Get Started

For now pull source code until we have releases ready.

## Development

```bash
dotnet run -- gen
```

## Docker Image

```bash
docker pull liquibase/liquibase
docker run --rm liquibase/liquibase --version

docker run --rm -v ./temp/liquibase.properties:/liquibase/changelog liquibase/liquibase init project
docker run --rm --network shared -v ./temp:/liquibase/changelog:Z liquibase/liquibase --defaultsFile=/liquibase/changelog/liquibase.properties update
