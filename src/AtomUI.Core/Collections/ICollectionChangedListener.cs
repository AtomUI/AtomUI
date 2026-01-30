// 摘取自 Avalonia 相关代码

using System.Collections.Specialized;

namespace AtomUI.Collections;

internal interface ICollectionChangedListener
{
    void PreChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e);
    void Changed(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e);
    void PostChanged(INotifyCollectionChanged sender, NotifyCollectionChangedEventArgs e);
}