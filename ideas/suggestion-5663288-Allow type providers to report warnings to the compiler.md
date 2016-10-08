# Idea 5663288: Allow type providers to report warnings to the compiler #

## Submitted by Gustavo Guerra on 3/21/2014 12:00:00 AM

## 11 votes






## Comment by Gustavo Guerra on 6/28/2014 1:26:00 PM

Maybe if the class that implements ITypeProvider has a public event named "WarningGenerated" or something like that, the compiler could plug into it and listen. It's a bit hack-y, but as there is a canonical implementation of ITypeProvider in TypeProviderForNameSpaces, we could create a method there called "EmitWarning", and hide how it's implemented, so there would not be a requirements for a specific FSharp.Core.
Unfortunately, I'm not remembering the case I was thinking about that I wanted to generate a warning on. I think it was in CsvProvider, but can't remember exactly

## Comment by exercitus vir on 6/19/2015 6:01:00 PM

I also think that this would allow for many more interesting use cases for type providers.
