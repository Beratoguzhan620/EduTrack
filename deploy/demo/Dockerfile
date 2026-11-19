# The public demo in one container, for a single free web service (Render): nginx in front of the app and the File
# API (under /files), with SQLite in a file. Every restart starts again from the demo classroom. Compose runs the
# app and the File API as separate containers on SQL Server instead.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/EduTrack.Web/EduTrack.Web.csproj -c Release -o /app/web /p:UseAppHost=false \
 && dotnet publish external-services/FileAPI/FileAPI.csproj -c Release -o /app/files /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends nginx && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/web ./web
COPY --from=build /app/files ./files
COPY deploy/demo/nginx.conf deploy/demo/start.sh ./
USER $APP_UID
ENTRYPOINT ["sh", "/app/start.sh"]
