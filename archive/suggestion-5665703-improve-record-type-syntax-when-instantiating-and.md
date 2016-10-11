# Idea 5665703: Improve Record Type syntax when instantiating and another type has the same properties #

### Status : declined

### Submitted by Jorge Fioranelli on 3/21/2014 12:00:00 AM

### 3 votes

type UserInfo { Id: int; Name: string }
type CompanyInfo { Id: int; Name: string }
let user = new { UserInfo.Id = 1; UserInfo.Name = "John Doe" }
Having to specify the type in each property takes more time, specially if the type contains several ones.
Proposed solution (like C#):
let user = new UserInfo { Id = 1; Name = "John Doe" }



## Response 
### by fslang-admin on 9/16/2014 12:00:00 AM

Declined at Jorge’s request

------------------------
## Comments


## Comment by Daniel Fabian on 3/22/2014 3:03:00 AM
But this already works. Just annotate the whole record with the type and you are done.
let user = { Id = 1; Name = "John Doe" } : UserInfo or use the annotation at the place where you
are using user as in
myFuncThatNeedsAUser user (might already be inferred correctly now) or else
myFuncThatNeedsAUser (user : UserInfo)


## Comment by Jorge Fioranelli on 3/22/2014 10:04:00 AM
Thanks Daniel, I didn't know you can do that.


## Comment by Jorge Fioranelli on 3/22/2014 10:05:00 AM
Please remove this idea.

