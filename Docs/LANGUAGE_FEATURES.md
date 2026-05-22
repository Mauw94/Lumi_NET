# Lumi language features

This document tracks what Lumi can do today, where the rough edges still are, and which language areas are the most likely to grow next.

## Implemented today

| Area | Supported today | Notes |
| --- | --- | --- |
| Literals | Numbers, strings, booleans, `null`, `undefined` | Core literal forms are tokenized and parsed directly. |
| Variables | `let`, `var`, `const` | Optional type annotations use `:` and initialization uses `->`. |
| Expressions | Arithmetic, comparison, logical operators, unary `-` and `!`, assignment | Current operator support covers the core expression language used by the examples and tests. |
| Output | `print` | Simple built-in output statement. |
| Blocks and scope | `{ ... }` blocks with lexical scoping | Inner declarations can shadow outer names. |
| Conditionals | `if` / `else` | Works for both simple and nested branches. |
| Loops | `for i in start to end step n` | `step` is optional. |
| Functions | `fn`, parameters, calls, `return` | Functions are compiled to bytecode entry points and invoked by name. |
| Structs | `struct`, fields, methods, `this`, constructors, field access and mutation | Constructors support positional and named arguments. |
| Collections | Array/list literals, indexing, `add`, `remove`, `length`, `contains` | Current collection behavior is centered on list-style operations. |
| Standard library | Implicit prelude globals such as `File.readText(...)` and `File.writeText(...)` | The first stdlib surface is native/runtime-backed and available without `import`. |
| Types in syntax | Primitive annotations and parameterized forms such as `list<Car>` | The syntax exists today even though the type system is still evolving. |

## Syntax snapshot

### Variables

```lumi
let x: int -> 42;
var total -> 0;
const name: str -> "Lumi";
```

### Control flow

```lumi
if (x > 10) {
    print x;
} else {
    print 0;
}

for i in 0 to 10 step 2 {
    print i;
}
```

### Functions

```lumi
fn fib(n) {
    if (n <= 1) {
        return n;
    }

    return fib(n - 1) + fib(n - 2);
}
```

### Structs and collections

```lumi
struct Person {
    name: str;
    age: int;
}

let person: Person -> new Person(name: "Alice", age: 30);
let items: list<int> -> [1, 2, 3];

items.add(4);
print items.length();
print person.age;
```

### Standard-library file I/O

```lumi
File.writeText("input.txt", "1\n2\n3");
let contents: str -> File.readText("input.txt");
print contents;
```

## Current limits

The implementation is already end-to-end, but some language areas are still in-progress:

- keywords such as `while`, `switch`, `try`, `catch`, `async`, `await`, `import`, and `export` are reserved but not yet part of the executable feature set;
- collections are useful today, and the first prelude-based standard-library APIs now exist, but the broader stdlib surface is still small;
- the type system validates common cases, but deeper generic behavior, richer inference, and more advanced user-defined type features are still ahead;
- the current runtime is intentionally simple, so there is not yet a separate optimizer, debugger, package system, or module loader.

## Feature roadmap

### Near term

| Theme | Likely work |
| --- | --- |
| Control flow | Add `while`, `break`, and `continue` so loops are less dependent on the current range-style `for`. |
| Diagnostics | Improve parser recovery, semantic messages, and source-span reporting so language errors are easier to debug. |
| Collections | Round out list behavior with more predictable mutation, lookup, and printing semantics. |
| Struct ergonomics | Keep improving field initialization, methods, and constructor validation. |
| Examples/docs | Grow the examples folder together with the docs so every major feature has a small reference script. |

### Medium term

| Theme | Likely work |
| --- | --- |
| Standard library | Expand the implicit prelude with string, math, collection, and additional I/O helpers. |
| Modules | Introduce `import`/`export` semantics and a file-based compilation story. |
| Tooling | Split out a dedicated CLI and expand the REPL beyond the current host program. |
| Runtime features | Add closures, lexical captures, and a more complete function story. |
| Debug support | Add bytecode dumps, source-to-bytecode mapping, and stack traces. |

### Long term

| Theme | Likely work |
| --- | --- |
| Optimization | Introduce a cleaner IR or optimization layer for constant folding, dead-code cleanup, and peephole improvements. |
| Ecosystem | Package management, reusable libraries, and a more distributable language workflow. |
| Alternate targets | Native or WebAssembly backends once the language surface is more stable. |
| Interop | Carefully scoped .NET interop for host integration and library reuse. |

### Nice to haves

These are not required for the language to mature, but they would make Lumi much nicer to use:

- pattern matching for structs and literals;
- a formatter and lint rules tailored to Lumi syntax;
- an LSP server for completion, go-to-definition, and inline diagnostics;
- a browser playground backed by the existing compiler pipeline;
- a bytecode visualizer/disassembler for learning and debugging;
- lightweight incremental compilation for faster edit-run loops;
- richer benchmarking so VM and language changes can be compared over time.
