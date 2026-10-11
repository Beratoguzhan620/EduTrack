FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY . .

RUN dotnet restore "src/EduTrack.Web/EduTrack.Web.csproj"

RUN dotnet publish "src/EduTrack.Web/EduTrack.Web.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

RUN mkdir -p /app/data-protection-keys \
    && chown -R "$APP_UID":"$APP_UID" /app

USER $APP_UID

ENTRYPOINT ["dotnet", "EduTrack.Web.dll"]