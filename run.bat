.\.paketpaket.bootstrapper.exe
.\paket\paket restore
msbuild fslang-migration.sln /p:Configuration=Release
fsi src\scrape.fsx