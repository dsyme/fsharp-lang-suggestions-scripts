# Idea 5664936: Implement IReadOnlyCollection<'T> in list<'T> #

## Submitted by Petr Onderka on 3/21/2014 12:00:00 AM

## 3 votes

.Net 4.5 has a new type IReadOnlyCollection<'T> and list<'T> (a.k.a. FSharpList<'T>) fits this interface precisely, so I think it should implement it.
Original on VS uservoice: http://visualstudio.uservoice.com/forums/121579/suggestions/2902147




## Comment by Mauricio Scheffer on 3/24/2014 7:46:00 PM

Workaround:
module List =
let asReadOnlyList (this: _ list) =
{ new System.Collections.Generic.IReadOnlyList<_> with
member x.GetEnumerator() = (this :> _ seq).GetEnumerator()
member x.GetEnumerator() = (this :> System.Collections.IEnumerable).GetEnumerator()
member x.Count = this.Length
member x.Item with get i = this.[i] }

## Comment by Brandon D'Imperio on 4/23/2015 12:39:00 PM

nice work-around Mauricio

## Comment by Don Syme on 7/18/2015 1:34:00 PM

This seems entirely reasonable (we really should have done it n the FSharp.Core 4.4.0.0 revision)

