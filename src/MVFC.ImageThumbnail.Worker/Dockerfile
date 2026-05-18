FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY . .

RUN dotnet restore ./src/MVFC.ImageThumbnail.Worker/MVFC.ImageThumbnail.Worker.csproj

RUN dotnet publish \
    ./src/MVFC.ImageThumbnail.Worker/MVFC.ImageThumbnail.Worker.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MVFC.ImageThumbnail.Worker.dll"]