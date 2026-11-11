using System.Globalization;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace AtomUI.Controls;

public class TextGlyph : WatermarkGlyph
{
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty
        .Register<TextGlyph, string?>(nameof(Text));

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty
        .Register<TextGlyph, double>(nameof(FontSize), 16);

    public IBrush Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public static readonly StyledProperty<IBrush> ForegroundProperty = AvaloniaProperty
        .Register<TextGlyph, IBrush>(nameof(Foreground), Brushes.Black);

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly StyledProperty<FontFamily> FontFamilyProperty = AvaloniaProperty
        .Register<TextGlyph, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly StyledProperty<FontWeight> FontWeightProperty = AvaloniaProperty
        .Register<TextGlyph, FontWeight>(nameof(FontWeight), FontWeight.Normal);

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public static readonly StyledProperty<FontStyle> FontStyleProperty = AvaloniaProperty
        .Register<TextGlyph, FontStyle>(nameof(FontStyle), FontStyle.Normal);

    protected FormattedText? FormattedText { get; private set; }

    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    private IDisposable? _themeSubscription;
    private IDisposable? _themeForeground;
    // Writing Foreground raises property callbacks that may move this shared glyph to another owner re-entrantly.
    // The versions let an outer call see that a newer one took over, so it neither overwrites the newer handles
    // nor leaks its own.
    private int _ownerVersion;
    private int _themeForegroundVersion;

    static TextGlyph()
    {
        TextProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontFamilyProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontWeightProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontStyleProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.RebuildFormatText());
        FontSizeProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
        ForegroundProperty.Changed.AddClassHandler<TextGlyph>((glyph, args) => glyph.UpdateFormatText());
    }

    protected virtual void RebuildFormatText()
    {
        if (Text == null)
        {
            FormattedText = null;
            return;
        }

        FormattedText = new FormattedText(
            Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight),
            FontSize,
            Foreground)
        {
            TextAlignment = TextAlignment.Center
        };
    }

    // The default foreground is the owner's ColorText, applied below any explicit value so that setting or
    // clearing Foreground behaves as usual. Theme subscriptions neither fire on subscribe nor survive a
    // detach, so the value is resolved again every time an owner attaches.
    private protected override void OnResourceOwnerChanged(IResourceHost? owner)
    {
        var ownerVersion = ++_ownerVersion;
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        if (owner is not StyledElement styledOwner)
        {
            return;
        }

        ApplyThemeForeground(ThemeScope.ResolveContext(styledOwner)?.Snapshot);
        if (ownerVersion != _ownerVersion)
        {
            // An owner change during the foreground write already subscribed for the newer owner.
            return;
        }

        _themeSubscription = s_themeTokenResolver.Subscribe(styledOwner, ApplyThemeForeground);
    }

    private void ApplyThemeForeground(ThemeSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return;
        }

        var color = s_themeTokenResolver.GetGlobal<Color>(snapshot, (int)SharedTokenKind.ColorText);
        var version = ++_themeForegroundVersion;
        var previous = _themeForeground;
        var handle = SetValue(ForegroundProperty, new ImmutableSolidColorBrush(color), BindingPriority.Template);
        if (version != _themeForegroundVersion)
        {
            // A newer write replaced the previous entry during the callbacks; this one is already stale.
            handle?.Dispose();
            return;
        }

        _themeForeground = handle;
        previous?.Dispose();
    }

    private void UpdateFormatText()
    {
        if (FormattedText == null)
        {
            return;
        }

        FormattedText.SetFontSize(FontSize);
        FormattedText.SetForegroundBrush(Foreground);
    }

    public override void Render(DrawingContext context)
    {
        if (FormattedText == null)
        {
            return;
        }

        context.DrawText(FormattedText, new Point());
    }

    public override Size GetDesiredSize()
    {
        return FormattedText == null ? new Size() : new Size(FormattedText.Width, FormattedText.Height);
    }

    // FormattedText 的布局尺寸不包含字形越界。此方法只用于确切的内置 TextGlyph，
    // 自定义子类的 Render 不受该边界约束。
    internal Rect GetDrawingBounds()
    {
        var bounds = new Rect(GetDesiredSize());
        if (FormattedText is not { } text || !double.IsFinite(text.Extent) || text.Extent <= 0)
        {
            return bounds;
        }

        var inkBounds = new Rect(text.OverhangLeading,
            text.Height + text.OverhangAfter - text.Extent,
            Math.Max(0, text.Width - text.OverhangTrailing - text.OverhangLeading), text.Extent);
        return bounds.Union(inkBounds);
    }
}
