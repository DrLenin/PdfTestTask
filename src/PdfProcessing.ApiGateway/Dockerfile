FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PdfProcessing.sln ./
COPY src/PdfProcessing.Shared/PdfProcessing.Shared.csproj src/PdfProcessing.Shared/
COPY src/PdfProcessing.ApiGateway/PdfProcessing.ApiGateway.csproj src/PdfProcessing.ApiGateway/
RUN dotnet restore src/PdfProcessing.ApiGateway/PdfProcessing.ApiGateway.csproj

COPY . .
RUN dotnet publish src/PdfProcessing.ApiGateway/PdfProcessing.ApiGateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PdfProcessing.ApiGateway.dll"]
