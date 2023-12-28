#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["TwitchStatusApi.csproj", "./"]
RUN dotnet restore "TwitchStatusApi.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "TwitchStatusApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TwitchStatusApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TwitchStatusApi.dll"]
