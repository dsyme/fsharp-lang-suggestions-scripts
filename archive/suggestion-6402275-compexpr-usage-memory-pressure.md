# Idea 6402275: compexpr usage memory pressure #

### Status : declined

### Submitted by Stanislaw Halik on 9/6/2014 12:00:00 AM

### 1 votes

Instantiating compexpr causes GC heap usage for all the DUs that are called as part of compexpr flow.
This automatic memory management caused by mere Bind/Return pessimizes certain workflows. No newtype infrastructure either.



## Response 
### by fslang-admin on 9/16/2014 12:00:00 AM

Declined as this is not a concrete design proposal.
Don Syme, Current BDFL F# Language/Library Evolution

------------------------
## Comments


## Comment by Don Syme on 9/6/2014 10:58:00 AM
Yes, though please convert this to a concrete design proposal.

