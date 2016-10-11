mono .paket/paket.bootstrapper.exe
mono .paket/paket restore
msbuild fslang-migration.sln /p:Configuration=Release
fsharpi src/scrape.fsx