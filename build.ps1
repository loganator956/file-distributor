dotnet publish -c Release --self-contained true -r win-x64
dotnet publish -c Release --self-contained true -r linux-x64
dotnet publish -c Release --self-contained true -r linux-arm
dotnet publish -c Release --self-contained true -r linux-arm64

Compress-Archive -Path ".\bin\Release\net6.0\win-x64\publish\file-distributor.exe" -CompressionLevel "Fastest" -Force -DestinationPath "./bin/file-distributor.win-x64.zip"
Compress-Archive -Path ".\bin\Release\net6.0\linux-x64\publish\file-distributor" -CompressionLevel "Fastest" -Force -DestinationPath "./bin/file-distributor.linux-x64.zip"
Compress-Archive -Path ".\bin\Release\net6.0\linux-arm\publish\file-distributor" -CompressionLevel "Fastest" -Force -DestinationPath "./bin/file-distributor.linux-arm.zip"
Compress-Archive -Path ".\bin\Release\net6.0\linux-arm64\publish\file-distributor" -CompressionLevel "Fastest" -Force -DestinationPath "./bin/file-distributor.linux-arm64.zip"