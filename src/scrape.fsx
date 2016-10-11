System.IO.Directory.SetCurrentDirectory __SOURCE_DIRECTORY__
#r "bin/release/octokit.dll"
#r @"bin/release/fslang_migration.exe"
open FslangMigration
open FslangMigration.Github
open FslangMigration.Program

 
let jsonfile = System.IO.Path.GetFullPath "../archive-data-2.json"
Scrape.scrapeData jsonfile