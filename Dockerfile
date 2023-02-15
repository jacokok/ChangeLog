FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine AS base
WORKDIR /app

RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["changelog.csproj", "./"]
RUN dotnet restore "changelog.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "changelog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "changelog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "changelog.dll"]