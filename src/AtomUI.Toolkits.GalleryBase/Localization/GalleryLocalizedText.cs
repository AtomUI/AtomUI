using Avalonia;

namespace AtomUI.Toolkits.GalleryBase.Localization;

public interface IGalleryLocalizedText
{
    object Resolve();
}

public sealed class GalleryLocalizedText<TResourceKind> : IGalleryLocalizedText
    where TResourceKind : struct, Enum
{
    public TResourceKind ResourceKind { get; }

    public string Fallback { get; }

    public GalleryLocalizedText(TResourceKind resourceKind, string fallback)
    {
        ResourceKind = resourceKind;
        Fallback     = fallback;
    }

    public object Resolve()
    {
        if (Application.Current is { } application &&
            global::AtomUI.ApplicationExtensions.GetLocalizer(application) is { } localizer)
        {
            return localizer.Get(ResourceKind);
        }

        return Fallback;
    }

    public override string ToString()
    {
        return Resolve().ToString() ?? string.Empty;
    }
}
