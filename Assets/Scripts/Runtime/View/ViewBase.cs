using UnityEngine;

/// <summary>
/// Base View class for MVVM; manages ViewModel binding lifecycle.
/// </summary>
public abstract class ViewBase<T> : MonoBehaviour, IView<T> where T : ViewModelBase
{
    private bool _isInitialized = false;
    protected readonly PropertyBinder<T> Binder = new();
    protected readonly BindableProperty<T> ViewmodelProperty = new();

    // The bound ViewModel.
    public T BindingContext
    {
        get => ViewmodelProperty.Value;
        set
        {
            if (!_isInitialized)
            {
                OnInitialize();
                _isInitialized = true;
            }
            ViewmodelProperty.Value = value;
        }
    }

    // Hook for subclasses to initialize bindings.
    protected virtual void OnInitialize()
    {
        ViewmodelProperty.ValueChanged += OnBindingContextChange;
    }

    // Default binding context change handling: unbind old, bind new.
    protected virtual void OnBindingContextChange(T oldValue, T newValue)
    {
        if (oldValue != null) Binder.Unbind(oldValue);
        if (newValue != null) Binder.Bind(newValue);
    }

    // Cleanup on destroy: unbind first, then unsubscribe.
    protected virtual void OnDestroy()
    {
        BindingContext = null;
        ViewmodelProperty.ValueChanged -= OnBindingContextChange;
    }
}
