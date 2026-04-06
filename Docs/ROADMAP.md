# Lumi Roadmap & TODO

## Current Implementation Status

The Lumi language pipeline currently includes:

### ✅ Core Compiler Pipeline
- `Lumi.Lexer` — Tokenization
- `Lumi.Parser` — AST construction
- `Lumi.SemanticAnalyzer` — Semantic validation
- `Lumi.Bytecode` — Bytecode generation
- `Lumi.VM` — Bytecode execution
- `Lumi.Engine` — REPL and script runner
- `Lumi.AST` — AST node types

### ✅ Testing Framework
- Test projects for all major components (`*.Tests`)

---

## TODO: Short-term Improvements

Helpful next steps for the current implementation:

- [ ] **Exception handling in the VM**: Add structured exception objects, `try`/`catch` support in the language and proper error unwinding in the VM.
- [ ] **Better documentation**: Add XML docs and inline comments for public APIs, the bytecode format, and the VM instruction semantics.
- [ ] **Improve tests**: Add more unit tests for parser error recovery, complex expression precedence and control flow.
- [ ] **Expand bytecode**: Implement more instructions, efficient opcodes for common cases, and a compact encoding.
- [ ] **Constant pool improvements**: Deduplicate constants and add serialization support.
- [ ] **Variable scoping & closures**: Implement lexical scoping, closures and environment captures.
- [ ] **Debugging support**: Add debug info (mapping bytecode offsets to source positions), stack traces and a simple REPL debugger.
- [ ] **Performance & profiling**: Add benchmarks for the VM, then optimize hot paths or consider a JIT later.

---

## TODO: Medium-term Projects (Additional Components)

These projects would complete the language toolchain for end-users and developers:

### **User-Facing Tools**
- [ ] **Lumi.CLI** — Command-line compiler/interpreter tool for end users
  - Compile `.lumi` files to bytecode or execute directly
  - Multiple output modes (execute, compile-only, optimize)
  - Error reporting with source location details

- [ ] **Lumi.REPL** — Enhanced interactive Read-Eval-Print Loop
  - Multi-line input support
  - Command history and editing
  - Built-in help system
  - Performance metrics for debugging

- [ ] **Lumi.LSP** / **Lumi.LanguageServer** — Language Server Protocol implementation
  - IDE integration (VSCode, Visual Studio, etc.)
  - IntelliSense and code completion
  - Real-time error checking and diagnostics
  - Go-to-definition, find-references, rename refactoring

### **Runtime & Standard Library**
- [ ] **Lumi.Runtime** / **Lumi.Core** — Runtime library with built-in functions and types
  - Type system extensions
  - Memory management helpers
  - Garbage collection (if applicable)

- [ ] **Lumi.StandardLibrary** / **Lumi.Stdlib** — Standard library functions
  - String manipulation
  - Collections (arrays, lists, dictionaries)
  - Math functions
  - I/O and file operations
  - Date/time utilities

### **Developer Tools**
- [ ] **Lumi.Compiler** — Main compiler orchestration project
  - High-level API that coordinates Lexer → Parser → Semantic → Bytecode
  - Handles file I/O and project compilation
  - Plugin architecture for custom passes

- [ ] **Lumi.Optimizer** — Bytecode/IR optimization passes
  - Dead code elimination
  - Constant folding
  - Inlining
  - Peephole optimization

- [ ] **Lumi.Diagnostics** — Comprehensive error reporting
  - Structured error/warning/info messages
  - Source location tracking
  - Suggestion and hint system
  - Multiple output formats (JSON, plain text, colored console)

- [ ] **Lumi.Debugger** — Debugging support for the VM
  - Breakpoints and step-through execution
  - Variable inspection and stack traces
  - Watch expressions
  - Integration with IDE debuggers

- [ ] **Lumi.Tools** — Additional developer utilities
  - Code formatter/pretty-printer
  - Linter and style checker
  - Static analyzer
  - Profiler integration

### **Documentation & Examples**
- [ ] **Lumi.Samples** — Example programs written in Lumi
  - Hello World
  - Fibonacci, factorial, and other math examples
  - String manipulation
  - Control flow patterns
  - Scoping and variable examples

- [ ] **Lumi.Benchmarks** — Performance benchmarks
  - Microbenchmarks for common operations
  - Larger integration tests
  - Comparison with other scripting languages
  - Performance regression detection

---

## Minimal Viable Product (MVP) for Full Language Release

To create a complete, distributable language implementation, prioritize in this order:

1. **Lumi.CLI** — Users need a way to run programs
2. **Lumi.Compiler** — Orchestrates the full pipeline
3. **Lumi.StandardLibrary** — Language needs built-in functionality
4. **Lumi.Diagnostics** — Good error messages are essential
5. **Lumi.Samples** — Users need examples to learn from

---

## Long-term Vision

- **Platform expansion**: Compile to native code or WebAssembly
- **Package manager**: Ecosystem for third-party libraries
- **Cross-platform debugger**: Unified debugging experience
- **Performance**: Implement JIT compilation for hot code paths
- **Interop**: Call into C# / .NET libraries from Lumi code
