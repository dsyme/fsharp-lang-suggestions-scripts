Run `.paket/paket.bootstrapper.exe`

Run `.paket/paket restore`

Run `.paket/paket generate-include-scripts`

Delete the include of FSharp.Core form the include.main.group.fsx file.

Run `uservoice.fsx` in FSI, which will generate the markdown files for uservoice entries in `/ideas`