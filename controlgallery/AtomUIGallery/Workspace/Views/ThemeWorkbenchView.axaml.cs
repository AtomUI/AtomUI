using AtomUI;
using AtomUI.Theme;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Branding;
using AtomUI.Theme.Schema;
using AtomUIGallery.Workspace.Theming;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace AtomUIGallery.Workspace.Views;

public partial class ThemeWorkbenchView : UserControl, IDisposable
{
    private sealed record OwnerChoice(string Label, ControlTokenDescriptor? Descriptor)
    {
        public override string ToString() => Label;
    }

    private sealed record TokenChoice(TokenDescriptor Descriptor, bool IsOwn)
    {
        public override string ToString() => $"{(IsOwn ? "Own" : "Global")} · {Descriptor.Name}";
    }

    private ThemeWorkbenchSession? _session;
    private IThemeManager? _runtime;
    private IReadOnlyList<TokenDescriptor> _globalTokens = Array.Empty<TokenDescriptor>();
    private bool _updating;
    private bool _jsonEdited;
    private long _editorRevision;
    private readonly CancellationTokenSource _lifetime = new();

    public ThemeWorkbenchView()
    {
        InitializeComponent();
        PresetPicker.SelectionChanged += (_, _) => SelectPreset();
        DarkPicker.IsCheckedChanged += (_, _) => SelectPreset();
        OwnerPicker.SelectionChanged += (_, _) => RefreshTokens();
        TokenPicker.SelectionChanged += (_, _) => RefreshValue();
        PreviewButton.Click += (_, _) => Run(PreviewDraft);
        ApplyButton.Click += async (_, _) => await ApplyDraftAsync();
        CancelButton.Click += (_, _) => _session?.CancelPendingApply();
        ResetButton.Click += (_, _) => Run(() => { _jsonEdited = false; _session!.ResetBrand(); });
        SetValueButton.Click += (_, _) => Run(() =>
        {
            var token = SelectedToken();
            var text = ValueEditor.Text ?? string.Empty;
            token.Descriptor.Parse(text);
            _session!.SetToken(SelectedOwner(), token.Descriptor.Name, text);
            RefreshValue();
        });
        InheritButton.Click += (_, _) => Run(() => _session!.SetToken(SelectedOwner(), SelectedToken().Descriptor.Name, null));
        ReadJsonButton.Click += (_, _) => Run(() => { _jsonEdited = false; _session!.SetJson(JsonEditor.Text ?? string.Empty); });
        JsonEditor.TextChanged += (_, _) =>
        {
            if (!_updating)
            {
                // TextChanged can be delivered after the programmatic update guard has exited.
                _jsonEdited = !string.Equals(JsonEditor.Text, _session?.Json, StringComparison.Ordinal);
                if (_jsonEdited)
                {
                    _editorRevision++;
                    StatusText.Text = "JSON has unread edits. Read the JSON draft to validate.";
                }
                RefreshActions();
            }
        };
        ImportButton.Click += async (_, _) => await ImportAsync();
        SaveButton.Click += async (_, _) => await SaveAsync(ThemeBrandRecipeKind.Complete);
        TokenExportButton.Click += async (_, _) => await SaveAsync(ThemeBrandRecipeKind.TokenOnly);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (_session is not null || _lifetime.IsCancellationRequested)
        {
            return;
        }
        _runtime = Application.Current!.GetThemeManager() ?? throw new InvalidOperationException("The Gallery theme runtime is unavailable.");
        _session = new ThemeWorkbenchSession(_runtime);
        _session.Changed += HandleSessionChanged;
        _globalTokens = ThemeTokenInspection.GlobalTokens(_runtime);
        _updating = true;
        PresetPicker.ItemsSource = GalleryBrandPresets.Ids;
        DarkPicker.IsChecked = _runtime.CurrentTheme!.Appearance == ThemeAppearance.Dark;
        var owners = new List<OwnerChoice> { new("Global", null) };
        owners.AddRange(ThemeTokenInspection.Controls(_runtime).OrderBy(static owner => owner.Identity.ToString(), StringComparer.Ordinal)
            .Select(static owner => new OwnerChoice(owner.Identity.ToString(), owner)));
        OwnerPicker.ItemsSource = owners;
        OwnerPicker.SelectedIndex = 0;
        _updating = false;
        RefreshTokens();
        BaselineScope.Config = ThemeBrandRecipes.CreateConfig(_runtime, _session.Baseline);
        PreviewDraft();
        Refresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Dispose();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        if (_lifetime.IsCancellationRequested)
        {
            return;
        }
        _lifetime.Cancel();
        if (_session is { } session)
        {
            session.Changed -= HandleSessionChanged;
            session.Dispose();
        }
        _session = null;
        _runtime = null;
        _globalTokens = Array.Empty<TokenDescriptor>();
    }

    private void HandleSessionChanged(object? sender, EventArgs e) => Refresh();

    private void SelectPreset()
    {
        if (_updating || _session is null || PresetPicker.SelectedItem is not string id)
        {
            return;
        }
        Run(() =>
        {
            _jsonEdited = false;
            _session.SelectPreset(id, DarkPicker.IsChecked == true);
            BaselineScope.Config = ThemeBrandRecipes.CreateConfig(_runtime!, _session.Baseline);
            PreviewDraft();
        });
    }

    private void PreviewDraft()
    {
        if (_session is { } session)
        {
            session.Preview(DraftScope, session.Revision, _lifetime.Token);
            RefreshValue();
        }
    }

    private async Task ApplyDraftAsync()
    {
        if (_session is not { } session)
        {
            return;
        }
        try
        {
            await session.ApplyAsync(_lifetime.Token);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or FormatException or OverflowException)
        {
            DiagnosticsText.Text = exception.Message;
        }
    }

    private void RefreshTokens()
    {
        if (_updating || _session is null)
        {
            return;
        }
        var choices = _globalTokens.Select(static token => new TokenChoice(token, false)).ToList();
        if (OwnerPicker.SelectedItem is OwnerChoice { Descriptor: { } owner })
        {
            choices.AddRange(owner.OwnTokens.Select(static token => new TokenChoice(token, true)));
        }
        TokenPicker.ItemsSource = choices;
        TokenPicker.SelectedIndex = choices.FindIndex(static item => item.Descriptor.Name == "FontSize");
        RefreshValue();
    }

    private void RefreshValue()
    {
        if (_session is not { } session || TokenPicker.SelectedItem is not TokenChoice token || !DraftScope.IsAttachedToVisualTree())
        {
            return;
        }
        Run(() =>
        {
            var value = ThemeTokenInspection.Read(DraftScope, SelectedOwner())
                .Single(item => item.Name == token.Descriptor.Name && item.IsOwn == token.IsOwn);
            var explicitValue = session.ExplicitValue(SelectedOwner(), value.Name);
            ValueEditor.Text = explicitValue ?? value.EffectiveValue;
            var guidance = Guidance(token);
            ValueInfoText.Text = $"{value.ValueType.Name} · {(explicitValue is null ? "Inherited/default" : "Explicit")}\nEffective: {value.EffectiveValue}" +
                "\n" + guidance.Description;
            RefreshActions();
        });
    }

    private void Refresh()
    {
        if (_session is not { } session)
        {
            return;
        }
        _updating = true;
        if (!_jsonEdited)
        {
            JsonEditor.Text = session.Json;
        }
        _updating = false;
        StatusText.Text = $"Draft r{session.Revision} · Preview {(session.PreviewRevision == session.Revision ? "current" : "stale")} · " +
            $"Last applied draft {(session.AppliedRevision?.ToString() ?? "—")} · Saved {(_jsonEdited || session.HasUnsavedChanges ? "modified" : "current")}" +
            (session.LastApplyResult is { } applied ? $" · Token: {applied.Status} / Presentation: {applied.Presentation.Status}" : string.Empty);
        StatusText.Text += $" · Theme: {session.ActiveTheme?.ThemeId} · Brand: {session.ActiveBrand?.Id ?? "—"}";
        StatusText.Text += $" · Scope: {session.Draft?.Kind.ToString() ?? "invalid"}";
        DiffText.Text = string.Join("\n", session.Diff().Select(static item => $"{item.Path}\n− {item.Before ?? "∅"}\n+ {item.After ?? "∅"}"));
        DiagnosticsText.Text = string.Join("\n", session.Diagnostics);
        RefreshActions();
    }

    private void RefreshActions()
    {
        var valid = !_jsonEdited && _session?.Draft is not null && !_lifetime.IsCancellationRequested;
        CancelButton.IsEnabled = _session?.IsApplying == true;
        PreviewButton.IsEnabled = valid;
        ApplyButton.IsEnabled = valid;
        SaveButton.IsEnabled = valid && _session!.Draft!.Kind == ThemeBrandRecipeKind.Complete;
        TokenExportButton.IsEnabled = valid;
        var editable = valid && TokenPicker.SelectedItem is TokenChoice token && !Guidance(token).IsReadOnly;
        SetValueButton.IsEnabled = editable;
        InheritButton.IsEnabled = editable;
    }

    private GalleryTokenGuidance Guidance(TokenChoice token) => GalleryTokenGuidanceCatalog.Describe(
        (OwnerPicker.SelectedItem as OwnerChoice)?.Descriptor, token.Descriptor.Name, token.IsOwn);

    private ControlTokenIdentity? SelectedOwner() => (OwnerPicker.SelectedItem as OwnerChoice)?.Descriptor?.Identity;
    private TokenChoice SelectedToken() => TokenPicker.SelectedItem as TokenChoice ?? throw new InvalidOperationException("Select a token first.");

    private async Task ImportAsync()
    {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage?.CanOpen != true || _session is not { } session)
        {
            DiagnosticsText.Text = "This host does not provide file import. Paste a complete recipe into the JSON draft.";
            return;
        }
        var generation = session.Revision;
        var editorGeneration = _editorRevision;
        try
        {
            var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false, FileTypeFilter = [JsonFileType] });
            if (files.Count == 0)
            {
                return;
            }
            using var file = files[0];
            var text = await ThemeWorkbenchFiles.ReadAsync(file, _lifetime.Token);
            if (!_lifetime.IsCancellationRequested && ReferenceEquals(session, _session) && generation == session.Revision && editorGeneration == _editorRevision)
            {
                _jsonEdited = false;
                session.SetJson(text);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OperationCanceledException or ArgumentException or NotSupportedException)
        {
            if (!_lifetime.IsCancellationRequested)
            {
                DiagnosticsText.Text = exception.Message;
            }
        }
    }

    private async Task SaveAsync(ThemeBrandRecipeKind kind)
    {
        if (_session is not { } session)
        {
            return;
        }
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage?.CanSave != true)
        {
            DiagnosticsText.Text = "This host does not provide file saving. The complete recipe remains in the JSON draft.";
            return;
        }
        try
        {
            var text = session.Export(kind);
            using var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = kind == ThemeBrandRecipeKind.Complete ? "atomui-brand.json" : "atomui-tokens.json",
                DefaultExtension = "json", FileTypeChoices = [JsonFileType]
            });
            if (file is null || _lifetime.IsCancellationRequested)
            {
                return;
            }
            await ThemeWorkbenchFiles.WriteAsync(file, text, _lifetime.Token);
            if (kind == ThemeBrandRecipeKind.Complete && !_lifetime.IsCancellationRequested && ReferenceEquals(session, _session))
            {
                session.MarkSaved(text);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or OperationCanceledException or InvalidOperationException or NotSupportedException)
        {
            if (!_lifetime.IsCancellationRequested)
            {
                DiagnosticsText.Text = exception.Message;
            }
        }
    }

    private static readonly FilePickerFileType JsonFileType = new("JSON") { Patterns = ["*.json"], MimeTypes = ["application/json"] };

    private void Run(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or FormatException or OverflowException)
        {
            DiagnosticsText.Text = exception.Message;
        }
    }
}
