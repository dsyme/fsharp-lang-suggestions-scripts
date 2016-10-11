# Idea 5663298: Improve optional parameter interop between F# and C# #

## Status : planned

## Submitted by Gustavo Guerra on 3/21/2014 12:00:00 AM

## 20 votes

It would be nice if the F# compiler automatically inserted [<Optional;DefaultParameterValue(null)>] in all optional parameters of methods declared in F# classes, so they would be easier to use from C#
It would also be nice if the constructors of records also used this for parameters of type Option<_>



## Response by fslang-admin on 8/3/2015 12:00:00 AM

This proposal is “approved in principle” for F# 4.x+
If you would like to submit an implementation and testing, please submit to the appropriate branch of http://github.com/Microsoft/visualfsharp. See http://fsharp.github.io/2014/06/18/fsharp-contributions.html for details about contributing to the F# language and core library
Don Syme, F# Language and Core Library Evolution

------------------------
## Comments


## Comment by Gustavo Guerra on 3/21/2014 8:26:00 AM
Original item: http://visualstudio.uservoice.com/forums/121579-visual-studio/suggestions/4519990-improve-optional-parameter-interop-between-f-and

