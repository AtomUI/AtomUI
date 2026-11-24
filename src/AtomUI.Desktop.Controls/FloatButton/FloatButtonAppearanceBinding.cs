using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

// The outline Tokens of FloatButtonToken; null when the theme draws no outline (the Ant Design look).
internal sealed record FloatButtonOutlineAppearance(
    bool OutlineOnlyShadow,
    bool CapsuleDescription,
    bool UppercaseDescription,
    FontWeight DescriptionWeight,
    double DescriptionLineHeight)
{
    private static readonly ThemeTokenResolver s_resolver = new();

    internal static FloatButtonOutlineAppearance? Read(ThemeSnapshot? snapshot)
    {
        if (snapshot is null ||
            !snapshot.Registry.TryGetControl(FloatButtonTokens.Identity, out var descriptor) ||
            !s_resolver.GetOwn<bool>(snapshot, descriptor.Slot, FloatButtonTokenKind.HasOutline))
        {
            return null;
        }

        var slot = descriptor.Slot;
        return new FloatButtonOutlineAppearance(
            s_resolver.GetOwn<bool>(snapshot, slot, FloatButtonTokenKind.OutlineOnlyShadow),
            s_resolver.GetOwn<bool>(snapshot, slot, FloatButtonTokenKind.CapsuleDescription),
            s_resolver.GetOwn<bool>(snapshot, slot, FloatButtonTokenKind.UppercaseDescription),
            s_resolver.GetOwn<FontWeight>(snapshot, slot, FloatButtonTokenKind.DescriptionWeight),
            s_resolver.GetOwn<double>(snapshot, slot, FloatButtonTokenKind.OutlineDescriptionLineHeight));
    }
}

internal sealed class FloatButtonAppearanceBinding : IDisposable
{
    #region 公共属性定义

    public static readonly AttachedProperty<bool> HasOutlineProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, bool>("HasOutline");

    public static readonly AttachedProperty<bool> UseExplicitPrimaryShadowProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, bool>("UseExplicitPrimaryShadow");

    public static readonly AttachedProperty<BoxShadows> ExplicitPrimaryShadowProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, BoxShadows>("ExplicitPrimaryShadow");

    public static readonly AttachedProperty<bool> CapsuleDescriptionProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, bool>("CapsuleDescription");

    public static readonly AttachedProperty<bool> UppercaseDescriptionProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, bool>("UppercaseDescription");

    public static readonly AttachedProperty<FontWeight> DescriptionWeightProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, FontWeight>("DescriptionWeight");

    public static readonly AttachedProperty<double> DescriptionLineHeightProperty =
        AvaloniaProperty.RegisterAttached<FloatButtonAppearanceBinding, StyledElement, double>("DescriptionLineHeight");

    public static bool GetHasOutline(StyledElement control) => control.GetValue(HasOutlineProperty);
    public static bool GetUseExplicitPrimaryShadow(StyledElement control) => control.GetValue(UseExplicitPrimaryShadowProperty);
    public static BoxShadows GetExplicitPrimaryShadow(StyledElement control) => control.GetValue(ExplicitPrimaryShadowProperty);
    public static bool GetCapsuleDescription(StyledElement control) => control.GetValue(CapsuleDescriptionProperty);
    public static bool GetUppercaseDescription(StyledElement control) => control.GetValue(UppercaseDescriptionProperty);
    public static FontWeight GetDescriptionWeight(StyledElement control) => control.GetValue(DescriptionWeightProperty);
    public static double GetDescriptionLineHeight(StyledElement control) => control.GetValue(DescriptionLineHeightProperty);

    #endregion

    public static readonly IValueConverter CapsuleRadius =
        new FuncValueConverter<double, CornerRadius>(height => new CornerRadius(height / 2));

    public static readonly IValueConverter UppercaseLabel =
        new FuncValueConverter<string, string>(text => text?.ToUpper(System.Globalization.CultureInfo.CurrentUICulture) ?? string.Empty);

    public const double UppercaseLetterSpacing = 1.2;

    private static readonly ThemeTokenResolver s_resolver = new();
    private static readonly object s_explicitShadowKey =
        new ThemeTokenResourceProvider.ExplicitTokenLookup(FloatButtonTokenKind.FloatButtonShadows);
    private static readonly object s_explicitLineHeightKey =
        new ThemeTokenResourceProvider.ExplicitTokenLookup(FloatButtonTokenKind.DescriptionLineHeight);
    private readonly StyledElement _control;
    private readonly IDisposable _subscription;
    private FloatButtonOutlineAppearance? _appearance;
    private OwnerTokenSet? _buttonTokens;

    internal FloatButtonAppearanceBinding(StyledElement control)
    {
        _control = control;
        _subscription = s_resolver.Subscribe(control, Update);
        Update(ThemeScope.ResolveContext(control)?.Snapshot);
    }

    public void Dispose()
    {
        _subscription.Dispose();
        _buttonTokens?.Dispose();
        _buttonTokens = null;
        _control.ClearValue(HasOutlineProperty);
        _control.ClearValue(UseExplicitPrimaryShadowProperty);
        _control.ClearValue(ExplicitPrimaryShadowProperty);
        _control.ClearValue(CapsuleDescriptionProperty);
        _control.ClearValue(UppercaseDescriptionProperty);
        _control.ClearValue(DescriptionWeightProperty);
        _control.ClearValue(DescriptionLineHeightProperty);
    }

    private void Update(ThemeSnapshot? snapshot)
    {
        var appearance = FloatButtonOutlineAppearance.Read(snapshot);
        var useExplicitPrimaryShadow = _control is AbstractFloatButton &&
                                       appearance is { OutlineOnlyShadow: true };
        _appearance = appearance;
        if (_control is AbstractFloatButton && appearance is not null)
        {
            // Plain resource bindings cannot distinguish theme defaults from application overrides.
            _buttonTokens ??= new OwnerTokenSet(_control, 2, (index, _, value) => UpdateToken(index, value));
            _buttonTokens.SetResourceKey(0, useExplicitPrimaryShadow ? s_explicitShadowKey : null);
            _buttonTokens.SetResourceKey(1, s_explicitLineHeightKey);
            UpdateToken(0, _buttonTokens.GetValue(0));
            UpdateToken(1, _buttonTokens.GetValue(1));
        }
        else if (_buttonTokens is not null)
        {
            _buttonTokens.Dispose();
            _buttonTokens = null;
            _control.ClearValue(ExplicitPrimaryShadowProperty);
            _control.ClearValue(DescriptionLineHeightProperty);
        }

        _control.SetValue(HasOutlineProperty, appearance is not null);
        _control.SetValue(UseExplicitPrimaryShadowProperty, useExplicitPrimaryShadow);
        if (_control is AbstractFloatButton)
        {
            _control.SetValue(CapsuleDescriptionProperty, appearance?.CapsuleDescription ?? false);
            _control.SetValue(UppercaseDescriptionProperty, appearance?.UppercaseDescription ?? false);
            _control.SetValue(DescriptionWeightProperty, appearance?.DescriptionWeight ?? FontWeight.Normal);
        }
    }

    private void UpdateToken(int index, object? value)
    {
        if (index == 0)
        {
            _control.SetValue(ExplicitPrimaryShadowProperty, value is BoxShadows shadows ? shadows : default);
        }
        else
        {
            _control.SetValue(DescriptionLineHeightProperty,
                value is double height ? height : _appearance?.DescriptionLineHeight ?? 0);
        }
    }
}
