# Idea 11699604: Pattern matching on member defintions #

## Status : declined

## Submitted by Harald Steinlechner on 2/3/2016 12:00:00 AM

## 1 votes

when extending discriminated unions with member implementations we can introduce a fresh name for 'this' (usually this or x). This identifier however is syntactically not a pattern.
It would be nice (for irrefutable patterns) to match directly on this position, .e.g.:
type Test2 = Test2 of int * int with
member (Test(a,b)).Blub() = a + b
instead of:
type Test = Test of int * int with
member x.Blub() = let (Test(a,b)) = x in a + b
The benefit seems to be minor, but additionally my proposal improves uniformity of the language.
However i fear this introduces ambiguities in the parser....

## Response by fslang-admin on 2/3/2016 12:00:00 AM

Declined – see comment above
Don Syme, F# Language Evolution


## Comment by Don Syme on 2/3/2016 10:21:00 AM

Hi Harald,
Thanks for taking the time to write this suggestion.
We considered this in the F# design, but decided against it since we felt that code became substantially less readable when this formulation was used. I don't think we'll revisit the design decision at this point.
Best
Don Syme, F# Language Evolution
