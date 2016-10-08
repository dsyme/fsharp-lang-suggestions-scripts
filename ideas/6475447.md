# Idea 6475447: Fix ReflectedDefinition for top-level pattern-matched let bindings #

## Submitted by Loic Denuziere on 9/23/2014 12:00:00 AM

## 9 votes

Right now, when you do something like this:
[<ReflectedDefinition>]
let (a, b) = (1, 2)
The compiled result is roughly equivalent to:
let private generatedIdent = (1, 2)
[<ReflectedDefinition>] let a = fst generatedIdent
[<ReflectedDefinition>] let b = snd generatedIdent
As you can see, the actual expression (1, 2) doesn't get reflected. This makes such definitions unusable for most use cases of ReflectedDefinition. As far as I can tell, this could be fixed by simply adding a ReflectedDefinition of generatedIdent.




