FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY Directory.* .editorconfig ./
COPY src/MVFC.Image.Shareable/*.csproj src/MVFC.Image.Shareable/
COPY src/MVFC.ImageDelete.Worker/*.csproj src/MVFC.ImageDelete.Worker/

RUN dotnet restore ./src/MVFC.ImageDelete.Worker/MVFC.ImageDelete.Worker.csproj

COPY src/MVFC.Image.Shareable/ src/MVFC.Image.Shareable/
COPY src/MVFC.ImageDelete.Worker/ src/MVFC.ImageDelete.Worker/

RUN dotnet publish \
    ./src/MVFC.ImageDelete.Worker/MVFC.ImageDelete.Worker.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MVFC.ImageDelete.Worker.dll"]