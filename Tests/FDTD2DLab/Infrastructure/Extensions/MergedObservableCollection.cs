using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

public class MergedObservableCollection<TBase> : ObservableCollection<TBase>, IDisposable
{
    private readonly List<INotifyCollectionChanged> _sourceCollections = new();
    private readonly Func<IEnumerable<TBase>> _getAllItems;

    // Приватный конструктор, принимающий способ получения всех элементов и список исходных коллекций
    private MergedObservableCollection(Func<IEnumerable<TBase>> getAllItems, IEnumerable<INotifyCollectionChanged> sourceCollections)
    {
        _getAllItems = getAllItems;
        foreach (var collection in sourceCollections)
        {
            _sourceCollections.Add(collection);
            collection.CollectionChanged += OnSourceCollectionChanged;
        }
        Rebuild();
    }

    // Фабричный метод для коллекций с элементами типа TBase
    public static MergedObservableCollection<TBase> FromCollections(params ObservableCollection<TBase>[] collections)
    {
        var list = collections.ToList();
        Func<IEnumerable<TBase>> getAll = () => list.SelectMany(c => c);
        return new MergedObservableCollection<TBase>(getAll, list);
    }

    // Фабричный метод для коллекций с элементами-наследниками TBase
    public static MergedObservableCollection<TBase> FromCollections<TChild>(params ObservableCollection<TChild>[] collections)
        where TChild : TBase
    {
        var list = collections.ToList();
        Func<IEnumerable<TBase>> getAll = () => list.SelectMany(c => c).Cast<TBase>();
        return new MergedObservableCollection<TBase>(getAll, list);
    }

    private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Rebuild();
    }

    private void Rebuild()
    {
        var allItems = _getAllItems().ToList();
        ClearItems();
        foreach (var item in allItems)
            Add(item);
    }

    public void Dispose()
    {
        foreach (var collection in _sourceCollections)
            collection.CollectionChanged -= OnSourceCollectionChanged;
    }
}