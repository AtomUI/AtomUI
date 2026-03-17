using AtomUI.Controls.Utils;

namespace AtomUI.Controls.Data;

public interface IListFilterDescription
{
    DefaultFilterValueSelector? FilterPropertySelector { get; }
    List<object> FilterConditions { get; }
    bool FilterBy(object record);
    Func<object, object, bool>? Filter { get; }
}