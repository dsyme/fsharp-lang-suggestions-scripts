# Idea 6343373: Allow defining extern methods in a class #

## Submitted by Kevin Jones on 8/25/2014 12:00:00 AM

## 3 votes

Currently it appears that extern methods for platform invoke only work properly when used inside of a module (see this discussion: http://stackoverflow.com/questions/22275072/why-does-the-f-compiler-give-an-error-for-one-case-but-not-the-other) this limitation poses problems when organizing your code in an efficient manner and results in having multiple modules defined (for an example, see here: http://stackoverflow.com/questions/25004314/working-with-safehandles-in-f)
It would be nice if extern methods could be defined in classes (and work correctly) even if the scope of the method is always limited to the type itself.



