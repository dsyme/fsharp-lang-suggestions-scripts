# Idea 8007312: warn about unused namespace #

## Status : declined

## Submitted by Dmitry Morozov on 5/17/2015 12:00:00 AM

## 9 votes

It would be nice if F# compiler had a switch to warn about unused namespace similar to --warnon:1182 - unused bindings. This can be lower cost solution to what in Visual C# addressed by "Remove Unused Usings" refactoring.



## Response by fslang-admin on 7/17/2015 12:00:00 AM

Closing this. The “greying out unused namespaces” is available in the Visual F# Power Tools. The core implementation can be reused in other tools such as FSharpLint.
Further comments, use cases, information and discussion welcome
Don Syme, F# Language and Core Library Evolution.

------------------------
## Comments


## Comment by Vasily Kirichenko on 5/18/2015 12:15:00 AM
The closest thing is "Gray out unused opens" in Visual F# Power Tools. And it was tricky to implement.


## Comment by Don Syme on 6/9/2015 1:38:00 PM
As Vasily says, "grey out unused namespace" in the Visual F# Power Tools is close (and I think FCS-based tooling is the way we would always address for F#)

