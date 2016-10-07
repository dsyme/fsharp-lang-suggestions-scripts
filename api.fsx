#load "paket-files/include-scripts/net46/include.main.group.fsx"
#load "types.fsx"

module API = 
    open FSharp.Data
    open UserVoice
    open System
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open Types

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
        printfn "%O" (sugg.Response.JsonValue.ToString())
        { Number = string sugg.Id
          Submitter = sugg.Creator.Name
          Submitted = sugg.CreatedAt
          Title = sugg.Title
          Text = defaultArg sugg.Text ""
          Votes = sugg.VoteCount
          Comments = comments 
          Status = defaultArg (sugg.Status |> Option.map (fun s -> s.Name)) ""
          Response = Unchecked.defaultof<Types.Response> }

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
