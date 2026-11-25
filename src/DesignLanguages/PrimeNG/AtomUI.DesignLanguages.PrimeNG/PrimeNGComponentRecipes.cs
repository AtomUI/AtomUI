using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>Table roles for the current PrimeNG theme.</summary>
public readonly ref struct PrimeNGTableRecipe
{
    private readonly PrimeNGToken _language;
    private readonly DesignToken _global;

    internal PrimeNGTableRecipe(PrimeNGToken language, DesignToken global)
    {
        _language = language;
        _global   = global;
    }

    public Color HeaderBackground => ColorUtils.OnBackground(_global.ColorFillAlter, RowBackground);
    public Color HeaderForeground => _global.ColorTextHeading;
    public Color RowBackground => _global.ColorBgContainer;
    public Color RowHoverBackground => HeaderBackground;
    public Color RowSelectedBackground => _global.ControlItemBgActive;
    public Color RowSelectedForeground => _global.ControlItemColorActive;
    public Color RowSelectedHoverBackground => _global.ControlItemBgActiveHover;

    /// <summary>The PrimeUIX striped row background for the active appearance.</summary>
    public Color GetStripedRowBackground(DesignVariant variant) =>
        variant.IsDark ? _language.Surface950 : _language.Surface50;

    public Color BorderColor => _global.ColorBorderSecondary;
    public Thickness CellPadding => _global.Padding;
    public Thickness CellPaddingMedium
    {
        get
        {
            var horizontal = _global.PaddingXS;
            var vertical = _global.PaddingSM;
            return new Thickness(horizontal.Left, vertical.Top, horizontal.Right, vertical.Bottom);
        }
    }

    public Thickness CellPaddingSmall => _global.PaddingXS;
    public CornerRadius CornerRadius => _global.BorderRadiusLG;
}

/// <summary>PrimeUIX list.option focus and highlight roles used by lists and selectors.</summary>
public readonly ref struct PrimeNGListItemRecipe
{
    private readonly DesignToken _global;

    internal PrimeNGListItemRecipe(DesignToken global)
    {
        _global = global;
    }

    public bool UsesSolidHighlight => SelectedBackground == _global.ColorPrimary;

    public Color HoverBackground => FocusBackground;
    public Color HoverForeground => FocusForeground;
    public Color FocusBackground => _global.ControlItemBgHover;
    public Color FocusForeground => _global.ColorText;
    public Color SelectedBackground => _global.ControlItemBgActive;
    public Color SelectedForeground => _global.ControlItemColorActive;
}

internal enum PrimeNGOverlayKind
{
    Menu,
    Select,
    Popover,
    Dialog
}

/// <summary>Panel roles for PrimeUIX menu, select, popover or modal overlays.</summary>
public readonly ref struct PrimeNGOverlayRecipe
{
    private readonly PrimeNGToken _language;
    private readonly DesignToken _global;
    private readonly DesignVariant _variant;
    private readonly PrimeNGOverlayKind _kind;

    internal PrimeNGOverlayRecipe(PrimeNGToken language, DesignToken global, DesignVariant variant, PrimeNGOverlayKind kind)
    {
        _language = language;
        _global = global;
        _variant = variant;
        _kind = kind;
    }

    private PrimeNGPreset Preset => PrimeNGPresets.Of(_variant.Style);

    public Color Background => _global.ColorBgElevated;
    public Color BorderColor
    {
        get
        {
            var rules = PrimeNGPresets.For(Preset, _variant.Appearance, _language);
            var shade = _kind == PrimeNGOverlayKind.Menu ? rules.MenuBorder : rules.OverlayBorder;
            return shade.Resolve(rules.Surface.AsSpan());
        }
    }

    public BoxShadows Shadows => _kind switch
    {
        PrimeNGOverlayKind.Popover => Preset.Controls.PopoverShadows,
        PrimeNGOverlayKind.Dialog => _global.BoxShadows,
        _ => _global.BoxShadowsSecondary
    };

    /// <summary>The panel shadow preceded by the visible portion of its outline.</summary>
    public BoxShadows ShadowsWithOutline => WithOutlineRing(Shadows, BorderColor, _global.LineWidth);

    public CornerRadius CornerRadius => _kind == PrimeNGOverlayKind.Dialog
        ? new CornerRadius(Math.Max(0, _global.BorderRadius.TopLeft + Preset.Sizes.BorderRadiusXLOffset))
        : _global.BorderRadius;

    public Thickness Padding => _kind switch
    {
        PrimeNGOverlayKind.Menu => PrimeNGPresets.KindOf(_variant.Style) switch
        {
            PrimeNGPresetKind.Lara => new Thickness(0, 8),
            PrimeNGPresetKind.Nora => new Thickness(0, 2),
            _ => new Thickness(4)
        },
        PrimeNGOverlayKind.Popover => new Thickness(Preset.Controls.PopoverPadding),
        PrimeNGOverlayKind.Dialog => new Thickness(Preset.Controls.DialogPadding),
        _ => Preset.Controls.SelectPopupContentPadding(_global)
    };

    public Color Mask => _kind == PrimeNGOverlayKind.Dialog ? _global.ColorBgMask : Colors.Transparent;

    /// <summary>Draws the PrimeNG panel border as an outline ring above its shadow.</summary>
    public static BoxShadows WithOutlineRing(BoxShadows shadows, Color border, double width)
    {
        if (border.A == 0 || width <= 0)
        {
            return shadows;
        }

        var ring = new BoxShadow { Spread = width, Color = border };
        if (shadows.Count == 0)
        {
            return new BoxShadows(ring);
        }

        var rest = new BoxShadow[shadows.Count];
        for (var i = 0; i < shadows.Count; i++)
        {
            rest[i] = shadows[i];
        }

        return new BoxShadows(ring, rest);
    }
}

/// <summary>PrimeUIX form.field and inputgroup.addon roles for the current preset and appearance.</summary>
public readonly ref struct PrimeNGFieldRecipe
{
    private readonly PrimeNGToken _language;
    private readonly DesignToken _global;
    private readonly DesignVariant _variant;

    internal PrimeNGFieldRecipe(PrimeNGToken language, DesignToken global, DesignVariant variant)
    {
        _language = language;
        _global = global;
        _variant = variant;
    }

    private PrimeNGPresetKind Kind => PrimeNGPresets.KindOf(_variant.Style);

    public Color Background => _global.ColorBgContainer;
    public Color BorderColor => _global.ColorBorder;
    public Color HoverBorderColor => Kind switch
    {
        PrimeNGPresetKind.Lara => _global.ColorPrimary,
        PrimeNGPresetKind.Nora => BorderColor,
        _ => _variant.IsDark ? _language.Surface500 : _language.Surface400
    };

    public Color FocusBorderColor => _global.InputActiveBorderColor;
    public BoxShadows FocusRing => Kind == PrimeNGPresetKind.Nora
        ? new BoxShadows(new BoxShadow { Spread = _global.LineWidth, Color = _global.ColorPrimary })
        : _global.InputActiveShadow;

    public Color PlaceholderForeground => _global.ColorTextPlaceholder;
    public Color DisabledBackground => _global.ColorBgContainerDisabled;
    public Color DisabledForeground => _global.ColorTextSecondary;
    public CornerRadius CornerRadius => _global.BorderRadius;
    public Color IconColor => Kind switch
    {
        PrimeNGPresetKind.Lara => _variant.IsDark ? _language.Surface400 : _language.Surface500,
        PrimeNGPresetKind.Nora => _variant.IsDark ? Colors.White : _language.Surface900,
        _ => _language.Surface400
    };

    public Color AddOnBackground => Kind == PrimeNGPresetKind.Lara
        ? (_variant.IsDark ? _language.Surface800 : _language.Surface50)
        : Background;
}
