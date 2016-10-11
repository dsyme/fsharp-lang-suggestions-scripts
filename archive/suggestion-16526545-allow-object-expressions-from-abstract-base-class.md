# Idea 16526545: Allow Object Expressions from abstract base classes without members #

### Status : open

### Submitted by lr on 10/6/2016 12:00:00 AM

### 1 votes

http://stackoverflow.com/questions/8154730/object-expression-for-abstract-class-without-abstract-members
Object Expressions are a great feature to create instances of single-use interface / abstract class instances without polluting the namespace.
At the moment it is impossible to inherit from an abstract class which does not define any members.
If I have an abstract base class
[<AbstractClass>]
type Foo(i:int) = class end
then I would like to be able to exten it like this:
let foo = { new Foo(1) }
But this errors out with
Invalid object expression. Objects without overrides or interfaces should use the expression form 'new Type(args)' without braces.
The suggestion obviously doesn't work, since the base type is abstract.
The workaround is to define it like this:
let foo = { new Foo(1) with member __.ToString() = base.ToString() }


------------------------
## Comments

