FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json NotificationDotnet.sln ./
COPY NotificationDotnet/*.csproj NotificationDotnet/
COPY NotificationDotnet.UnitTests/*.csproj NotificationDotnet.UnitTests/
RUN dotnet restore NotificationDotnet.sln --use-current-runtime

COPY NotificationDotnet/. NotificationDotnet/
COPY NotificationDotnet.UnitTests/. NotificationDotnet.UnitTests/

# Fail the image build if any unit test fails
RUN dotnet test --solution NotificationDotnet.sln -c Release --no-restore

RUN dotnet publish NotificationDotnet/NotificationDotnet.csproj -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "NotificationDotnet.dll" ]
