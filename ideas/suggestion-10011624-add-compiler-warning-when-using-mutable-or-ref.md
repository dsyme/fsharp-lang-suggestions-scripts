# Idea 10011624: Add compiler-warning when using mutable or ref #

## Status : declined

## Submitted by Wael on 10/1/2015 12:00:00 AM

## 7 votes

add compiler flag: --warn-mutable, --warn-ref which would generate warning when using mutable (either mutable keyword or when using ref).
This would allow pure modules to be made.
Note that F# doesn't have any "pure" tags on member methods like the "const" qualifier on C++ methods.

## Response by fslang-admin on 1/23/2016 12:00:00 AM

See comment above. Declined since we’d prefer this to be implemented in tools such as FSharpLint.
Don Syme, F# Language and Core Library Evolution


## Comment by Radek Micek on 10/19/2015 2:57:00 PM

> This would allow pure modules to be made.
I don't see how it helps since you can still use some type from referenced assembly which is equivalent to ref.
BTW this can be implemented in linter (eg. VFPT).

## Comment by knocte on 1/22/2016 4:26:00 AM

FYI I suggested something similar to this in a comment to this other idea: http://fslang.uservoice.com/forums/245727-f-language/suggestions/5670335-pure-functions-pure-keyword
NOTE: you can still use mutable locals (as oppoosed to fields), in immutable code (because local vars in a method cannot be shared among threads).

## Comment by Don Syme on 1/23/2016 11:52:00 AM

I would prefer if this were implemented in FSharpLint. I will decline this for the core language and please add a suggestion there, if it isn't available already.
