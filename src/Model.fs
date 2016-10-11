[<AutoOpen>]
module FslangMigration.Model

open System
open System.Threading.Tasks


type Microsoft.FSharp.Control.AsyncBuilder with
    member x.Bind(t:Task<'T>, f:'T -> Async<'R>) : Async<'R>  = 
        async.Bind(Async.AwaitTask t, f)

    member x.ReturnFrom(t:Task<'T>) : Async<'T>  = 
        Async.AwaitTask t


type Microsoft.FSharp.Control.Async with
    static member Parallel (tasks:seq<Task<'T>>) =
        tasks |> Seq.map Async.AwaitTask |> Async.Parallel



//    member x.Bind(t:Tasks.Task<'T>,f:('T -> Async<'R>) : Async<'R> =
//


type Comment = 
    {   Submitter : string
        Submitted : DateTime
        Content : string }
    
type Response = 
    {   Responded : DateTime
        Text : string
        Exists : bool }

type Idea = 
    {   Number : string
        Submitter : string
        Submitted : DateTime
        Title : string
        Text : string
        Votes : int32
        Comments : Comment list
        Status : string 
        Response : Response } 