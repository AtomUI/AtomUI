using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace AtomUIGallery.Controls;

public class AxamlCodeBlock : TemplatedControl
{
    internal const string EditorPart = "PART_Editor";
    private const ThemeName FallbackLightTheme = ThemeName.LightPlus;
    private const ThemeName FallbackDarkTheme = ThemeName.DarkPlus;

    private static readonly SolidColorBrush DefaultLightBackground = new(Color.Parse("#FFFFFF"));
    private static readonly SolidColorBrush DefaultDarkBackground = new(Color.Parse("#1E1E1E"));
    private static readonly SolidColorBrush DefaultLightForeground = new(Color.Parse("#1F2328"));
    private static readonly SolidColorBrush DefaultDarkForeground = new(Color.Parse("#D4D4D4"));
    private static readonly SolidColorBrush DefaultLightLineNumberForeground = new(Color.Parse("#8C959F"));
    private static readonly SolidColorBrush DefaultDarkLineNumberForeground = new(Color.Parse("#858585"));

    public static readonly StyledProperty<string?> CodeProperty =
        AvaloniaProperty.Register<AxamlCodeBlock, string?>(nameof(Code));

    private TextEditor? _editor;
    private RegistryOptions? _registryOptions;
    private TextMate.Installation? _textMateInstallation;
    private bool _isThemeVariantSubscribed;

    public string? Code
    {
        get => GetValue(CodeProperty);
        set => SetValue(CodeProperty, value);
    }

    static AxamlCodeBlock()
    {
        CodeProperty.Changed.AddClassHandler<AxamlCodeBlock>((codeBlock, args) =>
        {
            codeBlock.UpdateEditorText(args.GetNewValue<string?>());
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UnSubscribeThemeVariantChangedEvent();
        DisposeTextMateInstallation();
        _editor = e.NameScope.Find(EditorPart) as TextEditor;
        SubscribeThemeVariantChangedEvent();
        TryInstallTextMate();
        UpdateEditorText(Code);
        ApplyEditorTheme();
        base.OnApplyTemplate(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnSubscribeThemeVariantChangedEvent();
        DisposeTextMateInstallation();
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateEditorText(string? code)
    {
        if (_editor is null)
        {
            return;
        }

        var text = code ?? string.Empty;
        if (_editor.Text != text)
        {
            _editor.Text = text;
        }
    }

    private void TryInstallTextMate()
    {
        if (_editor is null)
        {
            return;
        }

        try
        {
            _registryOptions = new RegistryOptions(FallbackLightTheme);
            _textMateInstallation = _editor.InstallTextMate(_registryOptions);

            var language = _registryOptions.GetLanguageByExtension(".axaml")
                           ?? _registryOptions.GetLanguageByExtension(".xaml")
                           ?? _registryOptions.GetLanguageByExtension(".xml");
            if (language is null)
            {
                return;
            }

            var scope = _registryOptions.GetScopeByLanguageId(language.Id);
            if (!string.IsNullOrWhiteSpace(scope))
            {
                _textMateInstallation.SetGrammar(scope);
            }

            ApplyEditorTheme();
        }
        catch
        {
            DisposeTextMateInstallation();
        }
    }

    private void DisposeTextMateInstallation()
    {
        _textMateInstallation?.Dispose();
        _textMateInstallation = null;
        _registryOptions = null;
    }

    private void HandleActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ApplyEditorTheme();
    }

    private void SubscribeThemeVariantChangedEvent()
    {
        if (_isThemeVariantSubscribed)
        {
            return;
        }

        if (Application.Current is { } application)
        {
            application.ActualThemeVariantChanged += HandleActualThemeVariantChanged;
            _isThemeVariantSubscribed = true;
        }
    }

    private void UnSubscribeThemeVariantChangedEvent()
    {
        if (!_isThemeVariantSubscribed)
        {
            return;
        }

        if (Application.Current is { } application)
        {
            application.ActualThemeVariantChanged -= HandleActualThemeVariantChanged;
        }

        _isThemeVariantSubscribed = false;
    }

    private void ApplyEditorTheme()
    {
        if (_editor is null)
        {
            return;
        }

        var isDarkThemeMode = IsDarkThemeMode();
        if (_textMateInstallation is not null &&
            _registryOptions is not null)
        {
            var themeName = isDarkThemeMode ? FallbackDarkTheme : FallbackLightTheme;
            try
            {
                _textMateInstallation.SetTheme(_registryOptions.LoadTheme(themeName));
            }
            catch
            {
                // ignore theme switch failure, keep editor usable
            }
        }

        ApplyEditorPalette(isDarkThemeMode);
    }

    private void ApplyEditorPalette(bool isDarkThemeMode)
    {
        if (_editor is null)
        {
            return;
        }

        var defaultBackground = isDarkThemeMode ? DefaultDarkBackground : DefaultLightBackground;
        var defaultForeground = isDarkThemeMode ? DefaultDarkForeground : DefaultLightForeground;
        var defaultLineNumberForeground = isDarkThemeMode
            ? DefaultDarkLineNumberForeground
            : DefaultLightLineNumberForeground;

        _editor.Background = defaultBackground;
        _editor.Foreground = defaultForeground;
        _editor.LineNumbersForeground = defaultLineNumberForeground;
    }

    private static bool IsDarkThemeMode()
    {
        return Application.Current?.IsDarkThemeMode() ?? false;
    }
}
