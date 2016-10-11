# Idea 12469410: add DAG like source reference control feature #

### Status : declined

### Submitted by kusokuzeshiki kusokuzeshiki on 2/24/2016 12:00:00 AM

### 3 votes

F# has source reference limitation feature by source order, but it should be enhanced.
I think source references should be like DAG.
Now upper source code can be referenced by any lower source code, but I wanna control some group can and some not.
This may be enough if module has access control feature like C++'s friend against module.
This feature increase limitations but eliminates complex source relationship and make easier to grab source structure if it's used properly.



## Response 
### by fslang-admin on 2/25/2016 12:00:00 AM

Pretty much covered by [/archive/suggestion-10276974-allow-the-compiler-to-take-source-code-files-in-an](/archive/suggestion-10276974-allow-the-compiler-to-take-source-code-files-in-an.md)

------------------------
## Comments


## Comment by kusokuzeshiki kusokuzeshiki on 2/25/2016 8:45:00 AM
It's completely different thing.
The link one deduce limitation.
This increse limitation.
Still you think both are same idea?

