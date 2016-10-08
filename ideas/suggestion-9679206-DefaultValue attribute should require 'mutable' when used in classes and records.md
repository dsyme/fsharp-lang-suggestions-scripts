# Idea 9679206: DefaultValue attribute should require 'mutable' when used in classes and records #

## Submitted by Don Syme on 9/8/2015 12:00:00 AM

## 1 votes

For structs, adding the DefaultValue attribute to a val declaration (a field) results in a check that the field is mutable, since it doesn't make sense to use an immutable field which only ever has the default value.
[<Struct>]
type S() =
[<DefaultValue>] val x : C
AssemblyReader.fs(4304,23): error FS0880: Uninitialized 'val' fields must be mutable and marked with the '[<DefaultValue>]' attribute. Consider using a 'let' binding instead of a 'val' field.
For some reason, this condition is only checked for fields declared in structs. We should likewise give a warning for records and classes:
type C() =
[<DefaultValue>] val x : C
and
type R = { [<DefaultValue>] x : C; y : int }




