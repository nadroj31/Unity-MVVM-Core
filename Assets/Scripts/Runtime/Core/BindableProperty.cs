using System;

/// <summary>
/// Generic bindable property with thread-safe access and change notification.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public class BindableProperty<T>
{
    // Delegate invoked when value changes.
    public delegate void ValueChangedHandler(T oldValue, T newValue);

    // Raised when Value changes.
    public event ValueChangedHandler ValueChanged;

    private T _value;
    private readonly object _lock = new();

    // Constructor: optional initial value.
    public BindableProperty(T initialValue = default)
    {
        _value = initialValue;
    }

    // Thread-safe value property. Invokes ValueChanged outside the lock.
    public T Value
    {
        get
        {
            lock (_lock) { return _value; }
        }
        set
        {
            T oldValue;
            bool changed = false;
            lock (_lock)
            {
                if (!Equals(_value, value))
                {
                    oldValue = _value;
                    _value = value;
                    changed = true;
                }
                else
                {
                    return;
                }
            }

            if (changed)
            {
                ValueChanged?.Invoke(oldValue, value);
            }
        }
    }

    public override string ToString()
    {
        lock (_lock) { return _value != null ? _value.ToString() : "null"; }
    }
}
