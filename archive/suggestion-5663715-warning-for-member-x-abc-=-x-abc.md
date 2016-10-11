# Idea 5663715: Warning for "    member x.ABC = x.ABC" #

### Status : declined

### Submitted by Don Syme on 3/21/2014 12:00:00 AM

### 1 votes

In F#, it can happen that renaming and replacing identifiers gives rise to the non-sensical
member x.ABC = x.ABC
While this is a special case, it is very obviously an error. It may be worth special-casing a warning for this.



## Response 
### by fslang-admin on 7/18/2015 12:00:00 AM

Declining, no one has voted for this, this belongs in tools like FSharpLint.

------------------------
## Comments

