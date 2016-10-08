# Idea 11356293: Allow implementation of abstract slots with generic return type instantiated at type 'unit' #

## Submitted by Eric Stokes on 1/8/2016 12:00:00 AM

## 7 votes

The behavior described here,
http://stackoverflow.com/questions/26296401/why-is-unit-treated-differently-by-the-f-type-system-when-used-as-a-generic-i
is quite surprising to someone coming from other typed FP languages. The fact that a generic type parameter can't be unit makes the whole generics abstraction feel a bit leaky and hacky, which isn't great publicity, as F# actually has a lot of great ideas.
In practice this comes up when implementing type indexed values of various sorts, as an interface is an ideal and natural way to do that, and of course one often wants to have a sometype<unit> value. The compiler error is rather surprising as well, as it implies the object expression doesn't implement the interface, which of course isn't the case.



