FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY /src/marketdata.api/marketdata.api.csproj ./src/marketdata.api/
COPY /src/marketdata.domain/marketdata.domain.csproj ./src/marketdata.domain/
COPY /src/marketdata.infrastructure/marketdata.infrastructure.csproj ./src/marketdata.infrastructure/
COPY /src/marketdata.workerservice/marketdata.workerservice.csproj ./src/marketdata.workerservice/

RUN dotnet restore ./src/marketdata.api/marketdata.api.csproj

COPY /src/ ./src
WORKDIR /app/src/marketdata.api
RUN dotnet build marketdata.api.csproj -c Release -o /app/build

RUN dotnet publish marketdata.api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT [ "dotnet", "marketdata.api.dll" ]