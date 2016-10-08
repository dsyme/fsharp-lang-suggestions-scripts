# Idea 7063652: provide reflected definitions by default #

## Status : declined

## Submitted by Carl Patenaude Poulin on 2/6/2015 12:00:00 AM

## 1 votes

Having access to all code by default would greatly improve the metaprogramming story of F#.

## Response by fslang-admin on 2/14/2015 12:00:00 AM

This is really already done – RefelctedDefinition is allowed on modules


## Comment by mikero on 2/11/2015 5:09:00 PM

Or at least using a namespace/module-level attribute or compile switch

## Comment by Don Syme on 2/13/2015 4:13:00 AM

As of F# 3.1 you can add the ReflectedDefinition attribute to a module.
Cheers
Don
