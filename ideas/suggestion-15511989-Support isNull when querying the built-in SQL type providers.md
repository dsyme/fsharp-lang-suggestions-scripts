# Idea 15511989: Support isNull when querying the built-in SQL type providers #

## Submitted by Loic Denuziere on 8/2/2016 12:00:00 AM

## 5 votes

Right now the query expression-to-LINQ translation doesn't support queries such as `query { for x in table do where (isNull x.Field) }`. Instead we have to use `where (x.Field = null)`. That's quite inconsistent: in normal (non-query) code, `isNull` is advised, but in query code, we can't use it.




## Comment by Daniel Robinson on 8/4/2016 10:28:00 AM

Even better IMO, optimize comparison operators' handling of null so isNull isn't needed (anywhere).

