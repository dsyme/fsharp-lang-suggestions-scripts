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

        let! user = client.User.Current() 
        printfn "The Current User Is - %s | %s" user.Login user.Name
        return client
    }


    let throttled (client:IGitHubClient) (fn:'a->'b) = 
        let apiInfo = client.GetLastApiInfo()
        if isNull apiInfo then fn 
        else
        let activeWindow = abs (apiInfo.RateLimit.Reset.UtcDateTime - DateTime.UtcNow).Milliseconds        
        if apiInfo.RateLimit.Remaining <= 1 then Thread.Sleep (activeWindow + 1000)
        let apiInfo = client.GetLastApiInfo()   
        let interval = activeWindow / apiInfo.RateLimit.Remaining
        Thread.Sleep interval
        fn

    let rec retry  (client:IGitHubClient) (fn:unit->'a) = 
        try fn()
        with 
        | :? Octokit.RateLimitExceededException as e->
            printfn "%s" e.Message
            let sleeptime = abs (e.Reset.UtcDateTime - DateTime.UtcNow).Milliseconds
            printfn "sleeping for %M seconds" (decimal sleeptime / 1000.0m)
            Thread.Sleep sleeptime
            retry client fn
        | :? Octokit.ForbiddenException as e  -> 
            retry client (throttled client fn)

    /// get the handle of the user that started the client's session
    let getLogin (client:IGitHubClient) = async {
        let! user = client.User.Current() 
        return user.Login
    }

    /// Creates a new repository to test issue generation
    /// If a repo already exists with that name, delete the repo and create a clean one
    let setupTestRepo repoName (client : IGitHubClient) = async {
        let createRepo () = 
            NewRepository(repoName, AutoInit = Nullable true) 
            |> client.Repository.Create

        let! login = getLogin client
        let! repos = client.Repository.GetAllForCurrent()  

        let! initRepo = async {
            if Seq.contains repoName (repos |> Seq.map (fun r -> r.Name)) then
                let! repo = client.Repository.Get(login, repoName) 
                do! client.Repository.Delete repo.Id |> Async.AwaitTask
                printfn "deleting old %s repo" repoName
                printfn "creating new %s repo" repoName
                return! createRepo() 
            else
                printfn "creating new %s repo" repoName
                return! createRepo() 
            }        
        printfn "Test Repository created @ - %s" initRepo.HtmlUrl
        return initRepo
    }



    let labels = [
        "declined" , "171819"
        "under review", "cee283"
        "planned", "51b7e2"
        "started", "76bf1c"
        "completed", "540977"
        //"open", "d3b47e"
        "votes:0-10", "333333" //TODO actual colors
        "votes:11-50", "333333"
        "votes:51-100", "333333"
        "votes:101-150", "333333"
        "votes:151-200", "333333"
        "votes:201-300", "333333"
        "votes:300+", "333333"
    ]

    let inline runTaskSync<'a> = Seq.map (Async.AwaitTask >> Async.RunSynchronously)

    /// Creates the standard set of labels based on the uservoice suggestion categories
    let standardLabels repoId (client : IGitHubClient) = async {
        return!
            labels |> List.map (fun (name,hex) ->
                let newLabel = NewLabel (name, hex)
                client.Issue.Labels.Create (repoId, newLabel) 
            ) |> Async.Parallel
    }

    let logId id msg = printfn "[%s] %s" id msg

    let getLabels (issue:Issue) = issue.Labels |> Seq.map (fun l -> l.Name)

    /// create an issue on the specified repository with labels that have already
    /// been created on that repository
    let createIssue repoId title text labels (client : IGitHubClient) = async {
        let newIssue = NewIssue(title, Body = text)
        labels |> Seq.iter newIssue.Labels.Add
        let! issue = client.Issue.Create(repoId, newIssue)
        return issue
    }

    let bucket i =
        match i with
        | i when i = 0 -> None
        | i when i > 0 && i <= 10 -> Some "votes:1-10"
        | i when i > 10 && i <= 50 -> Some "votes:11-50"
        | i when i > 200 && i <= 300 -> Some "votes:200-300"
        | i when i > 300 -> Some "votes:300+"
        | i -> 
            let m = i / 50
            let M = (i / 50) + 1
            Some <| sprintf "votes:%d-%d" (m * 50 + 1) (M * 50)
        
    /// Pings github api 1 time per function call
    let ideaToIssue repoId (client:IGitHubClient) idea =
        let log = logId idea.Number
        // reorder commments in chronological order
        let idea = { idea with Comments = idea.Comments |> List.sortBy (fun c -> c.Submitted) }
        let text = Templating.submissionTemplate idea + Templating.archiveCommentLink idea
        let labels = [
            if idea.Status <> "open" then yield idea.Status
            match bucket idea.Votes with Some s -> yield s | None -> ()
        ] 
        let issue = createIssue repoId idea.Title text labels client |> Async.RunSynchronously
        log <| sprintf "%s :: [%s]" issue.Title (String.concat "][" (getLabels issue))
        issue

    //let loadIssuesInto getCredentials owner repoName ideas  = async {
    let createRepoIssues (client:IGitHubClient) repoId ideas  = 
        ideas |> List.map (fun i -> 
            Thread.Sleep 4000
            let issue = ideaToIssue repoId client i 
            Thread.Sleep 4000
            issue
        ) 

    /// Pings github api 1 time per function call
    let closeIssue repoId (issue:Issue) (client : IGitHubClient) = async {
        let closeUpdate = issue.ToUpdate()
        closeUpdate.State <- Nullable ItemState.Closed        
        let! issue = client.Issue.Update(repoId, issue.Number,closeUpdate) 
        printfn "'%s' was closed at '%s'" issue.Title (let i = issue.ClosedAt in if i.HasValue then string i.Value else "")
        return issue
    }


    /// close all issues in the repository that have at least one label from the provided list
    let closeLabeledIssues (client:IGitHubClient) repoId (labels:string list)  = async {
        let! allissues = client.Issue.GetAllForRepository repoId
        let issues = allissues |> Seq.filter (fun i -> 
            let l = getLabels i in Seq.contains "declined" l || Seq.contains "completed" l
        )
        printfn "\nFound %i Issues to Close\n"  <| Seq.length issues
        let closed = 
            [| for i in issues -> 
                Thread.Sleep 2000
                let issue = closeIssue repoId i client |>  Async.RunSynchronously  
                issue
            |] 
        return closed
    }


    let uploadFiles (client:IGitHubClient) repoId filenames = async {   
        printfn "uploading archive files (later) -\n%s" (filenames |> String.concat "\n")

        let testBlob = NewBlob(Content="this is a test",Encoding = EncodingType.Utf8)
        let! blobResult = client.Git.Blob.Create(repoId,testBlob) 
        
        printfn "sha for blob - %s" blobResult.Sha

        let! sha1 = client.Repository.Commit.GetSha1(repoId,"heads/master")

        let createdBlobs = 
            [| for file in filenames ->
                // throttle the calls to github to avoid the 300 call/m rate limit
                Thread.Sleep 5000
                let diskpath = Path.Combine("../archive",file) |> Path.GetFullPath
                let blob = NewBlob(Encoding=EncodingType.Utf8, Content=File.ReadAllText diskpath)
                printfn "created blob for '%s'" file
                client.Git.Blob.Create(repoId,blob) 
            |] |> Array.map (Async.AwaitTask >> Async.RunSynchronously)

        let treeItems = 
            (filenames, createdBlobs) ||> Seq.map2 (fun filename blob ->  
            NewTreeItem
                (   Type = TreeType.Blob
                ,   Mode = FileMode.File
                ,   Path = "archive/" + filename
                ,   Sha  = blob.Sha)                
            )
        
        let archiveRoot = NewTree(BaseTree=sha1)

        treeItems |> Seq.iter archiveRoot.Tree.Add 

        let! createdArchiveRoot = client.Git.Tree.Create(repoId,archiveRoot)

        let! master = client.Git.Reference.Get(repoId,"heads/master")
        
        Thread.Sleep 5000

        let newCommit = NewCommit("added uservoice suggestions archive", createdArchiveRoot.Sha, [|master.Object.Sha|])

        let! createdCommit = client.Git.Commit.Create(repoId,newCommit) 

        let reference  = ReferenceUpdate createdCommit.Sha
        let! updatedReference = client.Git.Reference.Update(repoId,"heads/master",reference)
        
        Thread.Sleep 5000

        printfn "Reference Update @ %s" updatedReference.Url
    }


