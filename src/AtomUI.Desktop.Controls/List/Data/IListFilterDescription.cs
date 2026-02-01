namespace AtomUI.Desktop.Controls.Data;

public interface IListFilterDescription
{
    ListFilterPropertySelector? FilterPropertySelector { get; }
    List<object> FilterConditions { get; }
    bool FilterBy(object record);
    Func<object, object, bool>? Filter { get; }
}