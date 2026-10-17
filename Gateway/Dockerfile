FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Gateway.sln ./
COPY Gateway/*.csproj Gateway/
COPY Gateway.UnitTests/*.csproj Gateway.UnitTests/
RUN dotnet restore Gateway.sln --use-current-runtime

COPY Gateway/. Gateway/
COPY Gateway.UnitTests/. Gateway.UnitTests/

# Fail the image build if any unit test fails
RUN dotnet test --solution Gateway.sln -c Release --no-restore

RUN dotnet publish Gateway/Gateway.csproj -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# .NET 8+ images listen on 8080 by default; keep port 80 to match the deployment config
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Gateway.dll" ]
