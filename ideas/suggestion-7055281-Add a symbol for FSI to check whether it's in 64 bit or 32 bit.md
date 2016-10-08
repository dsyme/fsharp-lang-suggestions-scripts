# Idea 7055281: Add a symbol for FSI to check whether it's in 64 bit or 32 bit #

## Submitted by Max on 2/5/2015 12:00:00 AM

## 4 votes

Often I want to reference different libraries based on whether the FSI is running in 32 bit or 64 bit mode. I often have to test two versions of an assembly, and it's enough of a pain just changing the flag every time, let alone change the references. Being able to do:
#if 64_BIT
#r "My64BitLib.dll"
#else
#r "My32BitLib.dll"
#endif




## Comment by ADMIN
fsharporg-lang (F# Software Foundation Language Group, F# Software Foundation) on 7/17/2015 8:32:00 AM

This seems entirely reasonable for scripting code.
I'll mark it as "approved in principle", however the Visual F# Tools team and others may have opinions about how we deal with this kind of configuration parameter. Probably the best thing to do is send a PR to http://github.com/Microsoft/visualfsharp and continue the discussion there, then link back to this discussion.

