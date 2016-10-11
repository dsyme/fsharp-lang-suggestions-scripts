System.IO.Directory.SetCurrentDirectory __SOURCE_DIRECTORY__
#r "bin/release/octokit.dll"
#r @"bin/release/fslang_migration.exe"
open System
open FslangMigration
open FslangMigration.Github
open FslangMigration.Program
open FslangMigration.Templating

 
//let jsonfile = System.IO.Path.GetFullPath "../archive-data.json"
//Scrape.scrapeData jsonfile

//Program.generateArchiveFiles jsonfile


testSession "issueTestRepo" |> Async.RunSynchronously
;;
//
//let data = readIdeaDataFromFile jsonfile |> Map.toList |> List.map snd
//let sample = data |> List.take 7
//;;
//sample |> List.iter (fun idea -> 
//    //Templating.responseTemplate idea.Response 
//    Templating.archiveTemplate idea
//    //+ (sprintf "[archived comments](archive/%s.md#comments)\n\n" (ideaFileName idea))
//    |> printfn "%s" )
//;;
//
////sample |> List.iter (fun idea -> Templating.responseTemplate idea.Response |> printfn "%s" )
//
//
////let statuses = data |> List.groupBy (fun i -> i.Status) |> List.map fst 
////let sample = data |> List.take 30
////let tryIssue () = sample |>  Github.loadIssuesInto userPasswordCreds "cloudroutine" "issueTestRepo" |> Async.RunSynchronously
//////let tryIssue () = sample |>  Github.loadIssuesInto tokenCreds "baronfel" "fsharp-lang-testbed" |> Async.RunSynchronously
////let updateData () = reformatData (System.IO.Path.GetFullPath("../archive")) data
////
////
////tryIssue()