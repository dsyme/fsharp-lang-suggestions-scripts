# Idea 7916448: AddObsoleteAttribute method on ProvidedStaticParameter #

## Submitted by Dmitry Morozov on 5/11/2015 12:00:00 AM

## 3 votes






## Comment by Don Syme on 6/9/2015 1:39:00 PM

This is entirely reasonable and we would accept a PR to implement this.
Don Syme

## Comment by luketopia on 7/31/2015 7:07:00 AM

I'm confused about what is being suggested here. Are we talking about the ProvidedStaticParameter that's internal to FSharp.Data.TypeProviders.dll? Most type providers I've seen provide their own implementation of ProvidedStaticParameter via their ProvidedTypes.fs. Should an issue be opened on FSharp.TypeProviders.StarterPack instead?

