using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Controls;

/// <summary>
/// Produces the <see cref="PathIcon" /> that a control shows for an icon property typed as this interface.
/// </summary>
public interface IIconTemplate : ITemplate<PathIcon?>
{}

/// <summary>
/// An <see cref="IIconTemplate" /> that asks a delegate supplied by the application for the icon.
/// </summary>
public class IconFuncTemplate : IIconTemplate
{
    private readonly Func<PathIcon?> _func;
    
    /// <summary>
    /// Stores the delegate that <see cref="Build" /> calls.
    /// </summary>
    /// <param name="func">
    /// The delegate that creates the icon.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="func" /> is <see langword="null" />.
    /// </exception>
    public IconFuncTemplate(Func<PathIcon?> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
    }
    
    /// <summary>
    /// Returns the icon produced by the delegate, or <see langword="null" /> when the delegate produces none.
    /// </summary>
    public PathIcon? Build()
    {
        return _func();
    }

    object? ITemplate.Build() => Build();
}
