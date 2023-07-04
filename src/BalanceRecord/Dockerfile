FROM hub.nexdev.net/base/dotnet/core/sdk:7.0-alpine AS builder
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ./src/Shared/Shared.csproj ./Shared/Shared.csproj
COPY ./src/BalanceRecord/BalanceRecord.csproj ./BalanceRecord/BalanceRecord.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore BalanceRecord/BalanceRecord.csproj -v n

# Copy everything else and build
COPY /src/Shared ./Shared
COPY /src/BalanceRecord ./BalanceRecord
WORKDIR /src/BalanceRecord
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine AS base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

FROM base AS final
WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "BalanceRecord.dll"]
