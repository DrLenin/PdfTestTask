FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY PdfProcessing.sln ./
COPY src/PdfProcessing.Shared/PdfProcessing.Shared.csproj src/PdfProcessing.Shared/
COPY src/PdfProcessing.BackgroundWorker/PdfProcessing.BackgroundWorker.csproj src/PdfProcessing.BackgroundWorker/
RUN dotnet restore src/PdfProcessing.BackgroundWorker/PdfProcessing.BackgroundWorker.csproj

COPY . .
RUN dotnet publish src/PdfProcessing.BackgroundWorker/PdfProcessing.BackgroundWorker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PdfProcessing.BackgroundWorker.dll"]
