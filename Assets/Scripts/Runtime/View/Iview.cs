/// <summary>
/// Simple MVVM view interface.
/// </summary>
public interface IView<T> where T : ViewModelBase
{
    T BindingContext { get; set; }
}
