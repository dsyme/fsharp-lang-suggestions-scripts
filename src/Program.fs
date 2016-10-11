﻿namespace FslangMigration         
open System
open System.IO
open Newtonsoft.Json
open Github
open Templating

module Program =
    Directory.SetCurrentDirectory __SOURCE_DIRECTORY__

    let readIdeaDataFromFile file =
        Path.Combine(__SOURCE_DIRECTORY__, file)
        |> System.IO.File.ReadAllText
        |> fun str -> JsonConvert.DeserializeObject<Map<string, Idea>> str

    let reloadIdeaDataFromUserVoice file = 
        Scrape.scrapeData file
    
    let saveToDisk root (name, markdownString) = 
        System.IO.File.WriteAllText(Path.Combine(root, name), markdownString)

    let reformatData root = List.map (Templating.formatMarkdown >> saveToDisk root) >> ignore

    let ideasFromJsonFile jsonfile =
        readIdeaDataFromFile jsonfile |> Map.toList |> List.map snd
        


    let generateArchiveFiles jsonfile =
        let ideas = ideasFromJsonFile jsonfile 
        ideas |> reformatData "../archive"
        printfn "Generated %i archive files" ideas.Length


    let jsonfile = System.IO.Path.GetFullPath "../archive-data.json"


    let testSession (repoName:string) = async {
        let! client = githubLogin userPasswordCreds

        let! repo = setupTestRepo repoName client 
        // create deafault labels
        let! _ = standardLabels repo.Id client

        let ideas = ideasFromJsonFile jsonfile |> List.take 5
        let fileNames = ideas |> List.map (fun i -> ideaFileName i + ".md")

        let! _ = createRepoIssues client repo.Id ideas

        //Threading.Thread.Sleep 5000

        let! _ = retry 5 <| uploadFiles client repo.Id fileNames
        let! _ = retry 5 <| closeLabeledIssues client repo.Id ["declined";"completed"]
        return ()
    }


    [<EntryPoint>]
    let main argv = 
        printfn "%A" argv

        let jsonfile = System.IO.Path.Combine(__SOURCE_DIRECTORY__, "archive-data.json")
        let data = readIdeaDataFromFile jsonfile |> Map.toList |> List.map snd
        let statuses = data |> List.groupBy (fun i -> i.Status) |> List.map fst 
        let sample = data |> List.take 5
        let tiny = sample |> List.map (fun s -> {s with Text = ""; Comments = []})
        //let tryIssue () = sample |>  Github.loadIssuesInto "cloudroutine" "fsharp-lang-suggestions" |> Async.RunSynchronously
        //let updateData () = Tasks.reformatData jsonfile data


        0 // return an integer exit code
