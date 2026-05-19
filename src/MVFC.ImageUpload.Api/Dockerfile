FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /src

COPY Directory.* .editorconfig ./
COPY src/MVFC.Image.Shareable/*.csproj src/MVFC.Image.Shareable/
COPY src/MVFC.Image.Domain/*.csproj src/MVFC.Image.Domain/
COPY src/MVFC.Image.Infra/*.csproj src/MVFC.Image.Infra/
COPY src/MVFC.Image.IoC/*.csproj src/MVFC.Image.IoC/
COPY src/MVFC.ImageUpload.Api/*.csproj src/MVFC.ImageUpload.Api/

RUN dotnet restore ./src/MVFC.ImageUpload.Api/MVFC.ImageUpload.Api.csproj

COPY src/MVFC.Image.Shareable/ src/MVFC.Image.Shareable/
COPY src/MVFC.Image.Domain/ src/MVFC.Image.Domain/
COPY src/MVFC.Image.Infra/ src/MVFC.Image.Infra/
COPY src/MVFC.Image.IoC/ src/MVFC.Image.IoC/
COPY src/MVFC.ImageUpload.Api/ src/MVFC.ImageUpload.Api/

RUN dotnet publish \
    ./src/MVFC.ImageUpload.Api/MVFC.ImageUpload.Api.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "MVFC.ImageUpload.Api.dll"]