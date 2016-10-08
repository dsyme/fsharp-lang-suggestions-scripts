#load "paket-files/include-scripts/net46/include.main.group.fsx"
#load "types.fsx"

module Parsing = 
    open Types
    open DotLiquid
    open FSharp.Reflection
    open canopy
    open OpenQA.Selenium
    open System
    open System.IO
    open System.Text.RegularExpressions

    let userVoiceItemClass = ".uvIdeaTitle a" // ideas are an anchor inside the uvIdeaTitle
    let nextpageClass = "a.next_page"
    let attr att (el : IWebElement) = el.GetAttribute att
    let href = attr "href"
    
    /// assumes that this is a 'time' html element
    let time (el : IWebElement) = el |> attr "datetime" |> DateTime.Parse

    let markdownLinkRegex = Regex("(\/ideas\/(\S+))\s*")
    /// attempts to rewrite all urls to uservoice to map to the matching issue document on github.
    /// This is just a simple regex-replace
    let rewrite_urls (text : string) = 
        let rewritten = text.Replace("https://fslang.uservoice.com/forums/245727-f-language/suggestions/", "/ideas/suggestion-")
        match markdownLinkRegex.Matches(rewritten) with
        | m when m.Count = 0 -> rewritten 
        | m -> 
            m |> Seq.cast<Match>
            |> Seq.fold (fun s m' -> 
                // rewrite the /ideas/suggestion string to [name of link](/ideas/suggestion-XXXX)
                let whole = m'.Groups.[0].Value.Trim()
                printfn "whole: %s" whole
                let file = m'.Groups.[1].Value.Trim()
                printfn "file: %s" file
                s.Replace(whole, sprintf "[%s](%s.md)" whole file)
             ) rewritten                

    let discoverIdeas () = 
        let rec parseUrlsFromPage address =
            printfn "discovering from %s" address 
            url address
            let pageItemLinks = 
                elements userVoiceItemClass
                |> List.map href
            
            match someElement nextpageClass |> Option.map href with
            | None -> pageItemLinks
            | Some next -> pageItemLinks @ parseUrlsFromPage next
        
        parseUrlsFromPage "https://fslang.uservoice.com/forums/245727-f-language"

    let rec parseCommentsFromPage address = 
        let parseComment pos (el :IWebElement) : Types.Comment =
            printfn "parsing comment %d" pos 
            let submitter = el |> elementWithin "span" |> read
            let submitted = el |> elementWithin "time" |> read |> DateTime.Parse
            let content = el |> elementWithin "div.typeset" |> read
            {Submitter = submitter; Submitted = submitted; Content = content |> rewrite_urls }
        
        url address
        let commentBlocks = unreliableElements ".uvIdeaComments li.uvListItem" |> List.mapi parseComment
        match someElement "a.next_page" |> Option.map href with
        | None -> commentBlocks
        | Some page -> commentBlocks @ parseCommentsFromPage page

    let parseIdeaFromPage pos address : Choice<Types.Idea,string> =
        url address
        printfn "parsing idea %d: %s" pos address
        try
            let voteCount = element "div.uvIdeaVoteCount" 
            let number = voteCount |> attr "data-id"
            let votes = voteCount |> elementWithin "strong" |> read |> Int32.Parse
            let title = element "h1.uvIdeaTitle" |> read
            let submitter = element "div.uvUserActionHeader span.fn" |> read
            let text = defaultArg (someElement "div.uvIdeaDescription div.typeset" |> Option.map (read >> rewrite_urls)) ""
            let submitted = element "section.uvIdeaSuggestors div.uvUserAction div.uvUserActionHeader span time" |> time
            let comments = parseCommentsFromPage address |> List.rev
            let state = 
                someElement "span.uvStyle-status" 
                |> Option.map (fun el ->((attr "class" el)
                                            .Split([|' '|], StringSplitOptions.RemoveEmptyEntries) |> Array.last)
                                            .Substring "uvStyle-status-".Length)
            
            let response : Types.Response = 
                let responded = someElement "article.uvUserAction-admin-response time" |> Option.map time 
                let text = someElement "article.uvUserAction-admin-response .typeset" |> Option.map (read >> rewrite_urls)
                match responded, text with
                | Some r, Some t -> { Responded = r; Text = t}
                | _, _ -> Unchecked.defaultof<Types.Response>    
                

            { Number = number
              Submitter = submitter
              Submitted = submitted
              Title = title
              Votes = votes
              Text = text
              Comments = comments
              Status = defaultArg state ""
              Response = response } : Types.Idea |> Choice1Of2
        with
        | ex -> Choice2Of2 <| sprintf "error accessing %s: %s" address (ex.Message)

    let parseTemplate<'T> template =
        let rec registerTypeTree ty =
            if FSharpType.IsRecord ty then
                let fields = FSharpType.GetRecordFields(ty)
                Template.RegisterSafeType(ty, [| for f in fields -> f.Name |])
                for f in fields do registerTypeTree f.PropertyType
            elif ty.IsGenericType &&
                ( let t = ty.GetGenericTypeDefinition()
                  in t = typedefof<seq<_>> || t = typedefof<list<_>> ) then   
                () //registerTypeTree (ty.GetGenericArguments().[0])
                registerTypeTree (ty.GetGenericArguments().[0])
            else 
                () (* printfn "%s" ty.FullName *)

        registerTypeTree typeof<'T>
        let t = Template.Parse(template)
        fun k (v:'T) -> t.Render(Hash.FromDictionary(dict [k, box v]))

    Template.NamingConvention <- NamingConventions.CSharpNamingConvention()
    let template = parseTemplate<Types.Idea> (File.ReadAllText(Path.Combine(__SOURCE_DIRECTORY__, "idea.md"))) "idea"

    let sanitize (s : string) =
        let mods = [
            (fun s -> ["<";">";":";"\\";"/";"\"";"|";"?";"*";" ";"`";"'";"(";")";".";"#";] |> List.fold (fun (s : string) sep -> s.Replace(sep, "-")) s)
            fun s -> s.Substring(0, min s.Length 49) // uservoice limits links to files to 49 characters of the title, so we need to keep this constraint if we want linking to still work
            fun s -> s.ToLowerInvariant() // links are tolower in UserVoice
            fun s -> s.TrimEnd('-') // links don't end with -
            fun s -> let r = Regex("-+") in r.Replace(s, "-")
        ]
        
        List.fold (fun str f -> f str) s mods
        

    let formatMarkdown (idea : Types.Idea) : string * string =
        let sanitizedName = sprintf "suggestion-%s-%s" idea.Number (sanitize idea.Title)
        sprintf "%s.md" sanitizedName, template idea

    let saveToDisk root (name, markdownString) = 
        System.IO.File.WriteAllText(System.IO.Path.Combine(root, name), markdownString)

module Scrape = 
    open Parsing
    open canopy
    open OpenQA.Selenium

    canopy.configuration.chromeDir <- @"packages/Selenium.WebDriver.ChromeDriver/driver"
    canopy.configuration.elementTimeout <- 2.0
    canopy.configuration.pageTimeout <- 2.0
    canopy.configuration.configuredFinders <- (fun selector f -> seq { yield finders.findByCss selector f })
 

    let scrapeData destination =
        start chrome                                                 
        let items = discoverIdeas ()
        let successful, errored = 
            items 
            |> List.mapi parseIdeaFromPage
            |> List.partition (function Choice1Of2 _ -> true | Choice2Of2 _ -> false) 

        errored
        |> List.iter (function  Choice2Of2 err -> printfn "%s" err | _ -> ())


        let successes = successful |> List.choose (function Choice1Of2 i -> Some i | _ -> None)

        let data = 
            successes 
            |> List.map (fun i -> i.Number, i) |> Map.ofList 
            |> Newtonsoft.Json.JsonConvert.SerializeObject

        let jsonFile = System.IO.Path.Combine(__SOURCE_DIRECTORY__, destination)
        System.IO.File.WriteAllText(jsonFile, data)

open Parsing
open System
open System.IO
open Newtonsoft.Json
open Types.Types

let jsonFile = Path.Combine(__SOURCE_DIRECTORY__, "ideas.json")
let readData = 
    System.IO.File.ReadAllText(jsonFile)
    |> (fun text -> JsonConvert.DeserializeObject<Map<string, Idea>>(text))

let data = readData |> Map.toList |> List.map snd

data |> List.groupBy (fun idea -> idea.Status)

data
|> List.map formatMarkdown
|> List.iter (saveToDisk (__SOURCE_DIRECTORY__ + "/ideas"))