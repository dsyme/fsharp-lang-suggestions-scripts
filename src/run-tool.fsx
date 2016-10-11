System.IO.Directory.SetCurrentDirectory __SOURCE_DIRECTORY__
#r "bin/release/octokit.dll"
#r @"bin/release/fslang_migration.exe"
open FslangMigration
open FslangMigration.Github
open FslangMigration.Program

 
let jsonfile = System.IO.Path.GetFullPath "../archive-data.json"
//Scrape.scrapeData jsonfile

let data = readIdeaDataFromFile jsonfile |> Map.toList |> List.map snd
let statuses = data |> List.groupBy (fun i -> i.Status) |> List.map fst 
let sample = data |> List.take 30
//let tryIssue () = sample |>  Github.loadIssuesInto userPasswordCreds "cloudroutine" "issueTestRepo" |> Async.RunSynchronously
let tryIssue () = sample |>  Github.loadIssuesInto tokenCreds "baronfel" "fsharp-lang-testbed" |> Async.RunSynchronously
let updateData () = reformatData (System.IO.Path.GetFullPath("../archive")) data