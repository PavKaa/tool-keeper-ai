FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8001


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ./ToolKeeperAIBackend/ToolKeeperAIBackend.csproj ./ToolKeeperAIBackend/
COPY ./Service/Service.csproj ./Service/

RUN dotnet restore ./ToolKeeperAIBackend/ToolKeeperAIBackend.csproj

COPY . .
WORKDIR /src/ToolKeeperAIBackend
RUN dotnet build ./ToolKeeperAIBackend.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish ./ToolKeeperAIBackend.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToolKeeperAIBackend.dll"]