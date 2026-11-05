using System.Text.Json;
using AtomUI.Theme;
using AtomUI.Theme.Branding;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUIGallery.Workspace.Theming;

internal sealed record ThemeDraftDifference(string Path, string? Before, string? After);

/// <summary>Owns draft generations and saved input; the existing runtime owns every effective value and commit.</summary>
internal sealed class ThemeWorkbenchSession : IDisposable
{
    private readonly IThemeManager _runtime;
    private CancellationTokenSource? _applyCancellation;
    private bool _disposed;
    private readonly Dictionary<ThemeConfigProvider, Action<bool>> _pendingPreviews = new();

    internal ThemeWorkbenchSession(IThemeManager runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        Baseline = ThemeBrandRecipes.ExportCurrent(runtime, "Gallery.Workbench", "1");
        Draft = Baseline;
        Json = ThemeBrandRecipes.Write(Baseline);
        _runtime.ThemeChanged += HandleRuntimeThemeChanged;
        _runtime.ThemePresentationChanged += HandlePresentationChanged;
    }

    internal ThemeBrandRecipe Baseline { get; private set; }
    internal ThemeBrandRecipe? Draft { get; private set; }
    internal string Json { get; private set; }
    internal string? SavedJson { get; private set; }
    internal long Revision { get; private set; }
    internal long? PreviewRevision { get; private set; }
    internal long? AppliedRevision { get; private set; }
    internal ThemeTransitionResult? LastApplyResult { get; private set; }
    internal IReadOnlyList<string> Diagnostics { get; private set; } = Array.Empty<string>();
    internal ThemeState? ActiveTheme => _runtime.CurrentTheme;
    internal ThemeBrandSelection? ActiveBrand => ThemeBrandRecipes.GetSelection(_runtime);
    internal bool IsApplying => _applyCancellation is not null;
    internal bool HasUnsavedChanges => !string.Equals(Json, SavedJson, StringComparison.Ordinal);
    internal event EventHandler? Changed;

    internal bool SetJson(string json)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(json);
        _applyCancellation?.Cancel();
        CancelPendingPreviews();
        Revision++;
        PreviewRevision = null;
        Json = json;
        var result = ThemeBrandRecipes.Read(_runtime, json);
        Draft = result.Recipe;
        Json = result.Success ? ThemeBrandRecipes.Write(result.Recipe!) : json;
        Diagnostics = result.Diagnostics.Select(static item => $"{item.Code} {item.Path}: {item.Message}").ToArray();
        Changed?.Invoke(this, EventArgs.Empty);
        return result.Success;
    }

    internal void SelectPreset(string id, bool dark)
    {
        var recipe = GalleryBrandPresets.Create(_runtime, id, dark);
        if (SetJson(ThemeBrandRecipes.Write(recipe)))
        {
            Baseline = recipe;
        }
    }

    internal void ResetBrand() => SetJson(ThemeBrandRecipes.Write(Baseline));

    internal bool SetToken(ControlTokenIdentity? owner, string name, string? value)
    {
        var recipe = RequireDraft();
        var config = new ThemeConfigBuilder().WithInherit(recipe.Config.Inherit);
        if (recipe.Config.Algorithms is { } algorithms)
        {
            config.WithAlgorithms(algorithms.ToArray());
        }
        foreach (var token in recipe.Config.Tokens)
        {
            if (owner is not null || token.Key != name)
            {
                config.WithToken(token.Key, token.Value);
            }
        }
        if (owner is null && value is not null)
        {
            config.WithToken(name, value);
        }
        var found = false;
        foreach (var entry in recipe.Config.Controls)
        {
            if (owner == entry.Key)
            {
                found = true;
                var updatedControl = EditControl(entry.Value, name, value);
                if (updatedControl.Tokens.Count != 0 || updatedControl.Algorithm != ControlAlgorithmMode.Unspecified ||
                    updatedControl.Algorithms is not null)
                {
                    config.WithControl(entry.Key, updatedControl);
                }
            }
            else
            {
                config.WithControl(entry.Key, entry.Value);
            }
        }
        if (owner is { } identity && !found && value is not null)
        {
            config.WithControl(identity, new ControlThemeConfigBuilder().WithToken(name, value).Build());
        }
        var updated = new ThemeBrandRecipe(recipe.Id, recipe.Revision, recipe.ThemeId, config.Build(),
            recipe.Assets, recipe.Requirements, recipe.Kind);
        return SetJson(ThemeBrandRecipes.Write(updated));
    }

    internal string? ExplicitValue(ControlTokenIdentity? owner, string name)
    {
        var config = Draft?.Config;
        if (config is null)
        {
            return null;
        }
        return owner is { } identity
            ? config.Controls.TryGetValue(identity, out var control) && control.Tokens.TryGetValue(name, out var value) ? value : null
            : config.Tokens.TryGetValue(name, out var globalValue) ? globalValue : null;
    }

    internal bool Preview(ThemeConfigProvider target, long expectedRevision, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(target);
        if (expectedRevision != Revision || cancellationToken.IsCancellationRequested || Draft is null || !target.IsAttachedToVisualTree())
        {
            return false;
        }
        if (_pendingPreviews.Remove(target, out var previousRequest))
        {
            previousRequest(true);
        }
        var previous = target.Config;
        var bound = ThemeBrandRecipes.CreateConfig(_runtime, Draft);
        var committed = false;
        var finished = false;
        CancellationTokenRegistration cancellation = default;
        void Finish(bool restore)
        {
            if (finished)
            {
                return;
            }
            finished = true;
            target.ThemeTransitionCompleted -= Completed;
            _pendingPreviews.Remove(target);
            cancellation.Dispose();
            if (restore && ReferenceEquals(target.Config, bound))
            {
                target.Config = previous;
            }
        }
        void Completed(object? sender, ThemeTransitionCompletedEventArgs args)
        {
            if (!ReferenceEquals(args.Request.Config, bound))
            {
                return;
            }
            var valid = !_disposed && expectedRevision == Revision && !cancellationToken.IsCancellationRequested;
            committed = valid && args.Result.Status is ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp &&
                args.Result.Presentation.Status != ThemePresentationStatus.Degraded;
            Finish(!committed);
            if (valid)
            {
                PreviewRevision = committed ? expectedRevision : null;
                Diagnostics = args.Result.Diagnostics.Concat(args.Result.PublishDiagnostics)
                    .Concat(args.Result.Presentation.Diagnostics)
                    .Select(static item => $"{item.Code} {item.Path}: {item.Message}").ToArray();
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
        target.ThemeTransitionCompleted += Completed;
        _pendingPreviews[target] = Finish;
        cancellation = cancellationToken.Register(() =>
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Finish(true);
            }
            else
            {
                Dispatcher.UIThread.Post(() => Finish(true));
            }
        });
        if (finished)
        {
            cancellation.Dispose();
            return false;
        }
        try
        {
            target.Config = bound;
            // Config is an input, not a receipt. Queued scope updates become current only on a completed receipt.
            return committed;
        }
        catch
        {
            Finish(true);
            throw;
        }
    }

    internal void CancelPendingApply() => _applyCancellation?.Cancel();

    internal async Task<ThemeTransitionResult?> ApplyAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var recipe = RequireDraft();
        var generation = Revision;
        _applyCancellation?.Cancel();
        using var request = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _applyCancellation = request;
        Changed?.Invoke(this, EventArgs.Empty);
        try
        {
            var result = await ThemeBrandRecipes.ApplyAsync(_runtime, recipe, request.Token);
            if (!_disposed && generation == Revision && !request.IsCancellationRequested)
            {
                LastApplyResult = result;
                if (result.Status is ThemeTransitionStatus.Committed or ThemeTransitionStatus.NoOp &&
                    result.Presentation.Status != ThemePresentationStatus.Degraded)
                {
                    AppliedRevision = generation;
                }
                Diagnostics = result.Diagnostics.Concat(result.PublishDiagnostics)
                    .Concat(result.Presentation.Diagnostics).Select(static item => $"{item.Code} {item.Path}: {item.Message}").ToArray();
                Changed?.Invoke(this, EventArgs.Empty);
            }
            return result;
        }
        catch (OperationCanceledException) when (request.IsCancellationRequested)
        {
            return null;
        }
        finally
        {
            if (ReferenceEquals(_applyCancellation, request))
            {
                _applyCancellation = null;
                if (!_disposed)
                {
                    Changed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    internal string Export(ThemeBrandRecipeKind kind = ThemeBrandRecipeKind.Complete)
    {
        var recipe = RequireDraft();
        if (kind == ThemeBrandRecipeKind.Complete && recipe.Kind != ThemeBrandRecipeKind.Complete)
        {
            throw new InvalidOperationException("A token-only import cannot be exported as a complete brand.");
        }
        return ThemeBrandRecipes.Write(kind == recipe.Kind ? recipe : new ThemeBrandRecipe(recipe.Id, recipe.Revision,
            recipe.ThemeId, recipe.Config, [], recipe.Requirements, ThemeBrandRecipeKind.TokenOnly));
    }

    internal void MarkSaved(string actualSavedJson)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // A file picker/write may finish after another edit. Only the exact bytes written become saved state.
        SavedJson = actualSavedJson;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    internal IReadOnlyList<ThemeDraftDifference> Diff()
    {
        var before = Flatten(ThemeBrandRecipes.Write(Baseline));
        Dictionary<string, string> after;
        try
        {
            after = Flatten(Json);
        }
        catch (JsonException)
        {
            return [new ThemeDraftDifference("$", ThemeBrandRecipes.Write(Baseline), Json)];
        }
        return before.Keys.Concat(after.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
            .Where(key => before.GetValueOrDefault(key) != after.GetValueOrDefault(key))
            .Select(key => new ThemeDraftDifference(key, before.GetValueOrDefault(key), after.GetValueOrDefault(key))).ToArray();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _applyCancellation?.Cancel();
        CancelPendingPreviews();
        _runtime.ThemeChanged -= HandleRuntimeThemeChanged;
        _runtime.ThemePresentationChanged -= HandlePresentationChanged;
        Changed = null;
    }

    private void CancelPendingPreviews()
    {
        foreach (var request in _pendingPreviews.Values.ToArray())
        {
            request(true);
        }
    }

    private void HandleRuntimeThemeChanged(object? sender, ThemeChangedEventArgs args)
    {
        AppliedRevision = null;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void HandlePresentationChanged(object? sender, ThemePresentationChangedEventArgs args) => Changed?.Invoke(this, EventArgs.Empty);

    private ThemeBrandRecipe RequireDraft() => Draft ?? throw new InvalidOperationException("Repair the draft diagnostics before preview, apply or export.");

    private static ControlThemeConfig EditControl(ControlThemeConfig source, string name, string? value)
    {
        var builder = new ControlThemeConfigBuilder().WithAlgorithm(source.Algorithm);
        if (source.Algorithms is { } algorithms)
        {
            builder.WithAlgorithms(algorithms.ToArray());
        }
        foreach (var token in source.Tokens)
        {
            if (token.Key != name)
            {
                builder.WithToken(token.Key, token.Value);
            }
        }
        if (value is not null)
        {
            builder.WithToken(name, value);
        }
        return builder.Build();
    }

    private static Dictionary<string, string> Flatten(string json)
    {
        using var document = JsonDocument.Parse(json);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        Visit(document.RootElement, "$", values);
        return values;
    }

    private static void Visit(JsonElement element, string path, Dictionary<string, string> values)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (!element.EnumerateObject().Any())
            {
                values[path] = "{}";
            }
            foreach (var item in element.EnumerateObject())
            {
                Visit(item.Value, $"{path}.{item.Name}", values);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            var index = 0;
            foreach (var item in element.EnumerateArray())
            {
                Visit(item, $"{path}[{index++}]", values);
            }
            if (index == 0)
            {
                values[path] = "[]";
            }
        }
        else
        {
            values[path] = element.GetRawText();
        }
    }
}
