# Idea 7017131: Make F# union types like "list" that have no subtypes 'sealed' #

## Status : planned

## Submitted by Don Syme on 1/27/2015 12:00:00 AM

## 1 votes

The compiled form of the "list" type in F# uses a single class. This is also the case for some other F# union types, e.g. ones which are enumerations where no case carries any data. For these types, the compiled representation of the union type should be marked "sealed" in F# metadata
This is a breaking change w.r.t. hypothetical clients in other .NET languages that take advantage of this, though we know of no such instance in practice.


## Response by fslang-admin on 8/3/2015 12:00:00 AM

Considering this approved for F# 4.x or the first possible update after this.
Discussion still very welcome.
Implementations of approved language design items can now be submitted as pull requests to the appropriate branch of http://github.com/Microsoft/visualfsharp. See http://fsharp.github.io/2014/06/18/fsharp-contributions.html for information on contributing to the F# language and core library.
Don Syme, F# Language/Library Evolution



