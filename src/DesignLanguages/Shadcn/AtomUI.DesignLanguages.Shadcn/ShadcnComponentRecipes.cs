using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.DesignLanguages.Shadcn;

/// <summary>Table roles used by the shadcn DataGrid value package.</summary>
public readonly ref struct ShadcnTableRecipe
{
    private readonly DesignToken _global;

    internal ShadcnTableRecipe(DesignToken global)
    {
        _global = global;
    }

    public Color HeaderBackground => ColorUtils.OnBackground(_global.ColorFillAlter, RowBackground);
    public Color HeaderForeground => _global.ColorTextHeading;
    public Color RowBackground => _global.ColorBgContainer;
    public Color RowHoverBackground => HeaderBackground;
    public Color RowSelectedBackground => _global.ControlItemBgActive;
    public Color RowSelectedForeground => _global.ControlItemColorActive;
    public Color RowSelectedHoverBackground => _global.ControlItemBgActiveHover;
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

/// <summary>The five shadcn chart series colors in registry order.</summary>
public readonly ref struct ShadcnChartRecipe
{
    private readonly ShadcnToken _language;

    internal ShadcnChartRecipe(ShadcnToken language)
    {
        _language = language;
    }

    public Color Series1 => _language.Chart1;
    public Color Series2 => _language.Chart2;
    public Color Series3 => _language.Chart3;
    public Color Series4 => _language.Chart4;
    public Color Series5 => _language.Chart5;
}

/// <summary>Interactive list item roles for shadcn list and select values.</summary>
public readonly ref struct ShadcnListItemRecipe
{
    private readonly DesignToken _global;

    internal ShadcnListItemRecipe(DesignToken global)
    {
        _global = global;
    }

    public Color HoverBackground => _global.ColorBgTextHover;
    public Color HoverForeground => _global.ColorTextSecondary;
    public Color FocusBackground => _global.ControlItemBgHover;
    public Color FocusForeground => SelectedForeground;
    public Color SelectedBackground => _global.ControlItemBgActive;
    public Color SelectedForeground => _global.ControlItemColorActive;
}

internal enum ShadcnOverlayKind
{
    Menu,
    Popover,
    Dialog
}

/// <summary>Panel roles from the shadcn menu, popover or dialog style table.</summary>
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

    public Color Background => _global.ColorBgElevated;
    public Color BorderColor => _language.OutlineRing;
    public BoxShadows Shadows => _kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogShadow,
        ShadcnOverlayKind.Popover => _language.PopoverShadow,
        _ => _language.MenuShadow
    };

    /// <summary>The outline as a one-pixel ring in front of the panel shadow.</summary>
    public BoxShadows ShadowsWithOutline => ShadcnRecipes.RingThenShadow(BorderColor, Shadows);

    public CornerRadius CornerRadius => new(_kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogRadius,
        ShadcnOverlayKind.Popover => _language.PopoverRadius,
        _ => _language.MenuRadius
    });

    public Thickness Padding => new(_kind switch
    {
        ShadcnOverlayKind.Dialog => _language.DialogPadding,
        ShadcnOverlayKind.Popover => _language.PopoverPadding,
        _ => _language.MenuPadding
    });

    public Color Mask => _kind == ShadcnOverlayKind.Dialog ? _global.ColorBgMask : Colors.Transparent;
}

/// <summary>Form-field roles from the shadcn input style and the effective global tokens.</summary>
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

    public Color Background => GetBackground(BorderColor);
    public Color BorderColor => _language.Input;
    public Color HoverBorderColor => BottomBorderOnly ? BorderColor : FocusBorderColor;
    public Color FocusBorderColor => _language.Ring;
    public BoxShadows FocusRing => BottomBorderOnly ? default : _global.InputActiveShadow;
    public Color PlaceholderForeground => _global.ColorTextPlaceholder;
    public Color DisabledBackground => _global.ColorBgContainerDisabled;
    public Color DisabledForeground => _global.ColorTextDisabled;
    public CornerRadius CornerRadius => new(_language.InputRadius);
    public bool BottomBorderOnly => _variant.Style == ShadcnStyles.Sera;
    public bool TransparentBorder => _variant.Style == ShadcnStyles.Luma || _variant.Style == ShadcnStyles.Rhea;

    /// <summary>The input fill before applying the Luma and Rhea field frame.</summary>
    public Color OutlinedBackground => _variant.IsDark && _language.DarkInputFill > 0
        ? ScaleAlpha(BorderColor, _language.DarkInputFill)
        : Colors.Transparent;

    /// <summary>Resolves the background using the field's effective border, including a control-level override.</summary>
    public Color GetBackground(Color effectiveBorderColor) =>
        TransparentBorder ? ScaleAlpha(effectiveBorderColor, 0.5) : OutlinedBackground;

    private static Color ScaleAlpha(Color color, double factor) =>
        ShadcnRecipes.WithAlpha(color, (byte)Math.Round(color.A * factor));
}
