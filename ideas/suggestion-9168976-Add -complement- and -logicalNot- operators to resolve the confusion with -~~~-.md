# Idea 9168976: Add "complement" and "logicalNot" operators to resolve the confusion with "~~~" #

## Submitted by Don Syme on 8/4/2015 12:00:00 AM

## 1 votes

When used on user-defined types, the ~~~ operator resolves to op_LogicalNot rather than the expected op_OnesComplement. See https://github.com/Microsoft/visualfsharp/issues/457#issuecomment-104900399 for a workaround.
This should be fixed FSharp.Core. An attempt to fix this transparently failed, see https://github.com/Microsoft/visualfsharp/pull/458.
A plan to address this going forward is at https://github.com/Microsoft/visualfsharp/pull/458#issuecomment-127711336 but will require an update to FSharp.Core.




