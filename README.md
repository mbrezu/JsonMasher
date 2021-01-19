# JsonMasher

A project to study [jq](https://stedolan.github.io/jq/) by re-implementing it in C# 9.0/.NET 5.0 as
a compiler-to-object-graphs (is there a better name for the object oriented version of [compilation
to closure trees](https://xach.livejournal.com/131456.html)?).

## Get started

Visit [jsonmasher.mbrezu.me](https://jsonmasher.mbrezu.me) for a web playground.

To get started with using JsonMasher in your own projects, look at the "end to end" tests, for
instance [Simple.cs](JsonMasher.Tests/EndToEnd/Simple.cs). "End to end" in this case means it tests
both the compiler and the resulting object tree.

The most important interface: [IJsonMasherOperator.cs](JsonMasher/Mashers/IJsonMasherOperator.cs).

Json representation used internally: [Json.cs](JsonMasher/JsonRepresentation/Json.cs). `System.Text.Json`
`JsonElement`/`JsonDocument` can be converted to `Json`, and `Json` can be pretty printed. Did not
add a conversion for [Newtonsoft Json](https://www.newtonsoft.com/json) to avoid the extra
dependency, but it shouldn't be hard to add.

A CLI interface project is [JsonMasher.Cli](JsonMasher.Cli).

A Web UI project is [JsonMasher.Web](JsonMasher.Web).

Some simple benchmarks in [JsonMasher.Benchmarks](JsonMasher.Benchmarks).

## What is missing

Short answer:

- some jq features; most notably, destructuring bind, modules and regular expressions (see the TODO link below)
- a Nuget package
- debugging facilities (`debug` is there, but I want more)

Long answer: [TODO JQ manual](TODO.md#implementation-status-of-jq-manual-features). Also see
examples in [jsonmasher.mbrezu.me](https://jsonmasher.mbrezu.me) for what is implemented.

## Performance

With the simple benchmarks in [JsonMasher.Benchmarks](JsonMasher.Benchmarks), it seems that
JsonMasher is about 1.5 times slower than jq, but these are only a handful of microbenchmarks...

I made no attempt at optimization and instead wrote the simplest code I could. So the performance
seems surprisingly good to me, so far. I guess this means the JIT people did an amazing job :-)

## License

[2-clause BSD](https://en.wikipedia.org/wiki/BSD_licenses#2-clause_license_.28.22Simplified_BSD_License.22_or_.22FreeBSD_License.22.29), see the [LICENSE](./LICENSE) file.
