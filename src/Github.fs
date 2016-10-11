namespace FslangMigration

open System
open System.IO
open System.Reflection
open System.Threading
open Octokit
open Octokit.Internal
open Input


module Github =

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

    /// create an issue on the specified repository with labels that have already
    /// been created on that repository
    let createIssue repoId title text labels (client : IGitHubClient) = async {
        let newIssue = NewIssue(title, Body = text)
        labels |> Seq.iter newIssue.Labels.Add
        let! issue = client.Issue.Create(repoId, newIssue) |> Async.AwaitTask
        return issue
    }


    let closeIssue repoId issueId (client : IGitHubClient) = 
        let closedIssue =  IssueUpdate(State = Nullable ItemState.Closed)
        client.Issue.Update(repoId, issueId,closedIssue) |> Async.AwaitTask


    /// Creates a new repository to test issue generation
    /// If a repo already exists with that name, delete the repo and create a clean one
    let setupTestRepo repoName (client : IGitHubClient) = async {
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
        return initRepo
    }


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
            ) |> Async.Parallel
    }

    let logId id msg = printfn "[%s] %s" id msg

    /// there's some delay to the github API, so we're going to try to fetch an issue up to times times 
    let rec pollFetchIssue times (client:IGitHubClient) repoId issueNum = async {
        try 
            printfn "polling for %d" issueNum
            let! issue = client.Issue.Get(repoId, issueNum) |> Async.AwaitTask
            printfn "found %d" issueNum
            return issue
        with 
        | :? AggregateException as aex when aex.InnerException.GetType() = typeof<NotFoundException> && times > 0 ->
            printfn "%d not found, trying again in 5 seconds" issueNum 
            do! Async.Sleep(5 * 1000)
            return! pollFetchIssue (times - 1) client repoId issueNum
        | ex ->
            return failwith <| sprintf "error polling for issue %d: %s" issueNum ex.Message 
    }

    let ideaToIssue repoId (client:IGitHubClient) idea =
        let log = logId idea.Number
        async {
            try
                // reorder commments in chronological order
                let idea = { idea with Comments = idea.Comments |> List.sortBy (fun c -> c.Submitted) }
                let text = Templating.submissionTemplate idea + Templating.archiveCommentLink idea
                let! issue = createIssue repoId idea.Title text (Seq.singleton idea.Status) client
                log "created"
                return issue |> Some
            with
            | :? AggregateException as aex when not <| isNull aex.InnerException ->
                printfn "issue '%s' transform failed\n%s" idea.Number aex.InnerException.Message
                return None
            | ex ->
                printfn "issue '%s' transform failed\n%s" idea.Number ex.Message
                return None
        }


    //let loadIssuesInto getCredentials owner repoName ideas  = async {
    let createRepoIssues (client:IGitHubClient) repoId ideas  = async {
        return! ideas |> List.map (ideaToIssue repoId client) |> Async.Parallel
    }


    /// close all issues in the repository that have at least one label from the provided list
    let closeLabeledIssues (client:IGitHubClient) repoId (labels:string list)  = async {
        let closeLabels = Set labels
        let! repoIssues = client.Issue.GetAllForRepository repoId |> Async.AwaitTask
        let  closeSqs = repoIssues |> Seq.filter (fun i ->
            let issueLabels = (i.Labels |> Seq.map (fun l -> l.Name)) |> Set
            let intersect = Set.intersect closeLabels issueLabels
            intersect.Count > 0
        )
        return! closeSqs |> Seq.map (fun i -> closeIssue repoId i.Id client) |> Async.Parallel
    }


    /// A version of 'reraise' that can work inside computation expressions
    let private captureAndReraise ex =
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw()
        Unchecked.defaultof<_>

    let private isRunningOnMono = System.Type.GetType "Mono.Runtime" <> null

    /// Retry the Octokit action count times
    let rec internal retry count asyncF =
        // This retry logic causes an exception on Mono:
        // https://github.com/fsharp/fsharp/issues/440
        if isRunningOnMono then asyncF else async {
            try  return! asyncF
            with ex ->
            return! 
                match (ex, ex.InnerException) with
                | (:? AggregateException, (:? AuthorizationException as ex)) -> captureAndReraise ex
                | _ when count > 0 -> retry (count - 1) asyncF
                | (ex, _) -> captureAndReraise ex
        }
//
//    /// Retry the Octokit action count times after input succeed
//    let private retryWithArg count input asycnF = async {
//        let! choice = input |> Async.Catch
//        match choice with
//        | Choice1Of2 input' ->
//            return! (asycnF input') |> retry count
//        | Choice2Of2 ex ->
//            return captureAndReraise ex
//    }


//        retryWithArg 5 draft <| fun draft' -> async {
//            let fi = FileInfo(fileName)
//            let archiveContents = File.OpenRead(fi.FullName)
//            let assetUpload = new ReleaseAssetUpload(fi.Name,"application/octet-stream",archiveContents,Nullable<TimeSpan>())
//            let! asset = Async.AwaitTask <| draft'.Client.Repository.Release.UploadAsset(draft'.DraftRelease, assetUpload)
//            printfn "Uploaded %s" asset.Name
//            return draft'
//
//        System.IO.File.ReadAllText
//        let! draft' = draft
//        let draftW = async { return draft' }
//        let! _ = Async.Parallel [for f in filenames -> uploadFile f draftW ]
//        return draft'


    let uploadFile (client:IGitHubClient) repoId (filepath:string) (contents:string) = async {
        let contents = 
            Text.Encoding.UTF8.GetBytes contents |> Convert.ToBase64String
        let request = CreateFileRequest(sprintf "created %s" filepath, contents, "master" )
        return! client.Repository.Content.CreateFile(repoId ,filepath, request)|> Async.AwaitTask
    }


    let uploadFiles (client:IGitHubClient) repoId filenames = async {        
        return! Async.Parallel [
            for file in filenames -> 
                let repopath = Path.Combine("archive",file)
                let diskpath = Path.Combine("../archive",file) |> Path.GetFullPath
                uploadFile  client repoId repopath (File.ReadAllText diskpath)
        ]
    }

