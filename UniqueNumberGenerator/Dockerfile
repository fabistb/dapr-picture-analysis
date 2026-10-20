FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json UniqueNumberGenerator.sln ./
COPY UniqueNumberGenerator/*.csproj UniqueNumberGenerator/
COPY UniqueNumberGenerator.UnitTests/*.csproj UniqueNumberGenerator.UnitTests/
RUN dotnet restore UniqueNumberGenerator.sln --use-current-runtime

COPY UniqueNumberGenerator/. UniqueNumberGenerator/
COPY UniqueNumberGenerator.UnitTests/. UniqueNumberGenerator.UnitTests/

# Fail the image build if any unit test fails
RUN dotnet test --solution UniqueNumberGenerator.sln -c Release --no-restore

RUN dotnet publish UniqueNumberGenerator/UniqueNumberGenerator.csproj -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
# .NET 8+ images listen on 8080 by default; keep port 80 to match the deployment config
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "UniqueNumberGenerator.dll" ]
