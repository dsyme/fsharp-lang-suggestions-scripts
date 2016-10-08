# Idea 15448122: Add a warning for new keyword used on types which are not IDisposable #

## Status : 

## Submitted by Reed Copsey, Jr. on 7/28/2016 12:00:00 AM

## 21 votes

This is an alternative to: [/ideas/suggestion-15257796-remove-warning-for-new-keyword-on-idisposable](/ideas/suggestion-15257796-remove-warning-for-new-keyword-on-idisposable.md)
The idea is that the new keyword provides valuable information, but only if you do not use it on all types.
When avoiding its usage, you get a visual clue as to instances of disposable types, as well as warnings if you bind them.
By making it a warning to use new unnecessarily, the compiler would effectively enforce a "best practice" with regards to IDisposable usage. It goes a long way today, but requires discipline to make it useful.
This would be especially helpful to people coming to F# from C#, as many immediately use new everywhere, and don't see this very nice safety benefit provided by the compiler.

