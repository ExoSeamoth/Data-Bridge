using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataBridgeRework.Utils.Entries;

public class ObservableStack<T> : Stack<T>, INotifyPropertyChanged, INotifyCollectionChanged
{
    private T _currentItem;

    public T CurrentItem
    {
        get => _currentItem;
        private set
        {
            if (EqualityComparer<T>.Default.Equals(_currentItem, value)) return;
            _currentItem = value;
            OnPropertyChanged(EventArgsCache.CurrentPropertyChanged);
        }
    }

    public virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    public new virtual void Clear()
    {
        base.Clear();
        CurrentItem = default!;
        OnCountPropertyChanged();
        OnCurrentPropertyChanged();
        OnCollectionReset();
    }

    public new virtual T Pop()
    {
        var item = base.Pop();
        CurrentItem = Count > 0 ? Peek() : default!;
        OnCountPropertyChanged();
        OnCurrentPropertyChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        return item;
    }

    public new virtual void Push(T item)
    {
        base.Push(item);
        CurrentItem = item;
        OnCountPropertyChanged();
        OnCurrentPropertyChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void OnCountPropertyChanged()
    {
        OnPropertyChanged(EventArgsCache.CountPropertyChanged);
    }

    private void OnCurrentPropertyChanged()
    {
        OnPropertyChanged(EventArgsCache.CurrentPropertyChanged);
    }

    private void OnCollectionReset()
    {
        OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    internal static class EventArgsCache
    {
        internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
        internal static readonly PropertyChangedEventArgs CurrentPropertyChanged = new("CurrentItem");

        internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged =
            new(NotifyCollectionChangedAction.Reset);
    }
}