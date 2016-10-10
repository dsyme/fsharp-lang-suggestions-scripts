namespace FslangMigration

open System
open System.IO
open System.Reflection
open System.Threading
open Octokit
open Octokit.Internal


module Github =
    open Input
    let setupClient () =
        let connection = Connection (ProductHeaderValue "fsharp-lang")
        GitHubClient connection
    
    let createClient user password = async {        
        let github = setupClient()
        github.Credentials <- Credentials (user, password)
        return github
    }

    let createClientWithToken token = async {        
        let github = setupClient()
        github.Credentials <- Credentials token
        return github
    }        

    let createLabel repoId label colorHex (client : IGitHubClient) = async {
        let! labels = client.Issue.Labels.GetAllForRepository(repoId) |> Async.AwaitTask
        match labels |> Seq.tryFind (fun l -> l.Name.Equals(label, StringComparison.OrdinalIgnoreCase)) with
        | Some label -> return label
        | None -> 
            let newLabel = NewLabel(label, colorHex)
            return! client.Issue.Labels.Create(repoId, newLabel) |> Async.AwaitTask
    }

    let createComment repoId issueId text (client : IGitHubClient) = client.Issue.Comment.Create(repoId, issueId, text) |> Async.AwaitTask
    
    let createIssue repoId title text labels comments (client : IGitHubClient) = async {
        let newIssue = NewIssue(title, Body = text)
        labels |> Seq.iter newIssue.Labels.Add
        let! issue = client.Issue.Create(repoId, newIssue) |> Async.AwaitTask
        for comment in comments do
            do! (createComment repoId issue.Id comment client |> Async.Ignore)
        return issue
    }

    
    let userPasswordCreds () = 
        let user = getUserInput "Github Username: "
        let password = getUserPassword "Github Password: "
        Credentials (user, password)

    let tokenCreds () = 
        Credentials <| getUserInput "Github Token: "

    let prompt2FA () = 
        getUserInput "Two-Factor Auth (2FA) Key: "


    let GithubEngage credsFn = async {  
        let client = setupClient()
        client.Credentials <- credsFn ()
    
        let! user = client.User.Current() |> Async.AwaitTask
        printfn "The Current User Is - %s" user.Name
        return client
    }


    let green = "009900"

    let closeIssue repoId issueId (client : IGitHubClient)  = client.Issue.Update(repoId, issueId, IssueUpdate(State = Nullable.op_Implicit ItemState.Closed)) |> Async.AwaitTask

    let logId id msg = 
        printfn "[%s] %s" id msg

    let transformIssue repoId client idea = 
        let log = logId idea.Number
        async {
            try
                log "create"
                let body = Templating.ideaTemplate idea
                log "render-suggestion"
                let renderedcomments = idea.Comments |> List.map (fun c -> c.Submitted, Templating.commentTemplate c)
                log "render-comments"
                let allcomments = 
                    if idea.Response = Unchecked.defaultof<Response> 
                    then renderedcomments
                    else (idea.Response.Responded, Templating.responseTemplate idea.Response) :: renderedcomments
                log "order-comments"
                let comments = allcomments |> List.sortBy fst |> List.map snd
                let! issue = createIssue repoId idea.Title body (Seq.singleton idea.Status) comments client
                log "created"
                if idea.Status = "declined" || idea.Status = "completed" 
                then 
                    log "closing"
                    let! closed = closeIssue repoId issue.Id client 
                    return Some closed
                else
                    return issue |> Some
            with 
            | e -> 
                printfn "issue '%s' transform failed\n%s" idea.Number e.Message
                return None
        }

    let loadIssuesInto getCredentials owner repoName ideas  = async {
        let! githubClient = GithubEngage getCredentials
        // find the repo
        let! repo = githubClient.Repository.Get(owner, repoName) |> Async.AwaitTask
        
        // labels must be present before we can create issues with them 
        let labels = ideas |> List.map (fun idea -> idea.Status) |> List.distinct
        let! createdLabels = labels |> List.map (fun l -> createLabel repo.Id l green githubClient) |> Async.Parallel

        return! ideas |> List.map (transformIssue repo.Id githubClient) |> Async.Parallel
    }


