/// <summary>
/// Simple MVVM view interface.
/// </summary>
public interface IView<T> where T : ModelBase
{
    T BindingContext { get; set; }
    void OnBindingContextChange(T oldValue, T newValue);
}
