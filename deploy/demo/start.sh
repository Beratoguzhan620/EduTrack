#!/bin/sh
# Starts the demo's two apps on local ports behind nginx (8080, the port Render routes to).
set -eu

export DOTNET_gcServer=0 DOTNET_GCConserveMemory=7
mkdir -p /tmp/files /tmp/keys

cd /app/files
ASPNETCORE_HTTP_PORTS=8082 FileServer__ApiKey="$FILE_API_KEY" FileServer__StoragePath=/tmp/files dotnet FileAPI.dll &
files=$!

cd /app/web
ASPNETCORE_HTTP_PORTS=8081 \
Database__Provider=Sqlite ConnectionStrings__DefaultConnection="Data Source=/tmp/edutrack.db" \
FileApi__BaseUrl=http://127.0.0.1:8082/ FileApi__ApiKey="$FILE_API_KEY" \
DataProtection__KeysPath=/tmp/keys \
Demo__LoginHint="Demo: eğitmen ogretmen@example.com, öğrenci zeynep@example.com, yönetici admin@example.com · şifre Demo123!" \
  dotnet EduTrack.Web.dll &
web=$!

nginx -e stderr -c /app/nginx.conf -g 'daemon off;' &
proxy=$!

trap 'kill 0' TERM INT
wait "$web" "$files" "$proxy"
