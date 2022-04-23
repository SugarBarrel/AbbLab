# AbbLab

This is a series of libraries, that I'm doing mainly for myself.

## AbbLab.SemanticVersioning

Provides classes and methods for working with [semantic versions](https://semver.org/spec/v2.0.0.html).

Features:

- [x] `SemanticVersion` with equality and comparison operators and parsing methods.
- [x] `SemanticPreRelease` helper struct.
- [x] `SemanticOptions` with a lot of various parsing options.
- [x] `BuildMetadataComparer` for comparing versions with build metadata.
- [x] `SemanticVersionBuilder` for building, modifying and incrementing versions.

To-do list:

- [ ] Formatting versions and pre-releases.
- [ ] Partial versions. ([`node-semver`](https://github.com/npm/node-semver))
- [ ] Version ranges. (`node-semver`)
  - [ ] Min/max versions.
  - [ ] Union `|`.
  - [ ] Intersection `&`.
  - [ ] Absolute complement `~` (and relative complement?).
  - [ ] Simplify.
  - [ ] Is subset/superset.
- [ ] Coercing? (`node-semver`)
- [ ] Diffing versions? (`node-semver`)
- [ ] Decrementing versions?

## AbbLab.Extensions

Provides useful extension methods and utility classes:

- Array extensions, so you can use `arr.ConvertAll(action)` instead of `Array.ConvertAll(arr, action)`. I don't know why they didn't do it like that in the first place. There's also a couple of extra ones: `Contains`, `Cast`, `OfType` and `Shuffle`.
- Enumerable extensions: `WithMin` (`WithMax`), `WithMinOrDefault` (`WithMaxOrDefault`) and `WithDistinct`. I found myself quite a lot in situations that needed that kind of LINQ methods.
- `ReadOnlyCollection.Empty<T>()` returns an empty read-only collection. Works just like `Array.Empty<T>()` and helps save some memory.
- `ReverseComparer<T>` class to sort collections in descending order.
- `Util.Fail<T>(out result)` helper method proves to be useful in parsing methods with a lot of branches. It sets `result` to its `default` value, and returns `false`.
