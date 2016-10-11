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
        let! labels = client.Issue.Labels.GetAllForRepository repoId |> Async.AwaitTask
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
            do! (createComment repoId issue.Number comment client |> Async.Ignore)
        return issue
    }

    let green = "009900"

    let labels = [
        "declined" , "171819"
        "under review", "cee283"
        "planned", "51b7e2"
        "started", "76bf1c"
        "completed", "540977"
        "open", "d3b47e"
    ]

    /// Creates the standard set of labels based on the uservoice suggestion categories
    let standardLabels repoId (client : IGitHubClient) = async {
        return!
            labels |> List.map (fun (name,hex) ->
                let newLabel = NewLabel (name, hex)
                client.Issue.Labels.Create (repoId, newLabel) |> Async.AwaitTask
            )
            |> Async.Parallel
    }


    /// Creates a new repository to test issue generation
    /// If a repo already exists with that name, delete the repo and create a clean one
    let setupTestRepo repoName (client : IGitHubClient) label = async {
        let createRepo () = NewRepository repoName |> client.Repository.Create
        let! user = client.User.Current() |> Async.AwaitTask
        let! repos = client.Repository.GetAllForCurrent()  |> Async.AwaitTask

        let! initRepo = async {
            if Seq.contains repoName (repos |> Seq.map (fun r -> r.Name)) then
                let! repo = client.Repository.Get(user.Login,repoName) |> Async.AwaitTask
                do! client.Repository.Delete repo.Id |> Async.AwaitTask
                printfn "deleting old %s repo" repoName
                printfn "creating new %s repo" repoName
                return! createRepo() |> Async.AwaitTask
            else
                printfn "creating new %s repo" repoName
                return! createRepo() |> Async.AwaitTask
            }
        let! l = createLabel initRepo.Id label green client
        printfn "created the label - %s | %s" l.Name l.Color
        return initRepo
    }


    let userPasswordCreds () =
        let user = getUserInput "Github Username: "
        let password = getUserPassword "Github Password: "
        Credentials (user, password)

    let tokenCreds () =
        Credentials <| getUserInput "Github Token: "

    let prompt2FA () =
        getUserInput "Two-Factor Auth (2FA) Key: "


    let githubLogin credsFn = async {
        let client = setupClient()
        client.Credentials <- credsFn ()

        let! user = client.User.Current() |> Async.AwaitTask
        printfn "The Current User Is - %s | %s" user.Login user.Name
        return client
    }

    let closeIssue repoId issueId (client : IGitHubClient)  = client.Issue.Update(repoId, issueId, IssueUpdate(State = Nullable.op_Implicit ItemState.Closed)) |> Async.AwaitTask

    let logId id msg =
        printfn "[%s] %s" id msg

    /// there's some delay to the github API, so we're going to try to fetch an issue up to times times 
    let rec pollFetchIssue times (client : IGitHubClient) repoId issueNo = async {
        try 
            printfn "polling for %d" issueNo
            let! issue = client.Issue.Get(repoId, issueNo) |> Async.AwaitTask
            printfn "found %d" issueNo
            return issue
        with 
        | :? AggregateException as aex when aex.InnerException.GetType() = typeof<NotFoundException> && times > 0 ->
            printfn "%d not found, trying again in 5 seconds" issueNo 
            do! Async.Sleep(5 * 1000)
            return! pollFetchIssue (times - 1) client repoId issueNo
        | ex ->
            return failwith <| sprintf "error polling for issue %d: %s" issueNo ex.Message 
    }

    let transformIssue repoId client idea =
        let log = logId idea.Number
        async {
            try
                log "create"
                let body = Templating.submissionTemplate idea
                log "render-suggestion"
                let renderedcomments = idea.Comments |> List.map (fun c -> c.Submitted, Templating.commentTemplate c)
                log "render-comments"
                let allcomments =
                    match idea.Response.Exists with
                    | false -> renderedcomments
                    | true -> (idea.Response.Responded, Templating.responseTemplate idea.Response) :: renderedcomments
                log "order-comments"
                let comments = allcomments |> List.sortBy fst |> List.map snd
                let! issue = createIssue repoId idea.Title body (Seq.singleton idea.Status) comments client
                log "created"
                if idea.Status = "declined" || idea.Status = "completed"
                then
                    log "closing"
                    let! closed = closeIssue repoId issue.Number client
                    return Some closed
                else
                    return issue |> Some
            with
            | :? AggregateException as aex when not <| isNull aex.InnerException ->
                printfn "issue '%s' transform failed\n%s" idea.Number aex.InnerException.Message
                return None
            | ex ->
                printfn "issue '%s' transform failed\n%s" idea.Number ex.Message
                return None
        }

    let loadIssuesInto getCredentials owner repoName ideas  = async {
        let! githubClient = githubLogin getCredentials

        // find the repo
        let! repo = setupTestRepo repoName githubClient "testing"
        //let! repo = githubClient.Repository.Get(owner, repoName) |> Async.AwaitTask

        // labels must be present before we can create issues with them
        //let labels = ideas |> List.map (fun idea -> idea.Status) |> List.distinct
        //let! createdLabels = labels |> List.map (fun l -> createLabel repo.Id l green githubClient) |> Async.Parallel

        return! ideas |> List.map (transformIssue repo.Id githubClient) |> Async.Parallel
    }
