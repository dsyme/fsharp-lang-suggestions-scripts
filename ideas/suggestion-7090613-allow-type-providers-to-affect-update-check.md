# Idea 7090613: Allow type providers to affect update-check #

## Status : declined

## Submitted by Robert Jeppesen on 2/13/2015 12:00:00 AM

## 2 votes

In short, if a TP uses a file, and that file changes, the needs-update check has no way of knowing to rebuild the project.
Some suggestions here: https://github.com/Microsoft/visualfsharp/issues/234


## Comment by Don Syme on 7/18/2015 1:20:00 PM

Looking at the original issue, @latkin confirmed that using the "Content" item type should solve things. If that's not the case please add more detail to that issue.
