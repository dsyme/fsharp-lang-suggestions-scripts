# Idea 12838959: Allow record types to implement interfaces just by adding the interface name when compatible #

## Status : 

## Submitted by Pierre Irrmann on 3/7/2016 12:00:00 AM

## 32 votes

When a record type already has all the members necessary to impement an interface, it could implement it without having to write the code.
For instance:
type IHasAnAge =
abstract member Age: int
type Person = {
Name : string
Age: int
} with interface IHasAnAge




