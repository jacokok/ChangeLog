# ChangeLog

Generate liquibase stored procedure changeLogs for mssql

![icon](icon.png)

## Installation

Install directly from nuget.

```bash
dotnet tool install --global Doink.ChangeLog
```

Update

```bash
dotnet tool update --global Doink.ChangeLog
```

Uninstall

```bash
dotnet tool uninstall --global Doink.ChangeLog
```

## How to use

Great now you should have changelog command available.

```bash
# Hello World
changelog

# Show all available commands
changelog -h

# Init changeLog.yml file with example sql server connection strings.
changelog init

# Update connection strings with your details. 
# I like to use something like: https://www.aireforge.com/tools/sql-server-connection-string-generator

# Validate connection
changelog validate

# List diff between connections with type User Table
changelog diff -t U

# Generate liquibase changeSet to seed the People table data
changelog seed -n People

# Generate liquibase changeSet for specific item
changelog generate -n storedProcedureName

# Update stored procedures changeSets in folder from database. -d option will make no changes
changelog update -d
```
