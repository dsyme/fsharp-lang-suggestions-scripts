# Idea 5664634: Provided Record Types #

## Status : declined

## Submitted by Anonymous on 3/21/2014 12:00:00 AM

## 1 votes

The included type providers should have the option of emitting F# record types rather than class types.
For example, when reading and writing SQL data, your F# code has to either deal with classes, which don't have the nice benefits of F# records (immutability, copy/update syntax, code that is easier to reason about, fewer bugs, etc.), or you have to map your classes to corresponding F# records, do your processing, and then convert back to classes.
Having the option to provide F# record types would really bring the power of the language closer to the data.
(see http://visualstudio.uservoice.com/forums/121579-visual-studio/suggestions/5576101-the-included-type-providers-should-have-the-option, moved from there to here per Visual Studio Team)

## Response by fslang-admin on 3/27/2014 12:00:00 AM

This is covered by existing suggestion http://fslang.uservoice.com/forums/245727-f-language/suggestions/5663267-allow-to-generate-dus-and-records-in-type-provider
Declining to allow votes to be recycled

