# Idea 12546540: Allow flexible types in default constructor constraint #

## Submitted by George on 3/1/2016 12:00:00 AM

## 2 votes

This would permit,
let ctor<'a when 'a:(new: unit -> #IWebDriver)> : unit -> IWebDriver = ...
versus
let ctor<'a when 'a:(new: unit- > 'a) and 'a :> IWebDriver> = fun () -> new 'a() :> IWebDriver




