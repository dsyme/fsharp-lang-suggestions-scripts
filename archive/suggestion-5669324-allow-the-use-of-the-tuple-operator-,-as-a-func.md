# Idea 5669324: Allow the use of the tuple operator (,) as a function. #

## Status : declined

## Submitted by Anonymous on 3/23/2014 12:00:00 AM

## 53 votes

I've seen F# code like
fun x y = (x,y)
or
let tuple2 x y = (x,y)
used everywhere, I think it will be more elegant to allow to use the function (,) and (,,) and so on, as in Haskell.



## Response by fslang-admin on 2/3/2016 12:00:00 AM

Declining per my comment below.
Don Syme, F# Language Evolution

------------------------
## Comments


## Comment by Don Syme on 2/3/2016 1:03:00 PM
This seems too corner case to warrant addition to the language: it is simple enough to use a named function such as "tuple2"

