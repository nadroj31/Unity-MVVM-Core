# Unity MVVM Core

A lightweight MVVM (Model–View–ViewModel) infrastructure designed specifically for Unity projects.

This repository provides a minimal, dependency-free MVVM core focused on explicit data flow, clear responsibilities, and safe lifecycle management within Unity.

## Design Goals

- No external dependencies
- No UniRx or reactive frameworks
- Explicit and predictable data binding
- Clear separation between View and ViewModel
- Safe event subscription and unsubscription
- Suitable for mobile and PC Unity projects

This implementation intentionally avoids over-engineering and favors readability and control.

## Architecture Overview

This MVVM implementation follows three core principles:

1. ViewModels are pure C# objects without Unity dependencies
2. Views are responsible only for presentation and user interaction
3. Data flow is explicit and unidirectional from ViewModel to View

## Core Components

### Model (ViewModel)

All ViewModels inherit from a common base class:

```csharp
public abstract class ModelBase
{
}
```
ViewModels expose state using `BindableProperty<T>` fields.


### `BindableProperty<T>`

A thread-safe observable value container.

Key characteristics:

- Explicit value change notifications
- Thread-safe get and set
- No Unity dependency

Example usage inside a ViewModel:

```csharp
public readonly BindableProperty<int> Health = new(100);
```
Internally, value changes trigger a strongly typed event.


### View

Views are Unity `MonoBehaviour` components that inherit from a generic `ViewBase<T>`.

```csharp
public abstract class ViewBase<T> : MonoBehaviour
    where T : ModelBase
{
    // ...
}
```
Responsibilities:

- Hold the binding context (ViewModel)
- Manage binding and unbinding lifecycle
- Forward property changes to the view logic

The view lifecycle is explicitly controlled and safe against memory leaks.


### `PropertyBinder<T>`

`PropertyBinder<T>` is responsible for connecting ViewModel properties to view handlers.

Responsibilities:

- Locate bindable properties on the ViewModel
- Subscribe to value change events
- Unsubscribe automatically when the View or ViewModel changes
- Centralize binding logic to avoid scattered event handling

Example binding usage inside a View:

```csharp
Binder.Add<int>(
    nameof(MyViewModel.Health),
    OnHealthChanged
);
```
This approach keeps bindings explicit and avoids hidden or implicit magic.

## Typical Usage Flow

1. A View is instantiated by Unity
2. The BindingContext (ViewModel) is assigned
3. The View initializes and subscribes to ViewModel changes
4. Property updates propagate via `BindableProperty`
5. The View is destroyed and all bindings are safely released

This ensures:

- No dangling event subscriptions
- Clear ownership of lifecycle


## Folder Structure

```text
Runtime/
├─ Core        // Core MVVM primitives
├─ Binding     // Binding infrastructure
└─ View        // Unity-facing view abstractions
```
Samples are intentionally separated to keep the runtime clean.


## Why Not UniRx / Reactive Frameworks?

This project is designed for teams that:

- Prefer explicit data flow
- Want predictable performance
- Avoid over-engineering for simple UI or gameplay states

It is especially suitable for:

- UI systems
- Game state presentation
- Tooling and in-game editors


## License

MIT License.

Use freely in commercial and non-commercial projects.
