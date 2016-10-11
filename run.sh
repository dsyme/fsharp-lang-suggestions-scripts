mono ./.paket/paket.bootstrapper.exe
mono ./.paket/paket.exe restore
msbuild fslang-migration.sln /p:Configuration=Release
fsharpi src/scrape.fsx