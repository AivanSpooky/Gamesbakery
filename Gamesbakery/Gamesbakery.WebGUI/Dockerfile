FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Gamesbakery.WebGUI/Gamesbakery.WebGUI.csproj", "Gamesbakery.WebGUI/"]
COPY ["Gamesbakery.BusinessLogic/Gamesbakery.BusinessLogic.csproj", "Gamesbakery.BusinessLogic/"]
COPY ["Gamesbakery.Core/Gamesbakery.Core.csproj", "Gamesbakery.Core/"]
COPY ["Gamesbakery.DataAccess/Gamesbakery.DataAccess.csproj", "Gamesbakery.DataAccess/"]
COPY ["Gamesbakery.Infrastructure/Gamesbakery.Infrastructure.csproj", "Gamesbakery.Infrastructure/"]
RUN dotnet restore "./Gamesbakery.WebGUI/Gamesbakery.WebGUI.csproj"
COPY . .
WORKDIR "/src/Gamesbakery.WebGUI"
RUN dotnet build "./Gamesbakery.WebGUI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gamesbakery.WebGUI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

COPY --from=build /src /src
COPY --from=publish /app/publish .
RUN mkdir -p /app/logs && chmod -R 777 /app/logs

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "Gamesbakery.WebGUI.dll"]