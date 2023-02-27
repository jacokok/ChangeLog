FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt update && apt install clang zlib1g-dev -y
WORKDIR /src
# COPY ["changelog.csproj", "./"]
# RUN dotnet restore "changelog.csproj" -p:PublishAot=true -p:PackAsTool=false -p:TargetFramework=net7.0 -p:TargetFrameworks=net7.0
COPY . .
# WORKDIR "/src/."
# RUN dotnet build "changelog.csproj" -c Release -o /app/build

# FROM build AS publish
RUN dotnet publish "changelog.csproj" -c Release -r linux-x64 -o /app/publish -p:PackAsTool=false -p:PublishAot=true -p:TargetFramework=net7.0 -p:TargetFrameworks=net7.0

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./changelog"]