using AtomUI.DesignLanguages.PrimeNG.Presets;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.PrimeNG;

/// <summary>
/// Table roles for the current PrimeNG theme.
/// </summary>
public readonly ref struct PrimeNGTableRecipe
{
    private readonly PrimeNGToken _language;
    private readonly DesignToken _global;

    internal PrimeNGTableRecipe(PrimeNGToken language, DesignToken global)
    {
        _language = language;
        _global   = global;
    }

    /// <summary>
    /// Background of the table header: the <c>ColorFillAlter</c> tint flattened onto
    /// <see cref="RowBackground" />, so the header is opaque.
    /// </summary>
    public Color HeaderBackground => ColorUtils.OnBackground(_global.ColorFillAlter, RowBackground);
    /// <summary>
    /// Text color of the table header.
    /// </summary>
    public Color HeaderForeground => _global.ColorTextHeading;
    /// <summary>
    /// Background of the table rows. The DataGrid values also use it as the background of the whole table.
    /// </summary>
    public Color RowBackground => _global.ColorBgContainer;
    /// <summary>
    /// Background of a row under the pointer; the same fill as the header.
    /// </summary>
    public Color RowHoverBackground => HeaderBackground;
    /// <summary>
    /// Background of a selected row.
    /// </summary>
    public Color RowSelectedBackground => _global.ControlItemBgActive;
    /// <summary>
    /// Text color of a selected row.
    /// </summary>
    public Color RowSelectedForeground => _global.ControlItemColorActive;
    /// <summary>
    /// Background of a selected row under the pointer.
    /// </summary>
    public Color RowSelectedHoverBackground => _global.ControlItemBgActiveHover;

    /// <summary>
    /// The PrimeUIX striped row background for the active appearance.
    /// </summary>
    public Color GetStripedRowBackground(DesignVariant variant) =>
        variant.IsDark ? _language.Surface950 : _language.Surface50;

    /// <summary>
    /// Border color of the table.
    /// </summary>
    public Color BorderColor => _global.ColorBorderSecondary;
    /// <summary>
    /// Padding of a cell in the default size. The DataGrid values also use it as the padding around the whole table.
    /// </summary>
    public Thickness CellPadding => _global.Padding;
    /// <summary>
    /// Padding of a cell in the medium size: <c>PaddingXS</c> horizontally and <c>PaddingSM</c> vertically.
    /// </summary>
    public Thickness CellPaddingMedium
    {
        get
        {
            var horizontal = _global.PaddingXS;
            var vertical = _global.PaddingSM;
            return new Thickness(horizontal.Left, vertical.Top, horizontal.Right, vertical.Bottom);
        }
    }

    /// <summary>
    /// Padding of a cell in the small size.
    /// </summary>
    public Thickness CellPaddingSmall => _global.PaddingXS;
    /// <summary>
    /// Corner radius of the table panel.
    /// </summary>
    public CornerRadius CornerRadius => _global.BorderRadiusLG;
}

/// <summary>
/// PrimeUIX list.option focus and highlight roles used by lists and selectors.
/// </summary>
public readonly ref struct PrimeNGListItemRecipe
{
    private readonly DesignToken _global;

    internal PrimeNGListItemRecipe(DesignToken global)
    {
        _global = global;
    }

    /// <summary>
    /// Reports whether the selected background is the solid primary color: <c>true</c> for Nora and for every preset
    /// under a noir primary. Consumers then choose colors that stay visible on that solid highlight instead of the
    /// ones used on the pale highlight of Aura and Lara.
    /// </summary>
    public bool UsesSolidHighlight => SelectedBackground == _global.ColorPrimary;

    /// <summary>
    /// Background of an item under the pointer. PrimeNG paints a hovered item like the active one, so this is
    /// <see cref="FocusBackground" />.
    /// </summary>
    public Color HoverBackground => FocusBackground;
    /// <summary>
    /// Text color of an item under the pointer; the same color as <see cref="FocusForeground" />.
    /// </summary>
    public Color HoverForeground => FocusForeground;
    /// <summary>
    /// Background of the active item, for example the option reached by keyboard navigation. A Select with multiple
    /// values also uses it as the background of its selected value items.
    /// </summary>
    public Color FocusBackground => _global.ControlItemBgHover;
    /// <summary>
    /// Text color of the active item, and through <see cref="HoverForeground" /> of an item under the pointer.
    /// </summary>
    public Color FocusForeground => _global.ColorText;
    /// <summary>
    /// Background of a selected item in a list, a list view or a Select.
    /// </summary>
    public Color SelectedBackground => _global.ControlItemBgActive;
    /// <summary>
    /// Text color of a selected item in a list, a list view or a Select.
    /// </summary>
    public Color SelectedForeground => _global.ControlItemColorActive;
}

internal enum PrimeNGOverlayKind
{
    Menu,
    Select,
    Popover,
    Dialog
}

/// <summary>
/// Panel roles for PrimeUIX menu, select, popover or modal overlays.
/// </summary>
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

    /// <summary>
    /// Background of the overlay panel, whatever its kind.
    /// </summary>
    public Color Background => _global.ColorBgElevated;
    /// <summary>
    /// Border color of the panel, taken from the surface tones of the preset for the active appearance: a menu uses
    /// the menu border shade, every other kind the overlay border shade.
    /// </summary>
    public Color BorderColor
    {
        get
        {
            var rules = PrimeNGPresets.For(Preset, _variant.Appearance, _language);
            var shade = _kind == PrimeNGOverlayKind.Menu ? rules.MenuBorder : rules.OverlayBorder;
            return shade.Resolve(rules.Surface.AsSpan());
        }
    }

    /// <summary>
    /// Shadow of the panel: a popover takes the preset's popover shadow, a dialog the main box shadow, and a menu or
    /// select panel the secondary box shadow.
    /// </summary>
    public BoxShadows Shadows => _kind switch
    {
        PrimeNGOverlayKind.Popover => Preset.Controls.PopoverShadows,
        PrimeNGOverlayKind.Dialog => _global.BoxShadows,
        _ => _global.BoxShadowsSecondary
    };

    /// <summary>
    /// The panel shadow preceded by the visible portion of its outline.
    /// </summary>
    public BoxShadows ShadowsWithOutline => WithOutlineRing(Shadows, BorderColor, _global.LineWidth);

    /// <summary>
    /// Corner radius of the panel: a dialog adds the preset's extra-large radius offset to the top-left base radius,
    /// never below zero; every other kind takes the base radius.
    /// </summary>
    public CornerRadius CornerRadius => _kind == PrimeNGOverlayKind.Dialog
        ? new CornerRadius(Math.Max(0, _global.BorderRadius.TopLeft + Preset.Sizes.BorderRadiusXLOffset))
        : _global.BorderRadius;

    /// <summary>
    /// Padding inside the panel. A menu gets vertical padding only in Lara and Nora and 4 on every side in Aura; a
    /// popover and a dialog take their preset paddings; a select panel takes the preset's select popup content
    /// padding, which is resolved against the global tokens.
    /// </summary>
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

    /// <summary>
    /// Color of the mask behind the overlay: the mask token for a dialog, transparent for every other kind.
    /// </summary>
    public Color Mask => _kind == PrimeNGOverlayKind.Dialog ? _global.ColorBgMask : Colors.Transparent;

    /// <summary>
    /// Draws the PrimeNG panel border as an outline ring above its shadow. When the border is fully transparent or
    /// the width is not positive, the shadows are returned unchanged.
    /// </summary>
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

/// <summary>
/// PrimeUIX form.field and inputgroup.addon roles for the current preset and appearance.
/// </summary>
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

    /// <summary>
    /// Background of a form field in its resting state.
    /// </summary>
    public Color Background => _global.ColorBgContainer;
    /// <summary>
    /// Border color of a form field in its resting state.
    /// </summary>
    public Color BorderColor => _global.ColorBorder;
    /// <summary>
    /// Border color of a form field under the pointer. An unchecked checkbox or radio uses it as its hover border
    /// as well. Aura takes surface 400 in a light and surface 500 in a dark appearance, Lara the primary color,
    /// and Nora keeps the resting border color.
    /// </summary>
    public Color HoverBorderColor => Kind switch
    {
        PrimeNGPresetKind.Lara => _global.ColorPrimary,
        PrimeNGPresetKind.Nora => BorderColor,
        _ => _variant.IsDark ? _language.Surface500 : _language.Surface400
    };

    /// <summary>
    /// Border color of a form field that has focus.
    /// </summary>
    public Color FocusBorderColor => _global.InputActiveBorderColor;
    /// <summary>
    /// Ring around a focused form field. Nora uses a ring spread by one line width in the primary color instead of
    /// the input active shadow that every other preset takes.
    /// </summary>
    public BoxShadows FocusRing => Kind == PrimeNGPresetKind.Nora
        ? new BoxShadows(new BoxShadow { Spread = _global.LineWidth, Color = _global.ColorPrimary })
        : _global.InputActiveShadow;

    /// <summary>
    /// Text color of the placeholder of an empty form field.
    /// </summary>
    public Color PlaceholderForeground => _global.ColorTextPlaceholder;
    /// <summary>
    /// Background of a disabled form field.
    /// </summary>
    public Color DisabledBackground => _global.ColorBgContainerDisabled;
    /// <summary>
    /// Text color of a disabled form field. PrimeNG uses the secondary text color here rather than the disabled
    /// text color of the global token.
    /// </summary>
    public Color DisabledForeground => _global.ColorTextSecondary;
    /// <summary>
    /// Corner radius of a form field: the base radius.
    /// </summary>
    public CornerRadius CornerRadius => _global.BorderRadius;
    /// <summary>
    /// Color of the icons in a form field, shared by the resting and the hover state. Lara uses surface 400 in a
    /// dark and surface 500 in a light appearance, Nora white in a dark and surface 900 in a light appearance, and
    /// Aura surface 400 in both.
    /// </summary>
    public Color IconColor => Kind switch
    {
        PrimeNGPresetKind.Lara => _variant.IsDark ? _language.Surface400 : _language.Surface500,
        PrimeNGPresetKind.Nora => _variant.IsDark ? Colors.White : _language.Surface900,
        _ => _language.Surface400
    };

    /// <summary>
    /// Background of an add-on next to a form field. Lara uses surface 50 in a light and surface 800 in a dark
    /// appearance; Aura and Nora keep the field background. The input stage writes it into <c>ColorFillAlter</c>,
    /// the fill the add-on reads.
    /// </summary>
    public Color AddOnBackground => Kind == PrimeNGPresetKind.Lara
        ? (_variant.IsDark ? _language.Surface800 : _language.Surface50)
        : Background;
}
