# Idea 11149770: Automatic lazy wrapping of argument #

## Status : declined

## Submitted by Anonymous on 12/19/2015 12:00:00 AM

## 8 votes

Add such that arguments are automatically wrapped in the lazy expression, if set as the type.
for instance:
let (-->) q (p : Lazy<bool>) = not q || (p.Force())
right now i have to write:
false --> lazy (1/0 = 0)
why not make lazy automatic so it becomes:
false --> 1/0 = 0 : True instead of Divide by Zero exception
(while you are at it add a force operator)

## Response by fslang-admin on 2/4/2016 12:00:00 AM

Declined per my comment below
Don Syme, F# Language and Core Library Evolution


## Comment by Don Syme on 2/4/2016 4:17:00 PM

We considered this for F# 1.0. In the end we decided against it, partly because of code-readability reasons, and partly because "Lazy" values are not free in F# - they cost a closure and an allocation.
I don't think we will revisit the decision at this stage.
