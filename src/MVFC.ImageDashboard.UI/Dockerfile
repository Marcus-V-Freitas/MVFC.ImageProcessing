FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY Directory.* .editorconfig ./
COPY src/MVFC.Image.Shareable/*.csproj src/MVFC.Image.Shareable/
COPY src/MVFC.ImageDashboard.UI/*.csproj src/MVFC.ImageDashboard.UI/

RUN dotnet restore ./src/MVFC.ImageDashboard.UI/MVFC.ImageDashboard.UI.csproj

COPY src/MVFC.Image.Shareable/ src/MVFC.Image.Shareable/
COPY src/MVFC.ImageDashboard.UI/ src/MVFC.ImageDashboard.UI/

RUN dotnet publish \
    ./src/MVFC.ImageDashboard.UI/MVFC.ImageDashboard.UI.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MVFC.ImageDashboard.UI.dll"]
