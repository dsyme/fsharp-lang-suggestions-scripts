# Idea 6368913: Add Array.foralli #

## Status : declined

## Submitted by Frank A. Krueger on 8/30/2014 12:00:00 AM

## 3 votes

I seem to be joining two arrays of varying types quite often to do comparisons.
For example:
let nums = [| 0; 1 |]
let alphas = [| "a"; "b" |]
let compare n a = ...
I would like:
alphas |> Array.foralli (fun i a -> compare nums.[i] a)

## Response by fslang-admin on 7/18/2015 12:00:00 AM

Combining with http://fslang.uservoice.com/forums/245727-f-language/suggestions/6952125-create-filteri-functions-for-the-various-collect and treating that one as canonical as it has more votes


## Comment by Loic Denuziere on 9/3/2014 7:05:00 AM

Note that you can use `Array.forall2` in your particular case.
