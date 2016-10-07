#load "paket-files/include-scripts/net46/include.main.group.fsx"

module Types = 
    open System

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
            Comments : Comment list
            Status : string } 

module Parsing = 
    open Types
    open DotLiquid
    open FSharp.Reflection
    open canopy
    open OpenQA.Selenium
    open System

    let userVoiceItemClass = ".uvIdeaTitle a" // ideas are an anchor inside the uvIdeaTitle
    let nextpageClass = "a.next_page"
    let attr att (el : IWebElement) = el.GetAttribute att
    let href = attr "href"
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
            let submitter = element "div.uvUserActionHeader span.fn" |> read
            let text = defaultArg (someElement "div.uvIdeaDescription div.typeset" |> Option.map read) ""
            let submitted = element "section.uvIdeaSuggestors div.uvUserAction div.uvUserActionHeader span time" |> attr "datetime" |> DateTime.Parse
            let comments = parseCommentsFromPage address |> List.rev
            let state = 
                someElement "span.uvStyle-status" 
                |> Option.map (attr "class") 
                |> Option.map (fun s -> s.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) |> Array.last)
                |> Option.map (fun s -> s.Substring("uvStyle-status-".Length))

            { Number = number
              Submitter = submitter
              Submitted = submitted
              Title = title
              Votes = votes
              Text = text
              Comments = comments
              Status = defaultArg state "" } |> Choice1Of2
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
    let template = parseTemplate<Idea> (IO.File.ReadAllText(__SOURCE_DIRECTORY__ + "/idea.md")) "idea"

    let formatMarkdown (idea : Idea) : string * string =
        sprintf "%s.md" idea.Number, template idea

    let saveToDisk root (name, markdownString) = System.IO.File.WriteAllText(System.IO.Path.Combine(root, name), markdownString)

module API = 
    open FSharp.Data
    open UserVoice
    open System
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq

    let [<Literal>] suggestionJson = """{
  "response_data": {
    "page": 1,
    "per_page": 100,
    "total_records": 2
  },
  "suggestions": [
    {
      "url": "http://initech.us.com:3000/forums/1155-initech/suggestions/106983-better-search-engine?subdomain=initech",
      "id": 106983,
      "title": "better search engine",
      "text": "yeah",
      "vote_count": 0,
      "comments_count": 0,
      "supporters_count": 0,
      "votes_for": 0,
      "votes_remaining": 10,
      "topic": {
        "id": 1155,
        "prompt": "I suggest you ...",
        "example": "Enter your idea",
        "votes_allowed": 10,
        "suggestions_count": 5,
        "votes_remaining": 10,
        "forum": {
          "id": "1155",
          "name": "Initech"
        }
      },
      "category": null,
      "closed_at": "2009-01-22T06:04:11.000Z",
      "status": {
        "id": 4,
        "name": "completed",
        "hex_color": "#7D7EDF"
      },
      "audit_statuses": {
        "id": "123",
        "initial_status": {
          "id": "10",
          "name": "Gathering feedback",
          "hex_color": "#48bdbd",
          "key": "gathering-feedback"
        },
        "final_status": {
          "id": "10",
          "name": "Gathering feedback",
          "hex_color": "#48bdbd",
          "key": "gathering-feedback"
        },
        "created_at": "2009-04-28T10:53:02.000Z",
        "updated_at": "2009-04-28T10:53:02.000Z"
      },
      "creator": {
        "id": 220051,
        "name": "anonymous",
        "title": null,
        "url": "http://initech.us.com:3000/users/220051-anonymous",
        "avatar_url": null,
        "karma_score": 0,
        "created_at": "2009-01-21T06:45:41.000Z",
        "updated_at": "2009-01-21T06:47:28.000Z"
      },
      "response": null,
      "created_at": "2009-01-21T06:45:48.000Z",
      "updated_at": "2010-08-04T08:14:42.000Z"
    },
    {
      "url": "http://initech.us.com:3000/forums/1155-initech/suggestions/107520-buy-more-monkeys?subdomain=initech",
      "id": 107520,
      "title": "buy more monkeys",
      "text": null,
      "vote_count": 0,
      "comments_count": 0,
      "supporters_count": 0,
      "votes_for": 0,
      "votes_remaining": 10,
      "topic": {
        "id": 1155,
        "prompt": "I suggest you ...",
        "example": "Enter your idea",
        "votes_allowed": 10,
        "suggestions_count": 5,
        "votes_remaining": 10,
        "forum": {
          "id": "1155",
          "name": "Initech"
        }
      },
      "category": null,
      "closed_at": "2009-01-22T06:05:29.000Z",
      "status": null,
      "creator": {
        "id": 221282,
        "name": "sarahum",
        "title": null,
        "url": "http://initech.us.com:3000/users/221282-sarahum",
        "avatar_url": "http://www.gravatar.com/avatar/63110d7cb07021e24ee52fb146f512ad",
        "karma_score": 0,
        "created_at": "2009-01-22T04:51:35.000Z",
        "updated_at": "2010-08-22T03:03:26.000Z"
      },
      "response": null,
      "created_at": "2009-01-22T04:53:53.000Z",
      "updated_at": "2010-08-04T08:14:52.000Z"
    }
  ]
}"""

    let [<Literal>] commentJson = """{
  "response_data": {
    "page": 1,
    "per_page": 100,
    "total_records": 1
  },
  "comments": [
    {
      "id": 3462,
      "text": "What coversheets?  Did I get that memo?  Was there a memo?",
      "creator": {
        "id": 211,
        "name": "Milton Addams",
        "title": null,
        "url": "http://initech.us.com:3000/users/211-milton-addams",
        "avatar_url": "http://a1.twimg.com/profile_images/523371144/Marcus_9-20-08_150_normal.png",
        "karma_score": 1661,
        "created_at": "2008-02-04T03:28:00.000Z",
        "updated_at": "2010-08-22T02:56:46.000Z"
      },
      "created_at": "2008-06-16T06:15:07.000Z",
      "updated_at": "2008-06-16T06:15:07.000Z"
    }
  ]
}"""

    type IdeasResponse = JsonProvider<Sample = suggestionJson>
    type CommentsResponse = JsonProvider<Sample = commentJson>
    
    let apiKey = Environment.GetEnvironmentVariable("FSHARP_UV_API_KEY")
    let apiSecret = Environment.GetEnvironmentVariable("FSHARP_UV_API_SECRET")
    let fslangForumId = "245727-f-language"
    let client = UserVoice.Client("fslang", apiKey, apiSecret).LoginAsOwner()
    
    let toIdea (sugg : IdeasResponse.Suggestion) comments : Types.Idea = 
        { Number = string sugg.Id
          Submitter = sugg.Creator.Name
          Submitted = sugg.CreatedAt
          Title = sugg.Title
          Text = defaultArg sugg.Text ""
          Votes = sugg.VoteCount
          Comments = comments 
          Status = defaultArg (sugg.Status |> Option.map (fun s -> s.Name)) "" }

    let toComment (c : CommentsResponse.Comment) : Types.Comment = 
        { Submitter = c.Creator.Name
          Submitted = c.CreatedAt
          Content = c.Text }
    
    let commentsForIdea idea = 
        client.Get(sprintf "/api/v1/forums/%s/suggestions/%s/comments.json?per_page=100" fslangForumId idea)
        |> string
        |> CommentsResponse.Parse
        |> fun root -> root.Comments
        |> Array.map toComment
    
    let ideas () = 
        let suggestions = 
            client.Get(sprintf "/api/v1/forums/%s/suggestions.json?per_page=200" fslangForumId)
            |> string
            |> IdeasResponse.Parse
            |> (fun root -> root.Suggestions)
        let mapped = suggestions |> Array.map (fun idea -> idea, commentsForIdea (string idea.Id) |> List.ofArray)
        mapped |> Array.map (fun (idea, comments) -> toIdea idea comments)

open Parsing
open canopy
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