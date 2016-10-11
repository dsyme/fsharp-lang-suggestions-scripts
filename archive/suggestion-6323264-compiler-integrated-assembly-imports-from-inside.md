# Idea 6323264: Compiler integrated assembly imports from inside .fs files #

## Status : declined

## Submitted by Daniel Bradley on 8/20/2014 12:00:00 AM

## 3 votes

Would it be possible to integrate things like package management into the F# compiler without tying it directly to NuGet using something similar in concept to type providers, but for importing assemblies from (potentially) remote locations? I would imagine the usage of some specific examples of assembly providers might look something like:
open Github<"fsharp/fsharpx","1.2.3beta">
or
open NuGet<"fsharpx","1.2.3">
The compiler would be agnostic to any specific platform, such as NuGet, but would provide an API, much like for type providers, to provide assemblies.



## Response by fslang-admin on 9/16/2014 12:00:00 AM

This is effectively a duplicate of [/archive/suggestion-5670137--package-directive-to-import-nuget-packages-in-f](/archive/suggestion-5670137--package-directive-to-import-nuget-packages-in-f.md)
The ideas aren’t identical. but it seems best to consider them as one.

------------------------
## Comments

