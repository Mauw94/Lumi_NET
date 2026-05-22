# Lumi (.NET)

Lumi is an educational programming language implemented in C#/.NET. The repository currently focuses on a readable end-to-end toolchain: source text is tokenized, parsed into an AST, validated, lowered to bytecode, and executed on a small stack-based virtual machine.

## Project overview

The codebase is organized as a set of focused projects rather than one large compiler assembly. That keeps the language pipeline easy to inspect, test, and evolve feature-by-feature.

| Area | Project | Responsibility |
| --- | --- | --- |
| Shared language metadata | `Lumi.Language` | Canonical keywords and shared language-level definitions. |
| Lexing | `Lumi.Lexer` | Converts source text into tokens with source positions. |
| Syntax model | `Lumi.AST` | AST nodes used by the parser, semantic analyzer, and bytecode generator. |
| Parsing | `Lumi.Parser` | Recursive-descent parser for statements, expressions, structs, and function syntax. |
| Semantics | `Lumi.SemanticAnalyzer` | Scope checks, symbol validation, type compatibility, and struct/function validation. |
| Code generation | `Lumi.Bytecode` | Emits instructions, constants, locals, function entry points, and struct metadata. |
| Runtime | `Lumi.VM` | Executes Lumi bytecode on a stack-based virtual machine. |
| Host | `Lumi.Engine` | REPL/script host built around an ordered execution pipeline. |
| Verification | `*.Tests` | Unit tests for each layer of the implementation. |
| Samples | `Examples` | Small Lumi programs used as examples and regression coverage. |

## Current architecture

The runtime path today is intentionally direct:

```text
Source
  -> Lumi.Lexer
  -> Lumi.Parser
  -> Lumi.SemanticAnalyzer
  -> Lumi.Bytecode
  -> Lumi.VM
```

The main architectural choices at the moment are:

1. `Lumi.Engine` composes execution as ordered pipeline steps, which keeps parse, analysis, bytecode generation, and execution easy to swap or extend.
2. `Lumi.Bytecode` owns the compiler-side state: constant pooling, local-slot assignment, jump patching, function addresses, and struct metadata.
3. `Lumi.VM` is a compact stack machine with array-backed variable storage and call frames, optimized for clarity over advanced runtime features.
4. The REPL reuses the VM, semantic analyzer, and bytecode generator across iterations, so interactive sessions can preserve state between inputs.

## Documentation map

- [Language features](./LANGUAGE_FEATURES.md) documents the syntax and runtime behavior implemented so far, plus feature-focused roadmap notes.
- [`Examples/`](../Examples/) contains small Lumi scripts that show the current syntax in practice.

## Roadmap

The separate roadmap document has been folded into the main docs so the current state and direction stay together.

| Horizon | Focus | Notes |
| --- | --- | --- |
| Now | Diagnostics, feature completeness, docs | Improve parser/semantic error quality, tighten collection and struct ergonomics, and keep architecture docs in sync with the code. |
| Next | Tooling and developer workflow | Add a dedicated CLI, richer REPL experience, disassembly/debug output, and better benchmarking hooks. |
| Later | Language growth | Expand the standard library, introduce modules/packages, improve function/runtime capabilities, and add more complete control-flow constructs. |
| Nice to have | Ecosystem polish | Formatter/linter support, LSP integration, a web playground, bytecode inspector, and lightweight optimization passes. |

## Architecture direction

The next architectural steps are likely to build on the current pipeline rather than replace it:

- keep `Lumi.Engine` as the orchestration layer;
- grow `Lumi.Bytecode` and `Lumi.VM` together as the executable core;
- add tooling projects beside the existing runtime instead of mixing them into the compiler layers;
- treat docs and examples as part of the language surface so new features are easier to understand and review.

## License

This project is licensed under the [MIT License](../LICENSE).
