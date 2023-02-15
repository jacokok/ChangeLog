# ChangeLog

Generate liquibase stored procedure changeLogs for mssql
![icon](icon.png)

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
```

## Package

```bash
dotnet pack -c Release -p:PackageVersion=0.0.2
dotnet nuget push ./bin/Release/Doink.ChangeLog.0.0.2.nupkg -s https://api.nuget.org/v3/index.json -k key
```
