# Idea 7184398: Cartesian product function for collections #

## Status : planned

## Submitted by luketopia on 3/8/2015 12:00:00 AM

## 7 votes

It's often useful to compute the Cartesian product (cross join) of two collections. I always end up writing something like this:
let cross xs ys =
seq {
for x in xs do
for y in ys ->
x, y
}
I think it would be useful to have this in the standard collection modules.

## Response by fslang-admin on 2/4/2016 12:00:00 AM

I’m marking this approved-in-principle, see RFC FS-1002 https://github.com/fsharp/FSharpLangDesign/blob/master/RFCs/FS-1002-cartesian-product-for-collections.md
The function would need to be added to List, Array and Seq. I would prefer a different name – I suggest “allPairs”


## Comment by Don Syme on 6/9/2015 2:04:00 PM

The FSharp.Core 4.0.0.0 ship has sailed. It seems reasonable for FSharp.Core vNext, though I'd like to see more input on this., e.g. do other languages have this in core functional collection libraries, ,what naming do they use etc.?

## Comment by luketopia on 8/2/2015 7:59:00 AM

Python calls it "product". https://docs.python.org/2/library/itertools.html#itertools.product
