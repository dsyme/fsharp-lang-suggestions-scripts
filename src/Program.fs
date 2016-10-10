namespace FslangMigration         
open System
open System.IO
open Newtonsoft.Json

module Program =

    let readIdeaDataFromFile file =
        Path.Combine(__SOURCE_DIRECTORY__, file)
        |> System.IO.File.ReadAllText
        |> fun str -> JsonConvert.DeserializeObject<Map<string, Idea>> str

    let reloadIdeaDataFromUserVoice file = 
        Scrape.scrapeData file
    
    let saveToDisk root (name, markdownString) = 
        System.IO.File.WriteAllText(System.IO.Path.Combine(root, name), markdownString)

    let reformatData root = List.map (Templating.formatMarkdown >> saveToDisk root) >> ignore

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
