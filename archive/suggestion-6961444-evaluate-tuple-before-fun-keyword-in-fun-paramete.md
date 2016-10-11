# Idea 6961444: Evaluate tuple before fun-keyword in fun-parameters #

## Status : declined

## Submitted by Tuomas Hietanen on 1/14/2015 12:00:00 AM

## 11 votes

Why you do have to have brackets in fun parameters:
fun (x,y) -> x+y
fun x,y -> x+y
This "fun" with "->" is already a kind of brackets.



## Response by fslang-admin on 2/4/2016 12:00:00 AM

Thanks for the suggestion, it’s appreciated. I’ve marked it as declined per my comment below.
Many thanks
Don Syme
F# Language Evolution

------------------------
## Comments


## Comment by Don Syme on 2/4/2016 5:47:00 PM
I think this would be ambiguous, or at least futz with other aspects of the grammar. I will decline this since it would be a small improvement only.

