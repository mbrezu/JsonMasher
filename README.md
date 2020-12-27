# JsonMasher

A project to study [jq](https://stedolan.github.io/jq/) by re-implementing it in C#/.NET 5.0.

## Get started

To get started with using JsonMasher in your own projects, look at the "end to end" tests, for
instance [Simple.cs](JsonMasher.Tests/EndToEnd/Simple.cs). "End to end" in this case means it tests
both the compiler and the resulting object tree.

## What is missing

- most of jq :-)
- a Nuget package
- proper error reporting
- debugging facilities (`debug` is there, but I want more)

## What is not missing

- a framework to implement most of jq
- some operators and a simple parser

# License

[2-clause BSD](https://en.wikipedia.org/wiki/BSD_licenses#2-clause_license_.28.22Simplified_BSD_License.22_or_.22FreeBSD_License.22.29), see the [LICENSE](./LICENSE) file.
