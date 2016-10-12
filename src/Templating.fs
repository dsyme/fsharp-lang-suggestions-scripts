module FslangMigration.Templating

open DotLiquid
open FSharp.Reflection
open System.IO
open System.Text.RegularExpressions
open System.Collections.Generic

let registrations = Dictionary<_,_>()

let parseTemplate<'T> template =
    let rec registerTypeTree ty =
        if registrations.ContainsKey ty then ()
        elif FSharpType.IsRecord ty then
            let fields = FSharpType.GetRecordFields ty
            Template.RegisterSafeType(ty, [| for f in fields -> f.Name |])
            registrations.[ty] <- true
            for f in fields do registerTypeTree f.PropertyType
        elif ty.IsGenericType then
            let t = ty.GetGenericTypeDefinition()
            if t = typedefof<seq<_>> || t = typedefof<list<_>>  then
                registrations.[ty] <- true
                registerTypeTree (ty.GetGenericArguments().[0])     
            elif t = typedefof<option<_>> then
                Template.RegisterSafeType(ty, [|"Value"; "IsSome"; "IsNone";|])
                registrations.[ty] <- true
                registerTypeTree (ty.GetGenericArguments().[0])
            elif ty.IsArray then          
                registrations.[ty] <- true
                registerTypeTree (ty.GetElementType())
        else ()
   
    registerTypeTree typeof<'T>
    let t = Template.Parse template
    fun k (v:'T) -> t.Render(Hash.FromDictionary(dict [k, box v]))

Template.NamingConvention <- NamingConventions.CSharpNamingConvention()
let templatedir = Path.GetFullPath "../templates/"
let fs = DotLiquid.FileSystems.LocalFileSystem templatedir
// next line tests tempalte file discovery
// ["'idea'"; "'idea_comment'";"'idea_response'";"'idea_submission'"]
// |> List.iter (fun path -> fs.ReadTemplateFile(DotLiquid.Context(ResizeArray(), DotLiquid.Hash(), DotLiquid.Hash(), false), path) |> ignore)
Template.FileSystem <- fs :> DotLiquid.FileSystems.IFileSystem

let templateFor<'a> file variableName = parseTemplate<'a> (File.ReadAllText(Path.Combine(templatedir, file))) variableName

let archiveTemplate    = templateFor<Idea>     "_idea_archive.liquid"    "idea"
let submissionTemplate = templateFor<Idea>     "_idea_submission.liquid" "idea"


let sanitize (s : string) =
    let mods = [
        (fun s -> ["<";">";":";"\\";"/";"\"";"|";"?";"*";" ";"`";"'";"(";")";".";"#";] |> List.fold (fun (s : string) sep -> s.Replace(sep, "-")) s)
        fun s -> s.Substring(0, min s.Length 49) // uservoice limits links to files to 49 characters of the title, so we need to keep this constraint if we want linking to still work
        fun s -> s.ToLowerInvariant() // links are tolower in UserVoice
        fun s -> s.TrimEnd '-' // links don't end with -
        fun s -> let r = Regex "-+" in r.Replace(s, "-")
    ]
    List.fold (fun str f -> f str) s mods

/// generate a filename in the form "suggestion-%s-%s" idea.Number (sanitize idea.Title)
let ideaFileName (idea:Idea) =
    sprintf "suggestion-%s-%s" idea.Number (sanitize idea.Title)

/// create a realtive md link to the associated archive document in the form
/// "[archived comments](archive/%s.md#comments)\n" 
let archiveCommentLink (idea:Idea) =
    sprintf "[Archived Uservoice Comments](../tree/master/archive/%s.md#comments)\n" (ideaFileName idea)

let formatMarkdown (idea : Idea) : string * string =
    let sanitizedName = ideaFileName idea
    sprintf "%s.md" sanitizedName, archiveTemplate idea

