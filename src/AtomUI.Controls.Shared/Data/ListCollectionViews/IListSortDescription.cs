using System.ComponentModel;

namespace AtomUI.Controls.Data;

public interface IListSortDescription
{
    ListSortDirection Direction { get; }
    IComparer<object> Comparer { get; }
    bool HasPropertyPath { get; }
    IOrderedEnumerable<object> OrderBy(IEnumerable<object> seq);
    IOrderedEnumerable<object> ThenBy(IOrderedEnumerable<object> seq);
    ListSortDescription SwitchSortDirection();
}