FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY global.json Computervision.sln ./
COPY Computervision/*.csproj Computervision/
COPY Computervision.UnitTests/*.csproj Computervision.UnitTests/
RUN dotnet restore Computervision.sln --use-current-runtime

COPY Computervision/. Computervision/
COPY Computervision.UnitTests/. Computervision.UnitTests/

# Fail the image build if any unit test fails
RUN dotnet test --solution Computervision.sln -c Release --no-restore

RUN dotnet publish Computervision/Computervision.csproj -c Release -o /app --use-current-runtime --self-contained false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT [ "dotnet", "Computervision.dll" ]
