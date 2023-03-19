FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source

COPY NotificationDotnet/*.csproj .
RUN dotnet restore --use-current-runtime

COPY NotificationDotnet/. .
RUN dotnet publish -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "NotificationDotnet.dll" ]