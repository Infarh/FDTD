using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace FDTD2DLab.Infrastructure
{
    public static class ObservableCollectionExtensions
    {
        public static void ForceReset<T>(this ObservableCollection<T> collection)
        {
            // Вызов защищённого метода OnCollectionChanged с действием Reset
            collection.GetType()
                .GetMethod("OnCollectionChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(collection, new object[] { new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset) });
            // Также можно уведомить об изменении свойства Count и Items[]
            collection.GetType()
                .GetMethod("OnPropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(collection, new object[] { new PropertyChangedEventArgs("Count") });
            collection.GetType()
                .GetMethod("OnPropertyChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.Invoke(collection, new object[] { new PropertyChangedEventArgs("Item[]") });
        }
    }
}