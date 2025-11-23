.PHONY: build run test migrate docker-up docker-down

build:
	dotnet build NetGPT.sln

run:
	cd src/NetGPT.API && dotnet run

test:
	dotnet test

migrate:
	cd src/NetGPT.API && dotnet ef database update --project ../NetGPT.Infrastructure

docker-up:
	docker-compose up -d

docker-down:
	docker-compose down

clean:
	dotnet clean
	find . -type d -name bin -exec rm -rf {} +
	find . -type d -name obj -exec rm -rf {} +
