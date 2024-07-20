# use PowerShell instead of sh:
set shell := ["powershell.exe", "-c"]

hello:
	Write-Host "Hello, world!"

start-infra:
	docker compose -f ./local/infra/docker-compose.observability.yaml up -d

stop-infra:
	docker compose -f ./local/infra/docker-compose.observability.yaml down -v

start-weather:
	clear;dotnet run --project ./src/Microservices/Microservices.WeatherService/Microservices.WeatherService.csproj

start-client:
	clear;dotnet run --project src/ClientApps/ClientApps.ClientSimulator/ClientApps.ClientSimulator.csproj