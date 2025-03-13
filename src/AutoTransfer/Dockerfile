FROM hub.nexdev.net/base/dotnet/core/sdk:7.0 AS builder
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ./src/Shared/Shared.csproj ./Shared/Shared.csproj
COPY ./src/AutoTransfer/AutoTransfer.csproj ./AutoTransfer/AutoTransfer.csproj
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore AutoTransfer/AutoTransfer.csproj -v n

# Copy everything else and build
COPY /src/Shared ./Shared
COPY /src/AutoTransfer ./AutoTransfer
WORKDIR /src/AutoTransfer
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:7.0-alpine-25h1 as base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

FROM base AS final
WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"

ENTRYPOINT ["dotnet", "AutoTransfer.dll"]
