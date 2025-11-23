FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NetGPT.API/NetGPT.API.csproj", "src/NetGPT.API/"]
COPY ["src/NetGPT.Application/NetGPT.Application.csproj", "src/NetGPT.Application/"]
COPY ["src/NetGPT.Domain/NetGPT.Domain.csproj", "src/NetGPT.Domain/"]
COPY ["src/NetGPT.Infrastructure/NetGPT.Infrastructure.csproj", "src/NetGPT.Infrastructure/"]
RUN dotnet restore "src/NetGPT.API/NetGPT.API.csproj"
COPY . .
WORKDIR "/src/src/NetGPT.API"
RUN dotnet build "NetGPT.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetGPT.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetGPT.API.dll"]
