# Closures in Lumi: Runtime and Compiler Reference

## Overview

Closures are now implemented as a combination of:

1. **Function body**: the bytecode entry point to execute.
2. **Environment**: the captured outer state for one closure instance.
3. **Closure object**: a callable heap object that ties the function body to its environment.

At runtime, a closure is effectively:

```text
{ code pointer, captured state }
```

---

## When closures are created

Closures are created **at runtime when a nested function declaration executes inside an outer function activation**.

Example:

```lumi
fn outer() {
    let x -> 1;
    fn inner() { print x; }
    return inner;
}
```

What happens:

1. `outer` is called.
2. Its locals are created for this specific invocation.
3. Execution reaches `fn inner()`.
4. The VM executes `MakeClosure`.
5. A closure object is created and bound to the current captured state of this `outer` call.
6. That closure is stored in the local slot for `inner`.

Closures must be created at runtime because captures belong to a **specific activation**, not just to source code. Two calls to `outer()` must produce two closure instances with separate captured state.

---

## Why closures are created at declaration time

Nested functions are not globally callable definitions anymore. They are **runtime values** inside the enclosing function.

That means this:

```lumi
fn outer() {
    fn inner() { ... }
    inner();
}
```

needs `inner` to exist as a local callable value at runtime. The nested declaration therefore:

1. compiles the nested function body separately,
2. emits `MakeClosure`,
3. stores the produced closure in the local slot for `inner`.

---

## What changed in bytecode generation

## 1. Capture metadata became explicit

`FunctionDescriptor` now stores capture metadata using `CaptureBinding`.

Each capture records:

- the capture name,
- whether it comes from an outer **local slot**,
- or from an outer **capture slot**.

This supports multi-level nested closures.

---

## 2. Captured reads and writes are different from local reads and writes

Before, captured variables still used:

- `LoadVar`
- `StoreVar`

That was not sufficient for real closures.

Now:

- captured reads emit `LoadCapture`
- captured writes emit `StoreCapture`

So the bytecode now distinguishes between:

- normal local access
- captured outer access

---

## 3. Nested functions become local callable values

Inside a function, a nested function declaration now produces a local runtime value.

So nested functions no longer depend on the global function-name map when called from local scope.

---

## 4. New call path for runtime values

There are now two relevant call paths:

| Call kind | Instruction | Source |
|---|---|---|
| Top-level named function call | `CallFn` | global function address map |
| Local closure/function value call | `CallValue` | closure value loaded from stack/local |

This allows:

```lumi
let f -> outer();
f();
```

to work as real closure invocation.

---

## What is `HeapCellObject`

`HeapCellObject` is a mutable heap box around a value.

It exists because captures in Lumi are **live**, not snapshots.

Example:

```lumi
let x -> 1;
fn inner() { print x; }
x = 2;
```

If captures were copied by value, `inner` would still see `1`.

Instead, the runtime upgrades captured locals into shared mutable cells:

- outer local slot points to the cell
- closure environment points to the same cell

Then:

- outer assignment updates the cell
- closure read reads from the same cell

Both observe the same current value.

---

## What changed in variable access

Before, variable slots held direct values only.

### Previous behavior

- `StoreVar` wrote directly into `_variables[slot]`
- `LoadVar` read directly from `_variables[slot]`

### Current behavior

A variable slot may now hold either:

1. a plain value, or
2. a `HeapCellObject` reference

### `LoadVar`

`LoadVar` now reads through `ReadVariableSlot(slot)`:

- if the slot holds a plain value, return it
- if the slot holds a cell, return `cell.Value`

### `StoreVar`

`StoreVar` now writes through `WriteVariableSlot(slot, value)`:

- if the slot holds a plain value, replace it
- if the slot already holds a cell, mutate `cell.Value`

This is what keeps captured locals and closures synchronized.

---

## When a local becomes a cell

Locals are **not** boxed eagerly.

A local is boxed only when a closure is created and needs to capture it. That happens during `MakeClosure`, via `EnsureLocalCell(...)`.

This keeps ordinary locals cheap and only pays the heap-boxing cost for variables that actually escape into closures.

---

## What is the environment

The **environment** is the runtime object that stores the closure's captured references.

In this implementation, `HeapEnvironmentObject` contains the captured entries for one closure instance. Because captures are live, these entries are effectively shared capture cells.

Conceptually:

```text
environment = [cell_for_x, cell_for_y, ...]
```

The closure then means:

```text
run function F using environment E
```

---

## Why environment is a `Value`

The VM uses `Value` as the universal runtime representation for:

- stack values
- local variables
- heap references

Heap objects are passed around as `Value.FromHeapObject(handle)`.

So the environment is stored as a `Value` because:

1. it is a heap object,
2. the VM already knows how to root and pass heap references as `Value`,
3. GC traversal already works through `Value`.

This keeps the runtime representation consistent.

---

## Are we always in an environment

No.

The current environment is only present while executing closure-backed code.

### No environment

- top-level code
- ordinary global function calls via `CallFn`
- normal non-closure execution paths

### Environment active

- closure invocation via `CallValue`

When a closure is called:

1. the VM reads the `HeapClosureObject`
2. gets its function entry point
3. gets its environment
4. pushes a call frame
5. sets `_currentEnvironment`
6. jumps into the function body

On return, the previous environment is restored from `CallFrame`.

---

## What changed in `CallFrame`

`CallFrame` previously stored only:

- return address
- previous base pointer

It now also stores the **previous environment**.

That matters because closure calls can nest. Returning from a closure must restore not just the instruction pointer and base pointer, but also the caller's closure context.

---

## What happens during `MakeClosure`

At runtime, `MakeClosure(functionId)` now does roughly this:

1. Look up the function descriptor.
2. Read its capture list.
3. For each capture:
   - if it comes from an outer local slot:
     - ensure that local is boxed into a `HeapCellObject`
   - if it comes from an outer capture:
     - reuse the existing capture cell from the current environment
4. Create a `HeapEnvironmentObject` holding those shared cells.
5. Create a `HeapClosureObject(entryPoint, environment)`.
6. Push that closure value to the stack.
7. Store it in the nested function's local slot.

So the closure object is not just a function pointer. It is a function pointer plus the exact captured state needed to run later.

---

## Why the environment stores cells instead of raw values

Because capture semantics are live.

If the environment stored copied values:

- closures would only see snapshots
- later outer mutations would be invisible

Because it stores cells:

- outer code and closures share the same mutable boxes
- reads and writes stay consistent

---

## Semantic analyzer change

The semantic analyzer now accepts nested/local function identifiers as callable values.

That enables patterns like:

```lumi
fn outer() {
    fn inner() { }
    inner();
    return inner;
}
```

without treating `inner` as if it had to come only from the global function registry.

---

## Mental model

Use this compact model:

- **local variable**: plain VM slot
- **captured local**: VM slot upgraded to shared heap cell
- **environment**: list of shared cells for one closure
- **closure**: function entry point + environment
- **closure call**: run function with `_currentEnvironment = closure.environment`

---

## Follow-up ideas

- Add more tests for multi-level nested closures.
- Decide whether non-capturing nested functions should always be materialized as local runtime values for consistency.
- Consider renaming `HeapEnvironmentObject.Captures` to `CaptureCells` to better reflect the runtime model.
