#load "paket-files/include-scripts/net46/include.main.group.fsx"

open canopy
open runner
open System
open OpenQA.Selenium
open DotLiquid
open FSharp.Reflection

let userVoiceItemClass = ".uvIdeaTitle a" // ideas are an anchor inside the uvIdeaTitle
let nextpageClass = "a.next_page"
let attr att (el : IWebElement) = el.GetAttribute att
let href = attr "href"

type Comment = 
    { Submitter : string
      Submitted : DateTime
      Content : string }

type Idea = 
    {   Number : string
        Submitter : string
        Submitted : DateTime
        Title : string
        Text : string
        Votes : int32
        Comments : Comment list } 

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
    let parseComment pos (el :IWebElement) : Comment =
        printfn "parsing comment %d" pos 
        let submitter = el |> elementWithin "span" |> read
        let submitted = el |> elementWithin "time" |> read |> DateTime.Parse
        let content = el |> elementWithin "div.typeset" |> read
        {Submitter = submitter; Submitted = submitted; Content = content}
    
    url address
    let commentBlocks = unreliableElements ".uvIdeaComments li.uvListItem" |> List.mapi parseComment
    match someElement "a.next_page" |> Option.map href with
    | None -> commentBlocks
    | Some page -> commentBlocks @ parseCommentsFromPage page

let parseIdeaFromPage pos address =
    url address
    printfn "parsing idea %d: %s" pos address
    try
        let voteCount = element "div.uvIdeaVoteCount" 
        let number = voteCount |> attr "data-id"
        let votes = voteCount |> elementWithin "strong" |> read |> Int32.Parse
        let title = element "h1.uvIdeaTitle" |> read
        let submitter = element "span.uvUserBadge img" |> attr "alt"
        let text = defaultArg (someElement "div.uvIdeaDescription div.typeset" |> Option.map read) ""
        let submitted = element "section.uvIdeaSuggestors div.uvUserAction div.uvUserActionHeader span time" |> attr "datetime" |> DateTime.Parse
        let comments = parseCommentsFromPage address |> List.rev
        Choice1Of2 <| { Number = number; Submitter = submitter; Submitted = submitted; Title = title; Votes = votes; Text = text; Comments = comments}   
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
    else () (* printfn "%s" ty.FullName *)

  registerTypeTree typeof<'T>
  let t = Template.Parse(template)
  fun k (v:'T) -> t.Render(Hash.FromDictionary(dict [k, box v]))

Template.NamingConvention <- NamingConventions.CSharpNamingConvention()
let template = parseTemplate<Idea> (IO.File.ReadAllText(__SOURCE_DIRECTORY__ + "/idea.md")) "idea"

let formatMarkdown (idea : Idea) : string * string =
    sprintf "%s.md" idea.Number, template idea

let saveToDisk root (name, markdownString) = System.IO.File.WriteAllText(System.IO.Path.Combine(root, name), markdownString)

start chrome 

let items = discoverIdeas ()
let successful, errored = 
    items 
    |> List.mapi parseIdeaFromPage
    |> List.partition (fun i -> match i with Choice1Of2 _ -> true | Choice2Of2 _ -> false) 

errored
|> List.iter (fun bad -> match bad with | Choice2Of2 err -> printfn "%s" err)

successful
|> List.map (fun s -> match s with Choice1Of2 i -> (i |> formatMarkdown))
|> List.iter (saveToDisk (__SOURCE_DIRECTORY__ + "/ideas"))