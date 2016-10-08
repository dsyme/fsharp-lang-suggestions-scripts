# Idea 6531829: Allow TypeProviders to supply location with errors #

## Submitted by Robert Jeppesen on 10/7/2014 12:00:00 AM

## 6 votes

When a custom type provider fails, the type provider will typically throw, there is not much else to do.
I suggest to add the ability to add an error location when something fails. This would allow to pinpoint with squigglies where the error is in ie json/sql/whatever.
Perhaps the simplest non-breaking implementation would be to add a known exception type that contains a file location and a range. So the TP still just throws, but adds this information where possible.




## Comment by ADMIN
fsharporg-lang (F# Software Foundation Language Group, F# Software Foundation) on 10/30/2014 6:58:00 AM

See also this proposal [/ideas/suggestion-5663288-allow-type-providers-to-report-warnings-to-the-com,](/ideas/suggestion-5663288-allow-type-providers-to-report-warnings-to-the-com,.md) these should probably be combined
