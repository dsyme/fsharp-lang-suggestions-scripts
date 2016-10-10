module FslangMigration.Templating

open DotLiquid
open FSharp.Reflection
open System.IO
open System.Text.RegularExpressions

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
let templatedir = Path.GetFullPath "../templates/"
let fs = DotLiquid.FileSystems.LocalFileSystem(templatedir)
// next line tests tempalte file discovery
//["'idea'"; "'idea_comment'";"'idea_response'";"'idea_submission'"]
//|> List.iter (fun path -> fs.ReadTemplateFile(DotLiquid.Context(ResizeArray(), DotLiquid.Hash(), DotLiquid.Hash(), false), path) |> ignore)
Template.FileSystem <- fs :> DotLiquid.FileSystems.IFileSystem
let templateFor<'a> file variableName = parseTemplate<'a> (File.ReadAllText(Path.Combine(templatedir, file))) variableName
let wholeTemplate = templateFor<Idea> "_idea.liquid" "idea"
let ideaTemplate = templateFor<Idea> "_idea_submission.liquid" "idea"
let responseTemplate = templateFor<Response> "_idea_response.liquid" "response"
let commentTemplate = templateFor<Comment> "_idea_comment.liquid" "comment"
let sanitize (s : string) =
    let mods = [
        (fun s -> ["<";">";":";"\\";"/";"\"";"|";"?";"*";" ";"`";"'";"(";")";".";"#";] |> List.fold (fun (s : string) sep -> s.Replace(sep, "-")) s)
        fun s -> s.Substring(0, min s.Length 49) // uservoice limits links to files to 49 characters of the title, so we need to keep this constraint if we want linking to still work
        fun s -> s.ToLowerInvariant() // links are tolower in UserVoice
        fun s -> s.TrimEnd('-') // links don't end with -
        fun s -> let r = Regex("-+") in r.Replace(s, "-")
    ]
    
    List.fold (fun str f -> f str) s mods
    

let formatMarkdown (idea : Idea) : string * string =
    let sanitizedName = sprintf "suggestion-%s-%s" idea.Number (sanitize idea.Title)
    sprintf "%s.md" sanitizedName, wholeTemplate idea

