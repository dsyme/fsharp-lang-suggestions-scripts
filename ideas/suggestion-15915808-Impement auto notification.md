# Idea 15915808: Impement auto notification #

## Submitted by Ivan J. Simongauz on 9/3/2016 12:00:00 AM

## 1 votes

Impement auto notification chages for classes and records when implemented INotifyPropertyChanged or custom.
type A() =
let achaged name old new =
PropertyChanged(this, name)
member B : int 3 with get and set and notifyby achanged




