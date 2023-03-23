using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace FDTD2DLab.Infrastructure.Extensions;

public static class NotifyCollectionChangedEx
{
    public static CollectionItemsChangeTracker<T> OnItems<T>(this T Collection)
        where T : INotifyCollectionChanged => Collection is null ? null : new(Collection);
}

public class CollectionItemsChangeTracker<T> : IDisposable
    where T : INotifyCollectionChanged
{
    private readonly T _Collection;

    public CollectionItemsChangeTracker(T Collection)
    {
        _Collection = Collection;
        Collection.CollectionChanged += OnItemsChanged;
    }

    private void OnItemsChanged(object Sender, NotifyCollectionChangedEventArgs E)
    {
        switch (E.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (E.NewItems is { } new_items)
                    foreach (var item in new_items.OfType<INotifyPropertyChanged>())
                        item.PropertyChanged += OnItemPropertyChanged;
                break;
            case NotifyCollectionChangedAction.Remove:
                if (E.OldItems is { } old_items)
                    foreach (var item in old_items.OfType<INotifyPropertyChanged>())
                        item.PropertyChanged -= OnItemPropertyChanged;
                break;
        }
    }

    private void OnItemPropertyChanged(object Sender, PropertyChangedEventArgs E)
    {
        if(_Handlers.TryGetValue(E.PropertyName!, out var handler))
            handler?.Invoke(Sender);
    }

    private readonly Dictionary<string, Action<object>> _Handlers = new();
    public IDisposable Changed(string PropertyName, Action<object> Handler)
    {
        if (_Handlers.ContainsKey(PropertyName))
            _Handlers[PropertyName] += Handler;
        else
            _Handlers[PropertyName] = Handler;

        return LambdaDisposable.OnDisposed(() => _Handlers[PropertyName] -= Handler);
    }

    public void Dispose()
    {
        _Collection.CollectionChanged -= OnItemsChanged;
        if (_Collection is not IEnumerable items) return;
        foreach (var item in items.OfType<INotifyPropertyChanged>())
            item.PropertyChanged -= OnItemPropertyChanged;
    }
}
