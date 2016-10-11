# Idea 12251217: Add non-empty collection versions (or a generic nonempty type?) to the core library #

## Status : declined

## Submitted by Anthony Lloyd on 2/8/2016 12:00:00 AM

## 3 votes

A class of empty collection bugs and checking code could be eliminated if there was standard library support for non-empty collections.
In a similar way to the option type it would make function signatures more explicit.



## Response by fslang-admin on 2/10/2016 12:00:00 AM

Thanks for the suggestion! See comment below

------------------------
## Comments


## Comment by Don Syme on 2/10/2016 10:51:00 AM
Our approach is that this doesn't need to be in the core library - you can define such a collection yourself. There are a lot of constrained types that could go in the F# core library, adding them to the core library would expand out the library surface area too much.

