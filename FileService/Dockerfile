FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json FileService.sln ./
COPY FileService/*.csproj FileService/
COPY FileService.UnitTests/*.csproj FileService.UnitTests/
RUN dotnet restore FileService.sln --use-current-runtime

COPY FileService/. FileService/
COPY FileService.UnitTests/. FileService.UnitTests/

# Fail the image build if any unit test fails
RUN dotnet test --solution FileService.sln -c Release --no-restore

RUN dotnet publish FileService/FileService.csproj -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "FileService.dll" ]
