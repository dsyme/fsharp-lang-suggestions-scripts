# Idea 6027309: Allow pattern matching on ranges #

## Status : declined

## Submitted by Patrick Q on 6/9/2014 12:00:00 AM

## 19 votes

It would be quite useful to allow pattern matching on ranges as in the following:
let find x =
match x with
| 42 -> "aaa"
| 150 .. 180 -> "bbb"
| 222 .. 333 -> "ccc"
| _ -> "zzz"
While one can use guards or active patterns to provide equivalent functionality, it would be quite useful and result in cleaner code if pattern matching on ranges was natively supported.

## Response by fslang-admin on 6/20/2014 12:00:00 AM

I’m declining this because a general mechanism (active patterns) already exists that achieves the desired result. In F# 2.0 we decided to add the more general mechanism and remove special cases such as character range patterns (present in OCaml).
Language design is always a tradeoff, and adding new special syntactic cases for this case doesn’t seem appropriate. For example, we would have to consider ranges of other types including floating point numbers, decimals, user-defined numeric literals etc.
Don Syme, current BDFL for the F# Language Design

