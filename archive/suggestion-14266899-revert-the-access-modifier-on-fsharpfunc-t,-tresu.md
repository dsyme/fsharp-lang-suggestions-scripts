# Idea 14266899: Revert the access modifier on FSharpFunc<T, TResult> constructor to be protected again. #

## Status : declined

## Submitted by Mikkel Christensen on 5/27/2016 12:00:00 AM

## 1 votes

It has been changed to public in 4.4, which is a rather odd construct for an abstract class.



## Response by fslang-admin on 6/13/2016 12:00:00 AM

Declined per my comment below
Don Syme
F# Language Evolution

------------------------
## Comments


## Comment by Don Syme on 6/13/2016 5:50:00 AM
I don't particularly recall why this change was made, but I don't think we'll change it back now for the sake of stability.

