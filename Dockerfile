FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env
WORKDIR /src

COPY . .
RUN dotnet restore ./src/BaGet/Aiursoft.BaGet.csproj --no-cache --configfile nuget.config
RUN dotnet publish ./src/BaGet/Aiursoft.BaGet.csproj --no-restore --configuration Release --output /build

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /build .
EXPOSE 80
ENTRYPOINT ["dotnet","./Aiursoft.BaGet.dll"]
