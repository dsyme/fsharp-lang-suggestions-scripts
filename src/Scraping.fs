namespace FslangMigration    

module Parsing = 
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
    let rewriteUrls (text : string) =
        let urls = [
            "https://fslang.uservoice.com/forums/245727-f-language/suggestions/"
            "http://fslang.uservoice.com/forums/245727-f-language/suggestions/"
        ] 
        let rewritten = urls |> List.fold (fun (s:string) url -> s.Replace(url, "/ideas/suggestion-")) text
        match markdownLinkRegex.Matches(rewritten) with
        | m when m.Count = 0 -> rewritten 
        | m -> 
            m |> Seq.cast<Match>
            |> Seq.fold (fun s m' -> 
                // rewrite the /ideas/suggestion string to [name of link](/ideas/suggestion-XXXX)
                let whole = m'.Groups.[0].Value.Trim()
                //printfn "whole: %s" whole
                let file = m'.Groups.[1].Value.Trim()
                //printfn "file: %s" file
                s.Replace(whole, sprintf "[%s](%s.md)" whole file)
             ) rewritten                

    let discoverIdeas () = 
        let rec parseUrlsFromPage address =
            //printfn "discovering from %s" address 
            url address
            let pageItemLinks = 
                elements userVoiceItemClass
                |> List.map href
            
            match someElement nextpageClass |> Option.map href with
            | None -> pageItemLinks
            | Some next -> pageItemLinks @ parseUrlsFromPage next
        
        let pages = 
            [
                "https://fslang.uservoice.com/forums/245727-f-language"
                "https://fslang.uservoice.com/forums/245727-f-language/status/1225913"
                "https://fslang.uservoice.com/forums/245727-f-language/status/1225914"
                "https://fslang.uservoice.com/forums/245727-f-language/status/1225915"
                "https://fslang.uservoice.com/forums/245727-f-language/status/1225916"
                "https://fslang.uservoice.com/forums/245727-f-language/status/1225917"
            ]

        pages |> List.collect parseUrlsFromPage |> List.distinct

    let rec parseCommentsFromPage address = 
        let parseComment pos (el :IWebElement) : Comment =
            //printfn "parsing comment %d" pos 
            let submitter = el |> elementWithin "span" |> read
            let submitted = el |> elementWithin "time" |> read |> DateTime.Parse
            let content = el |> elementWithin "div.typeset" |> read
            {Submitter = submitter; Submitted = submitted; Content = content |> rewriteUrls }
        
        url address
        let commentBlocks = unreliableElements ".uvIdeaComments li.uvListItem" |> List.mapi parseComment
        match someElement "a.next_page" |> Option.map href with
        | None -> commentBlocks
        | Some page -> commentBlocks @ parseCommentsFromPage page

    let parseIdeaFromPage pos address : Choice<Idea,string> =
        url address
        printfn "parsing idea %d: %s" pos address
        try
            let voteCount = element "div.uvIdeaVoteCount" 
            let number = voteCount |> attr "data-id"
            let votes = voteCount |> elementWithin "strong" |> read |> Int32.Parse
            let title = element "h1.uvIdeaTitle" |> read
            let submitter = element "div.uvUserActionHeader span.fn" |> read
            let text = defaultArg (someElement "div.uvIdeaDescription div.typeset" |> Option.map (read >> rewriteUrls)) ""
            let submitted = element "section.uvIdeaSuggestors div.uvUserAction div.uvUserActionHeader span time" |> time
            let comments = parseCommentsFromPage address |> List.rev
            let state = 
                someElement "span.uvStyle-status" 
                |> Option.map (fun el ->((attr "class" el)
                                            .Split([|' '|], StringSplitOptions.RemoveEmptyEntries) |> Array.last)
                                            .Substring "uvStyle-status-".Length)
            
            let response = 
                let responded = someElement "article.uvUserAction-admin-response time" |> Option.map time 
                let text = someElement "article.uvUserAction-admin-response .typeset" |> Option.map (read >> rewriteUrls)
                match responded, text with
                | Some r, Some t -> { Responded = r; Text = t; Exists = true }
                | _, _ -> {Exists = false; Responded = DateTime.MinValue; Text = ""}
                

            { Number = number
              Submitter = submitter
              Submitted = submitted
              Title = title
              Votes = votes
              Text = text
              Comments = comments
              Status = defaultArg state "open"
              Response = response } : Idea |> Choice1Of2
        with
        | ex -> Choice2Of2 <| sprintf "error accessing %s: %s" address (ex.Message)


module Scrape = 
    open Parsing
    open canopy
    open OpenQA.Selenium

    canopy.configuration.chromeDir <- @"/usr/local/bin"
    canopy.configuration.elementTimeout <- 2.0
    canopy.configuration.pageTimeout <- 2.0
    canopy.configuration.configuredFinders <- (fun selector f -> seq { yield finders.findByCss selector f })
    canopy.configuration.autoPinBrowserRightOnLaunch <- false

    let scrapeData destination =
        start chrome                                                 
        let items = discoverIdeas ()
        printfn "found %d ideas" items.Length
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


