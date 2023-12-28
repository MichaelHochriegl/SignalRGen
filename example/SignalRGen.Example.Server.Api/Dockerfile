FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["example/SignalRGen.Example.Server.Api/SignalRGen.Example.Server.Api.csproj", "example/SignalRGen.Example.Server.Api/"]
RUN dotnet restore "example/SignalRGen.Example.Server.Api/SignalRGen.Example.Server.Api.csproj"
COPY . .
WORKDIR "/src/example/SignalRGen.Example.Server.Api"
RUN dotnet build "SignalRGen.Example.Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SignalRGen.Example.Server.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRGen.Example.Server.Api.dll"]
