using Avalonia.Styling;

namespace AtomUI.Theme.Styling;

internal sealed class ThemeStyleHostStamp
{
    private readonly IStyleHost[] _ancestors;
    private readonly IStyle[] _styles;

    internal ThemeStyleHostStamp(IStyleHost host)
    {
        var ancestors = new List<IStyleHost>();
        for (var parent = host.StylingParent; parent is not null; parent = parent.StylingParent)
        {
            ancestors.Add(parent);
        }
        _ancestors = ancestors.ToArray();
        _styles = host.Styles.ToArray();
    }

    private ThemeStyleHostStamp(IStyleHost[] ancestors, IStyle[] styles)
    {
        _ancestors = ancestors;
        _styles = styles;
    }

    internal ThemeStyleHostStamp WithCandidate(Styles previous, Styles candidate)
    {
        var index = Array.IndexOf(_styles, previous);
        if (index < 0)
        {
            throw new InvalidOperationException("The committed style group is no longer mounted.");
        }
        var styles = _styles.ToList();
        styles.Insert(index + 1, candidate);
        return new ThemeStyleHostStamp(_ancestors, styles.ToArray());
    }

    internal bool Matches(IStyleHost host)
    {
        var index = 0;
        for (var parent = host.StylingParent; parent is not null; parent = parent.StylingParent)
        {
            if (index == _ancestors.Length || !ReferenceEquals(parent, _ancestors[index++]))
            {
                return false;
            }
        }
        if (index != _ancestors.Length || host.Styles.Count != _styles.Length)
        {
            return false;
        }
        for (index = 0; index < _styles.Length; index++)
        {
            if (!ReferenceEquals(host.Styles[index], _styles[index]))
            {
                return false;
            }
        }
        return true;
    }
}
