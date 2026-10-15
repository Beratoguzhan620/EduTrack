FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY external-services/FileAPI/FileAPI.csproj external-services/FileAPI/
RUN dotnet restore external-services/FileAPI/FileAPI.csproj

COPY external-services/FileAPI/ external-services/FileAPI/
RUN dotnet publish external-services/FileAPI/FileAPI.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    FileServer__StoragePath=/data/files

EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /data/files && chown -R "$APP_UID":"$APP_UID" /data/files

USER $APP_UID

ENTRYPOINT ["dotnet", "FileAPI.dll"]
