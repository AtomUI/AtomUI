using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>
/// Table roles used by the shadcn DataGrid value package.
/// </summary>
public readonly ref struct ShadcnTableRecipe
{
    private readonly DesignToken _global;

    internal ShadcnTableRecipe(DesignToken global)
    {
        _global = global;
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
    /// Border color of the table.
    /// </summary>
    public Color BorderColor => _global.ColorBorderSecondary;
    /// <summary>
    /// Padding of a cell in the default size. The DataGrid values also use it as the padding around the whole
    /// table.
    /// </summary>
    public Thickness CellPadding => _global.Padding;
    /// <summary>
    /// Padding of a cell in the medium size: the extra-small padding token horizontally and the small one
    /// vertically.
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
    /// Corner radius of the table frame; the large radius token.
    /// </summary>
    public CornerRadius CornerRadius => _global.BorderRadiusLG;
}

/// <summary>
/// The five shadcn chart series colors in registry order.
/// </summary>
public readonly ref struct ShadcnChartRecipe
{
    private readonly ShadcnToken _language;

    internal ShadcnChartRecipe(ShadcnToken language)
    {
        _language = language;
    }

    /// <summary>
    /// Color of the first chart series.
    /// </summary>
    public Color Series1 => _language.Chart1;
    /// <summary>
    /// Color of the second chart series.
    /// </summary>
    public Color Series2 => _language.Chart2;
    /// <summary>
    /// Color of the third chart series.
    /// </summary>
    public Color Series3 => _language.Chart3;
    /// <summary>
    /// Color of the fourth chart series.
    /// </summary>
    public Color Series4 => _language.Chart4;
    /// <summary>
    /// Color of the fifth chart series.
    /// </summary>
    public Color Series5 => _language.Chart5;
}

/// <summary>
/// Interactive list item roles for shadcn list and select values.
/// </summary>
public readonly ref struct ShadcnListItemRecipe
{
    private readonly DesignToken _global;

    internal ShadcnListItemRecipe(DesignToken global)
    {
        _global = global;
    }

    /// <summary>
    /// Background of a list item under the pointer.
    /// </summary>
    public Color HoverBackground => _global.ColorBgTextHover;
    /// <summary>
    /// Text color of a list item under the pointer.
    /// </summary>
    public Color HoverForeground => _global.ColorTextSecondary;
    /// <summary>
    /// Background of the active option, for example the one reached by keyboard navigation.
    /// </summary>
    public Color FocusBackground => _global.ControlItemBgHover;
    /// <summary>
    /// Text color of the active option; the same color as a selected option.
    /// </summary>
    public Color FocusForeground => SelectedForeground;
    /// <summary>
    /// Background of a selected option or list item.
    /// </summary>
    public Color SelectedBackground => _global.ControlItemBgActive;
    /// <summary>
    /// Text color of a selected option or list item.
    /// </summary>
    public Color SelectedForeground => _global.ControlItemColorActive;
}

internal enum ShadcnOverlayKind
{
    Menu,
    Popover,
    Dialog
}

/// <summary>
/// Panel roles from the shadcn menu, popover or dialog style table.
/// </summary>
public readonly ref struct ShadcnOverlayRecipe
{
    private readonly ShadcnToken _language;
    private readonly DesignToken _global;
    private readonly ShadcnOverlayKind _kind;

    internal ShadcnOverlayRecipe(ShadcnToken language, DesignToken global, ShadcnOverlayKind kind)
    {
        _language = language;
        _global = global;
        _kind = kind;
    }

    /// <summary>
    /// Background of the panel.
    /// </summary>
    public Color Background => _global.ColorBgElevated;
    /// <summary>
    /// Outline color of the panel: the ring color that the language gives its floating surfaces
    /// (<see cref="ShadcnToken.OutlineRing" />).
    /// </summary>
    public Color BorderColor => _language.OutlineRing;
    /// <summary>
    /// Drop shadow of the panel: the shadow token of its kind, that is the dialog, popover or menu shadow.
    /// </summary>
    public BoxShadows Shadows => _kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogShadow,
        ShadcnOverlayKind.Popover => _language.PopoverShadow,
        _ => _language.MenuShadow
    };

    /// <summary>
    /// The outline as a one-pixel ring in front of the panel shadow.
    /// </summary>
    public BoxShadows ShadowsWithOutline => ShadcnRecipes.RingThenShadow(BorderColor, Shadows);

    /// <summary>
    /// Corner radius of the panel: the radius token of its kind, that is the dialog, popover or menu radius.
    /// </summary>
    public CornerRadius CornerRadius => new(_kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogRadius,
        ShadcnOverlayKind.Popover => _language.PopoverRadius,
        _ => _language.MenuRadius
    });

    /// <summary>
    /// Padding inside the panel, between its edge and its content: the padding token of its kind, that is the
    /// dialog, popover or menu padding.
    /// </summary>
    public Thickness Padding => new(_kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogPadding,
        ShadcnOverlayKind.Popover => _language.PopoverPadding,
        _ => _language.MenuPadding
    });

    /// <summary>
    /// Color of the mask that covers the page behind the panel: the mask token for a dialog, and transparent
    /// for a menu or a popover so that they do not dim the page.
    /// </summary>
    public Color Mask => _kind == ShadcnOverlayKind.Dialog ? _global.ColorBgMask : Colors.Transparent;
}

/// <summary>
/// Form-field roles from the shadcn input style and the effective global tokens.
/// </summary>
public readonly ref struct ShadcnFieldRecipe
{
    private readonly ShadcnToken _language;
    private readonly DesignToken _global;
    private readonly DesignVariant _variant;

    internal ShadcnFieldRecipe(ShadcnToken language, DesignToken global, DesignVariant variant)
    {
        _language = language;
        _global = global;
        _variant = variant;
    }

    /// <summary>
    /// Background of a field that shows the recipe's own <see cref="BorderColor" />:
    /// <see cref="GetBackground" /> without a control-level border override.
    /// </summary>
    public Color Background => GetBackground(BorderColor);
    /// <summary>
    /// Border color of a field at rest: the language's <c>--input</c> variable
    /// (<see cref="ShadcnToken.Input" />).
    /// </summary>
    public Color BorderColor => _language.Input;
    /// <summary>
    /// Border color of a hovered field. The <see cref="ShadcnStyles.Sera" /> style draws a bottom rule only
    /// and keeps <see cref="BorderColor" />; every other style uses <see cref="FocusBorderColor" />, the ring
    /// color.
    /// </summary>
    public Color HoverBorderColor => BottomBorderOnly ? BorderColor : FocusBorderColor;
    /// <summary>
    /// Border color of a focused field: the language's <c>--ring</c> variable (<see cref="ShadcnToken.Ring" />),
    /// not the theme primary.
    /// </summary>
    public Color FocusBorderColor => _language.Ring;
    /// <summary>
    /// Shadow that marks a focused field: <c>InputActiveShadow</c>, or <see langword="default" /> (no shadow)
    /// for the <see cref="ShadcnStyles.Sera" /> style, which marks focus with the border color alone.
    /// </summary>
    public BoxShadows FocusRing => BottomBorderOnly ? default : _global.InputActiveShadow;
    /// <summary>
    /// Text color of the placeholder of an empty field.
    /// </summary>
    public Color PlaceholderForeground => _global.ColorTextPlaceholder;
    /// <summary>
    /// Background of a disabled field.
    /// </summary>
    public Color DisabledBackground => _global.ColorBgContainerDisabled;
    /// <summary>
    /// Text color of a disabled field.
    /// </summary>
    public Color DisabledForeground => _global.ColorTextDisabled;
    /// <summary>
    /// Corner radius of a field at every size: the language's form-field radius
    /// (<see cref="ShadcnToken.InputRadius" />). The input family applies it to the small, medium and large
    /// radius tokens alike, because the registry input has no size variants.
    /// </summary>
    public CornerRadius CornerRadius => new(_language.InputRadius);
    /// <summary>
    /// Whether the field frame is a bottom rule only: <see langword="true" /> for the
    /// <see cref="ShadcnStyles.Sera" /> style. Such a field keeps its resting border on hover and draws no
    /// focus ring.
    /// </summary>
    public bool BottomBorderOnly => _variant.Style == ShadcnStyles.Sera;
    /// <summary>
    /// Whether the field frame is a translucent fill instead of a solid border: <see langword="true" /> for
    /// the <see cref="ShadcnStyles.Luma" /> and <see cref="ShadcnStyles.Rhea" /> styles.
    /// <see cref="GetBackground" /> then fills the field with half the alpha of the border color.
    /// </summary>
    public bool TransparentBorder => _variant.Style == ShadcnStyles.Luma || _variant.Style == ShadcnStyles.Rhea;

    /// <summary>
    /// The field fill of the styles that draw a border, that is when <see cref="TransparentBorder" /> is
    /// <see langword="false" />: in a dark variant with a positive <see cref="ShadcnToken.DarkInputFill" />,
    /// <see cref="BorderColor" /> with its alpha scaled to that share; otherwise transparent, so the field
    /// shows the surface behind it.
    /// </summary>
    public Color OutlinedBackground => _variant.IsDark && _language.DarkInputFill > 0
        ? ScaleAlpha(BorderColor, _language.DarkInputFill)
        : Colors.Transparent;

    /// <summary>
    /// Resolves the background using the field's effective border, including a control-level override.
    /// The <see cref="ShadcnStyles.Luma" /> and <see cref="ShadcnStyles.Rhea" /> styles take half the alpha of
    /// that border; every other style returns <see cref="OutlinedBackground" /> and ignores the border.
    /// </summary>
    public Color GetBackground(Color effectiveBorderColor) =>
        TransparentBorder ? ScaleAlpha(effectiveBorderColor, 0.5) : OutlinedBackground;

    private static Color ScaleAlpha(Color color, double factor) =>
        ShadcnRecipes.WithAlpha(color, (byte)Math.Round(color.A * factor));
}
