# Idea 15915547: Implement interface delegating #

## Status : 

## Submitted by Ivan J. Simongauz on 9/3/2016 12:00:00 AM

## 1 votes

Implement interface delegating by next syntax:
type MyType() =
let delegator : IAddingService
interface IAddingService by delegator with
member this.Add x y = // <- this is override
x + y
Special behavior when event in interface - sender substitution

