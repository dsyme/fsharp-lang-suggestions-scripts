# Idea 5769706: The pattern name of Active Patterns can duplicate define #

## Status : declined

## Submitted by Anonymous on 4/13/2014 12:00:00 AM

## 3 votes

When input parameters different from each other, the pattern name of Active Patterns can duplicate define, It will be cool as below.
[<AutoOpen>]
module ActiveModule=
let (|HasError|HasNotError|) (input:QueryResult)=
if input.HasError then HasError (input:>ResultBase)
else HasNotError (input.ResultData)
let (|HasError|HasNotError|) (input: ExecuteResult) =
if input.HasError then HasError (input:>ResultBase)
else HasNotError input



## Response by fslang-admin on 6/20/2014 12:00:00 AM

Realistically, we’re not going to add this to the active pattern feature.
Type-directed overloading of this kind doesn’t fit well with the feature.
If active patterns could be defined on objects as members we might consider adding type-directed overloading like this.
Declining this for now to recycle votes.
Don Syme

------------------------
## Comments

