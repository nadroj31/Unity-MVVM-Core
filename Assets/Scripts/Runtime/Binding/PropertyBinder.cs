using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Binds/unbinds ViewModel BindableProperty events to external handlers.
/// </summary>
public class PropertyBinder<T> where T : ModelBase
{
    private delegate void BindHandler(T viewmodel);
    private delegate void UnbindHandler(T viewmodel);

    private readonly List<BindHandler> _binders = new();
    private readonly List<UnbindHandler> _unBinders = new();

    /// <summary>
    /// Register a binding for a public field (BindableProperty) on the ViewModel.
    /// </summary>
    public void Add<TProperty>(string name, BindableProperty<TProperty>.ValueChangedHandler valueChangedHandler)
    {
        var fieldInfo = typeof(T).GetField(name, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new Exception(string.Format("Cannot find bindable property field '{0},{1}'", typeof(T).Name, name));

        _binders.Add(viewmodel =>
        {
            var property = GetPropertyValue<TProperty>(name, viewmodel, fieldInfo);
            property.ValueChanged += valueChangedHandler;
        });

        _unBinders.Add(viewmodel =>
        {
            var property = GetPropertyValue<TProperty>(name, viewmodel, fieldInfo);
            property.ValueChanged -= valueChangedHandler;
        });
    }

    // Retrieve the BindableProperty<TProperty> instance from the viewmodel field.
    private BindableProperty<TProperty> GetPropertyValue<TProperty>(string name, T viewmodel, FieldInfo fieldInfo)
    {
        var value = fieldInfo.GetValue(viewmodel);
        if (value is BindableProperty<TProperty> property) return property;
        throw new Exception(string.Format("Invalid bindable property field type '{0},{1}'", typeof(T).Name, name));
    }

    // Apply all registered bindings.
    public void Bind(T viewmodel)
    {
        if (viewmodel == null) return;
        foreach (var binder in _binders) binder(viewmodel);
    }

    // Remove all registered bindings.
    public void Unbind(T viewmodel)
    {
        if (viewmodel == null) return;
        foreach (var unBinder in _unBinders) unBinder(viewmodel);
    }
}
