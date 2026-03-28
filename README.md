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

### ViewModel

All ViewModels inherit from `ViewModelBase`:

```csharp
public abstract class ViewModelBase { }
```

ViewModels expose state as `public readonly` `BindableProperty<T>` fields:

```csharp
public class PlayerViewModel : ViewModelBase
{
    public readonly BindableProperty<int> Health = new(100);
    public readonly BindableProperty<string> Name = new("Player");
}
```

> `PropertyBinder<T>` resolves bindings via reflection on **public fields**.
> Properties (`{ get; }`) are not supported.


### `BindableProperty<T>`

A thread-safe observable value container.

Key characteristics:

- Fires `ValueChanged` only when the new value differs from the current one
- Thread-safe get and set; event is invoked outside the lock to prevent deadlocks
- Uses `EqualityComparer<T>.Default` — no boxing for value types
- No Unity dependency

Example usage inside a ViewModel:

```csharp
public readonly BindableProperty<int> Health = new(100);
```


### `IView<T>`

The View contract. Requires only a settable `BindingContext`:

```csharp
public interface IView<T> where T : ViewModelBase
{
    T BindingContext { get; set; }
}
```


### View

Views are Unity `MonoBehaviour` components that inherit from `ViewBase<T>`, which implements `IView<T>`:

```csharp
public abstract class ViewBase<T> : MonoBehaviour, IView<T>
    where T : ViewModelBase
{
    // ...
}
```

Responsibilities:

- Hold the ViewModel via `BindingContext`
- Manage binding and unbinding lifecycle
- Forward property changes to presentation logic

The view lifecycle is explicitly controlled and safe against memory leaks.

Override `OnInitialize` to register bindings, then override `OnBindingContextChange` for any custom swap logic:

```csharp
public class PlayerView : ViewBase<PlayerViewModel>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Binder.Add<int>(nameof(PlayerViewModel.Health), OnHealthChanged);
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        // update UI
    }
}
```


### `PropertyBinder<T>`

`PropertyBinder<T>` connects ViewModel fields to View handlers.

Responsibilities:

- Locate `BindableProperty<T>` fields on the ViewModel via reflection
- Subscribe all registered handlers when `Bind` is called
- Unsubscribe all registered handlers when `Unbind` is called
- Centralize binding logic to avoid scattered event handling

```csharp
Binder.Add<int>(nameof(PlayerViewModel.Health), OnHealthChanged);
```

`Add` resolves the field once by name and caches the `FieldInfo`. Subsequent `Bind`/`Unbind` calls reuse that cache.


## Typical Usage Flow

1. A View is instantiated by Unity
2. `BindingContext` is assigned, triggering `OnInitialize` on the first call
3. The View subscribes to ViewModel property changes via `Binder`
4. Property updates propagate through `BindableProperty.ValueChanged`
5. When the ViewModel is swapped, the old bindings are released and new ones applied
6. On `OnDestroy`, `BindingContext` is set to `null` first (releasing all bindings), then the internal event subscription is removed

This ensures:

- No dangling event subscriptions
- Clear ownership of lifecycle


## Folder Structure

```text
Runtime/
├─ Core        // BindableProperty<T> and ViewModelBase
├─ Binding     // PropertyBinder<T>
└─ View        // IView<T> and ViewBase<T>
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
