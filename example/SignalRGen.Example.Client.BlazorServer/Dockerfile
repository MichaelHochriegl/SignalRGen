FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["example/SignalRGen.Example.Client.BlazorServer/SignalRGen.Example.Client.BlazorServer.csproj", "example/SignalRGen.Example.Client.BlazorServer/"]
RUN dotnet restore "example/SignalRGen.Example.Client.BlazorServer/SignalRGen.Example.Client.BlazorServer.csproj"
COPY . .
WORKDIR "/src/example/SignalRGen.Example.Client.BlazorServer"
RUN dotnet build "SignalRGen.Example.Client.BlazorServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalRGen.Example.Client.BlazorServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalRGen.Example.Client.BlazorServer.dll"]
