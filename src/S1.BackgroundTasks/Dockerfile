FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ./src/S1.BackgroundTasks/S1.BackgroundTasks.csproj ./S1.BackgroundTasks/S1.BackgroundTasks.csproj
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore S1.BackgroundTasks/S1.BackgroundTasks.csproj -v n

# Copy everything else and build
COPY /src/S1.BackgroundTasks ./S1.BackgroundTasks
WORKDIR /src/S1.BackgroundTasks
RUN dotnet publish -r linux-x64 --self-contained false -o /app

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine AS base

RUN adduser --disabled-password \
    --home /app \
    --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

FROM base AS final
WORKDIR /app
COPY --from=builder /app .
ENV TZ="America/La_Paz"
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "S1.BackgroundTasks.dll"]
