using Avalonia.Controls;
using Avalonia.Styling;

namespace AtomUI.Controls;

public interface IIconTemplate : ITemplate<PathIcon?>
{}

public class IconFuncTemplate : IIconTemplate
{
    private readonly Func<PathIcon?> _func;
    
    public IconFuncTemplate(Func<PathIcon?> func)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
    }
    
    public PathIcon? Build()
    {
        return _func();
    }

    object? ITemplate.Build() => Build();
}
