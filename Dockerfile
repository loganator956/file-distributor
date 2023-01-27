FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY ./src ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out -p:PublishSingleFile=false -p:PublishTrimmed=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App/out .
ENV FD_FOLDER_A=/input
ENV FD_FOLDER_B=/output
ENV FD_SIZE=5
ENTRYPOINT ["dotnet", "file-distributor.dll"]
