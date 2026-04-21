using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DevDock.ViewModels
{
    // Shared base class for view models that participate in WPF data binding.
    // It centralizes INotifyPropertyChanged support so each view model does not
    // need to repeat the same notification boilerplate.
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Raises a property change notification for the calling property by default.
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Standard helper used by property setters:
        // - avoids unnecessary UI refreshes when the value is unchanged
        // - updates the backing field
        // - raises PropertyChanged for the bound property
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
