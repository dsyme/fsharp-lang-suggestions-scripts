# Idea 12920034: Add constraints to match FSharpType.Is* predicates #

## Status : open

## Submitted by Matthew Orlando on 3/13/2016 12:00:00 AM

## 1 votes

I have a type, CaseSet<'a>, that is only meant to be used with descriminated unions. The only way I've found to enforce this is at runtime using FSharpType.IsUnion. I considered using type providers, but those aren't supported in PCLs.
I propose adding the following constraints based on the corresponding FSharpType reflection functions.
function : IsFunction
tuple : IsTuple
union : IsUnion
record : IsRecord
module : IsModule
exception : IsExceptionRepresentation
(I'm not really sure about that last one... I included it for completeness but maybe just testing for some subclass is sufficient?)


------------------------
## Comments

