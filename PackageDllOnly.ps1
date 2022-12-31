# Get the file name from the C# project name
$ProjectName = Get-Item ./src/*.csproj | % {$_.Name -replace $_.Extension}
Remove-Item -Force -Recurse ./Package/

$targetDir = mkdir -Force  ./Package/$ProjectName


Copy-Item -Destination $targetDir "src\bin\Release\*\*.dll" 

Compress-Archive -Path $targetDir -Force -DestinationPath ./$ProjectName.zip
