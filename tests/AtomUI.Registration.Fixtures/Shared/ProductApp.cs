using AtomUI.Desktop.Controls;
using AtomUI.Registration;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Schema;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
#if COMPLEX
using Fixture.ThirdParty;
using Fixture.ThirdParty.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
#endif
using TextBlock = Avalonia.Controls.TextBlock;
using Button = AtomUI.Desktop.Controls.Button;
using Expander = AtomUI.Desktop.Controls.Expander;
using NumericUpDown = AtomUI.Desktop.Controls.NumericUpDown;
using ButtonSpinner = AtomUI.Desktop.Controls.ButtonSpinner;
using TextArea = AtomUI.Desktop.Controls.TextArea;
using AtomToolTip = AtomUI.Desktop.Controls.ToolTip;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;

namespace AtomUI.Registration.Fixtures;

public partial class ProductApp : Application
{
    private Button _button = null!;
    private IReadOnlyList<EnabledControlPackage> _packages = null!;
    private IAtomUIBuilder _builder = null!;
#if COMPLEX
    private Expander _expander = null!;
    private IncludeControl _include = null!;
    private NumericUpDown _numeric = null!;
    private TextArea _textArea = null!;
    private ThemeConfigProvider _scope = null!;
#if !BROWSER
    private OtpLineEdit _otp = null!;
#endif
#endif
    private static IAtomUIBuilder? s_configuredBuilder;

    /// <summary>宿主在 BuildAvaloniaApp 链末尾登记产品配置。</summary>
    public static void Configure(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        // 配置回调早于 Application 实例创建，因此经静态字段转交，由 Initialize 取用。
        s_configuredBuilder = builder;
        builder.UseDesktopControls();
#if COMPLEX
        GC.KeepAlive(new TokenOnlyToken());
        GC.KeepAlive(IdentityOnlyTokens.Identity);
        GC.KeepAlive(EnumOnlyTokenKind.FixtureValue);
        GC.KeepAlive(TokenKeyOnlyTokenKey.FixtureValue);
        // Both stock ILC10 and ILC8 may remove KeepAlive of a boxed enum constant in no-scanner
        // builds. Exercise the enum observably so this scenario actually requires its CLR type.
        Console.WriteLine(TokenKeyOnlyTokenKey.FixtureValue);
        GC.KeepAlive(new ExtensionOnlyTokenResourceExtension());
        builder.UseFixtureControls();
#endif
    }

    public override void Initialize()
    {
        // Configure 在 Attach 之前同步执行，此时 builder 已就绪。
        _builder = s_configuredBuilder ?? throw new InvalidOperationException(
            "PRODUCT_FAIL the host did not register AtomUI configuration before startup.");
        _packages = ((ThemeManagerBuilder)_builder.Theme).PackageDeclarations;
#if COMPLEX
        Styles.Add(new ProductFixtureStyles());
        if (!this.TryFindResource(typeof(ButtonSpinner), out var spinnerBase) || spinnerBase is not Avalonia.Styling.ControlTheme original)
        {
            throw new InvalidOperationException("PRODUCT_FAIL ButtonSpinner base theme missing");
        }
        Resources[typeof(ButtonSpinner)] = new Avalonia.Styling.ControlTheme(typeof(ButtonSpinner))
        {
            BasedOn = original,
            Setters = { new Avalonia.Styling.Setter(Control.TagProperty, "application-spinner-theme") }
        };
#endif
    }

    public Control CreateContent()
    {
        _button = new Button { Content = "TypeMap product", IsMotionEnabled = false };
        var stack = new StackPanel();
        stack.Children.Add(_button);
#if COMPLEX
        _expander = new Expander { Header = "Published Expander", Content = "string content", IsExpanded = true, IsMotionEnabled = false };
        _include = new IncludeControl { Width = 100, Height = 25 };
        stack.Children.Add(_expander);
        stack.Children.Add(_include);
        _numeric = new NumericUpDown { Value = 42, Width = 220, Mode = NumericUpDownMode.Spinner, IsMotionEnabled = false, CornerRadius = new CornerRadius(11) };
        stack.Children.Add(_numeric);
        _textArea = new TextArea { Text = "AOT reflection probe", Lines = 2, Width = 220 };
        stack.Children.Add(_textArea);
#if !BROWSER
        _otp = new OtpLineEdit { Width = 260 };
        stack.Children.Add(_otp);
#endif
#endif
#if COMPLEX
        _scope = new ThemeConfigProvider { Child = stack, Config = new ThemeConfigBuilder().WithToken(nameof(DesignToken.FontSize), "19").Build() };
        return _scope;
#else
        return stack;
#endif
    }

    public async Task VerifyAsync()
    {
        Require(_button.Template is not null && _button.GetVisualDescendants().Any(), "Button template executes");
        var manager = this.GetThemeManager()!;
        Require(manager.CurrentTheme is not null, "ThemeManager initialized");
        Require(_packages.Count >= 2 && _packages[0].Definition.Id == "AtomUI.Controls.Common" &&
                _packages[1].Definition.Id == "AtomUI.Desktop.Controls", "Common commits before Desktop");
#if COMPLEX
        _button.Resources[ButtonTokenKind.ContentFontSize] = 23d;
        _button.UpdateLayout();
        Require(_button.FontSize == 23d, "local boxed-enum own Token override wins");
        _button.Resources[ButtonTokenKind.ContentFontSize] = 25d;
        _button.UpdateLayout();
        Require(_button.FontSize == 25d, "local boxed-enum own Token update republishes");
        _button.Resources.Remove(ButtonTokenKind.ContentFontSize);
        _button.UpdateLayout();
        Require(_button.FontSize == 19d, "removing local own Token restores scoped value");
        Require(_expander.Template is not null && _expander.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "string content"), "Expander string template executes");
        _expander.Content = new TextBlock { Text = "nonstring content" };
        _expander.UpdateLayout();
        Require(_expander.GetVisualDescendants().OfType<TextBlock>().Any(t => t.Text == "nonstring content"), "Expander nonstring content executes");
        Require(_include.Background is ISolidColorBrush brush && brush.Color == Color.Parse("#FF123456"), "cross-package ResourceInclude executes");
        Require(_textArea.Template is not null && _textArea.GetVisualDescendants().OfType<AvaloniaScrollViewer>().Any(),
            "trimmed TextArea private ScrollViewer adapter executes");
#if REAL_DESKTOP
        AtomToolTip.SetTip(_button, "AOT popup probe");
        AtomToolTip.SetIsOpen(_button, true);
        Require(AtomToolTip.GetIsOpen(_button), "trimmed Popup private parent adapter executes");
        AtomToolTip.SetIsOpen(_button, false);
#elif BROWSER
        // The fixture's SingleView host intentionally has no popup implementation or
        // overlay layer. Opening reaches AtomUI's private SetPopupParent adapter first;
        // the expected host failure therefore proves that trimmed reflection resolved.
        AtomToolTip.SetTip(_button, "AOT popup reflection probe");
        var reachedUnsupportedHost = false;
        try
        {
            AtomToolTip.SetIsOpen(_button, true);
        }
        catch (InvalidOperationException error) when (
            error.Message.Contains("Unable to create IPopupImpl", StringComparison.Ordinal))
        {
            reachedUnsupportedHost = true;
        }
        finally
        {
            AtomToolTip.SetIsOpen(_button, false);
            AtomToolTip.SetTip(_button, null);
        }
        Require(reachedUnsupportedHost, "trimmed Popup private parent adapter executes before unsupported host rejection");
#endif
#if !BROWSER
        Require(_otp.Template is not null && _otp.GetVisualDescendants().Any(control => control.GetType().Name == "OtpTextBox"),
            "OTP resource class creates internal text box template");
#endif
        var spinner = _numeric.GetVisualDescendants().OfType<ButtonSpinner>().Single();
        Require(_numeric.Template is not null && spinner.GetVisualDescendants().Any() &&
                spinner.TryFindResource(spinner.GetType(), out var spinnerTheme) &&
                spinnerTheme is Avalonia.Styling.ControlTheme { BasedOn.TargetType: { } baseTarget } && baseTarget == typeof(ButtonSpinner),
                "NumericUpDown typed ButtonSpinner base theme executes");
        Require(Equals(spinner.Tag, "application-spinner-theme"), "NumericUpDown inherits application ButtonSpinner theme replacement");
        Require(_numeric.CornerRadius == new CornerRadius(11) && !spinner.IsMotionEnabled,
                "NumericUpDown local customization survives inherited theme");
        Require(_packages.Any(p => p.Definition.Id == "Fixture.ThirdParty"), "third-party package enabled");
        // Query selected runtime descriptors, rather than the full declaration manifest: a
        // declaration alone would not prove that trimming retained the selected fragment.
        var registry = ((ThemeManager)manager).RootContext.Snapshot.Registry;
        Require(registry.TryGetControl(new ControlTokenIdentity("Fixture.ThirdParty", "TokenOnly"), out _) &&
                registry.TryGetControl(new ControlTokenIdentity("Fixture.ThirdParty", "IdentityOnly"), out _),
                "token-only and typed-identity-only registration");
        Require(registry.TryGetControl(new ControlTokenIdentity("Fixture.ThirdParty", "EnumOnly"), out _) &&
                registry.TryGetControl(new ControlTokenIdentity("Fixture.ThirdParty", "TokenKeyOnly"), out _) &&
                registry.TryGetControl(new ControlTokenIdentity("Fixture.ThirdParty", "ExtensionOnly"), out _),
                "own-enum-only, TokenKey-only and extension-only registration");
        Require(_button.TryFindResource(EnumOnlyTokenKind.FixtureValue, out var enumValue) && Equals(enumValue, 31d), "boxed-enum-only own resource materializes");
        Require(_button.TryFindResource(ControlTokenResourceKey.Own(new ControlTokenIdentity("Fixture.ThirdParty", "TokenOnly"), TokenOnlyTokenKind.FixtureValue), out var tokenValue) && Equals(tokenValue, 17d), "token-only own resource materializes");
        var semantic = manager.SemanticParts.Controls.Single(c => c.Identity.Id == "SemanticOnly").Parts.Single(p => p.Name == "leaf");
        Require(semantic.Path == "leaf" && semantic.SelectorClass == "semantic-fixture-leaf" && semantic.SelectorRoute == ">> .semantic-fixture-leaf" && semantic.ContractType == typeof(Border) &&
                semantic.Cardinality == SemanticPartCardinality.Optional && semantic.Customization == SemanticPartCustomization.Selector && semantic.Theme is null && !semantic.CrossVisualRoot &&
                semantic.Since == "1.0.0" && semantic.RuntimeCreated && semantic.CrossNestedOwners && semantic.RestHidden && semantic.StyleType == typeof(SemanticOnlyLeafStyle), "semantic-only metadata preserved");
        var dark = await manager.ApplyThemeAsync(new ThemeRequest(IThemeManager.DEFAULT_THEME_ID,
            new ThemeConfigBuilder().WithAlgorithms(ThemeAlgorithm.Default, ThemeAlgorithm.Dark).Build(), ThemeTransitionReason.UserRequest));
        Require(dark.Status == ThemeTransitionStatus.Committed && manager.CurrentTheme!.Appearance == ThemeAppearance.Dark, "dark theme commits");
        _scope.Config = new ThemeConfigBuilder().WithToken(nameof(DesignToken.FontSize), "29").Build();
        _button.UpdateLayout();
        Require(_button.FontSize == 29d, "scoped token update reaches template");
#endif
        await Task.CompletedTask;
        try
        {
            _builder.UseDesktopControls();
            throw new Exception("Frozen registry accepted late entry");
        }
        catch (InvalidOperationException e)
        {
            Require(e.Message.Contains("frozen", StringComparison.Ordinal), "Registry frozen after startup");
        }
    }
    private static void Require(bool value, string message)
    {
        if (!value)
        {
            throw new InvalidOperationException("PRODUCT_FAIL " + message);
        }
        Console.WriteLine("PRODUCT_CHECK " + message);
    }
}
