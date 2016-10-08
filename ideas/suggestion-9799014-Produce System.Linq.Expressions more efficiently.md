# Idea 9799014: Produce System.Linq.Expressions more efficiently #

## Submitted by Eugene Tolmachev on 9/17/2015 12:00:00 AM

## 30 votes

Most of .NET query providers are written in C# and use System.Linq.Expressions. While the conversion Expr->Expression is available it introduces significant penalty: http://stackoverflow.com/questions/32592811/why-is-f-expression-tree-generation-is-much-slower-than-c-can-it-be-made-fast




## Comment by Dzmitry Lahoda on 9/22/2015 12:44:00 PM

Also I want want to use API of Elastic NEST or NHibernate I have to map F# Expr to Linq manually losing some readability of queries.

## Comment by Eugene Tolmachev on 10/9/2015 12:42:00 PM

Alternatively, maybe introduce an argument attribute, similar to ReflectedDefinition, something that would tell the compiler to produce C#-compatible tree from a lambda at compile-time, rather than the code to perform the conversion at runtime?

## Comment by Don Syme on 2/3/2016 11:09:00 AM

I have updated the title to change the suggestion to be about performance.

