using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MoneyMaker
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void Set<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            field = value;

            FirePropertyChanged(propertyName);
        }

        protected void FirePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            OnPropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

    }
}
