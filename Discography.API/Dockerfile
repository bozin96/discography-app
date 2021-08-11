#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["Discography.API/Discography.API.csproj", "Discography.API/"]
RUN dotnet restore "Discography.API/Discography.API.csproj"
COPY . .
WORKDIR "/src/Discography.API"
RUN dotnet build "Discography.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discography.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discography.API.dll"]