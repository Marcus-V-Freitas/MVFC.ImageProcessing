FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY . .

RUN dotnet restore ./src/MVFC.ImageAnalysis.Worker/MVFC.ImageAnalysis.Worker.csproj

RUN dotnet publish \
    ./src/MVFC.ImageAnalysis.Worker/MVFC.ImageAnalysis.Worker.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MVFC.ImageAnalysis.Worker.dll"]
