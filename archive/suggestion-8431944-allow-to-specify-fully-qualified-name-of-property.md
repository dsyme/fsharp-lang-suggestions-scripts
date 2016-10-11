# Idea 8431944: Allow to specify fully qualified name of property in `StructuredFormatDisplayAttribute` #

## Status : declined

## Submitted by exercitus vir on 6/16/2015 12:00:00 AM

## 1 votes

Currently, specifying the name of a property in `StructuredFormatDisplayAttribute` prepends the fully qualified name of the property named between {}.
I would like to use a property in `StructuredFormatDisplayAttribute` that is part of an interface, but this is not possible because interfaces are explicitly defined. So currently, I have to define a dummy property directly on the type and call the property of the interface from there.
It would be much nicer if I could specify {Interface.Property} or {FullyQualifiedNameOfInterface.Property} instead.



## Response by fslang-admin on 7/18/2015 12:00:00 AM

Treating this issue as covered by [/archive/suggestion-8432340-inherit-structuredformatdisplayattribute-specifi](/archive/suggestion-8432340-inherit-structuredformatdisplayattribute-specifi.md) to avoid too many micro issues related to the same general problem/feature area

------------------------
## Comments

