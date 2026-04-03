# Unity MVVM Core

A lightweight MVVM (Model–View–ViewModel) infrastructure designed specifically for Unity projects.
The focus is on explicit data flow, clear responsibilities, and safe lifecycle management — not on framework magic.

> **Note:** This repository is the standalone MVVM core extracted from [Unity Match-3 Puzzle — Architecture Showcase](https://github.com/nadroj31/Unity-Match-Puzzle-Showcase). It is designed to be dropped into any Unity project as-is.

---

## Design Goals

- No external dependencies
- No UniRx or reactive frameworks
- Explicit and predictable data binding
- Clear separation between View and ViewModel
- Safe event subscription and unsubscription
- Suitable for mobile and PC Unity projects

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────┐
│                   View Layer                         │
│  (MonoBehaviours — inherit from ViewBase<T>)         │
└────────────────────┬────────────────────────────────┘
                     │ binds to
┌────────────────────▼────────────────────────────────┐
│                ViewModel Layer                       │
│  (plain C# — inherit from ViewModelBase)             │
└────────────────────┬────────────────────────────────┘
                     │ exposes state via
┌────────────────────▼────────────────────────────────┐
│              BindableProperty<T>                     │
│  (observable value container — no Unity dependency)  │
└─────────────────────────────────────────────────────┘
```

Data flow is **unidirectional**: ViewModel pushes state → View reacts. Views never write back to ViewModels directly.

---

## Core Components

| Class | Responsibility |
|---|---|
| `ViewModelBase` | Marker base class; all ViewModels inherit from it |
| `BindableProperty<T>` | Observable value wrapper — fires `ValueChanged` only on actual change, using `EqualityComparer<T>` |
| `ViewBase<T>` | Abstract `MonoBehaviour` base for all Views; manages binding lifecycle (initialize → bind → unbind → destroy) |
| `PropertyBinder<T>` | Resolves `BindableProperty` fields by name via reflection; caches `FieldInfo` for subsequent bind/unbind calls |

---

## Usage

### 1. Define a ViewModel

```csharp
public class PlayerViewModel : ViewModelBase
{
    public readonly BindableProperty<int> Health = new(100);
    public readonly BindableProperty<string> Name = new("Player");
}
```

> `PropertyBinder<T>` resolves bindings via reflection on **public fields**.
> Properties (`{ get; }`) are not supported.

### 2. Define a View

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

### 3. Assign the ViewModel

```csharp
playerView.BindingContext = new PlayerViewModel();
```

Assigning `BindingContext` triggers `OnInitialize` on the first call, then binds all registered handlers automatically.

---

## Lifecycle

1. A View is instantiated by Unity
2. `BindingContext` is assigned — `OnInitialize` runs once, registering all bindings via `Binder`
3. `Binder.Bind` subscribes all handlers to the ViewModel's `BindableProperty` fields
4. Property updates propagate through `BindableProperty.ValueChanged`
5. When the ViewModel is swapped, old bindings are released and new ones applied
6. On `OnDestroy`, `BindingContext` is set to `null` first (releasing all bindings), then the internal subscription is removed

This ensures:
- No dangling event subscriptions
- Clear ownership of lifecycle

---

## BindableProperty Details

- Fires `ValueChanged` only when the new value **differs** from the current one
- Thread-safe get and set; event is invoked outside the lock to prevent deadlocks
- Uses `EqualityComparer<T>.Default` — no boxing for value types
- No Unity dependency

---

## Folder Structure

```
Runtime/
├── Core/       # BindableProperty<T> and ViewModelBase
├── Binding/    # PropertyBinder<T>
└── View/       # IView<T> and ViewBase<T>
```

Samples are intentionally separated to keep the runtime clean.

---

## Why Not UniRx / Reactive Frameworks?

This project is designed for teams that:

- Prefer explicit, readable data flow
- Want predictable performance
- Avoid over-engineering for simple UI or gameplay states

It is especially suitable for:

- UI systems
- Game state presentation
- Tooling and in-game editors

Keeping the framework small (~150 lines total) means you can read and understand the entire binding system in minutes. No hidden magic.

---

## License

MIT License.

Use freely in commercial and non-commercial projects.
