using AtomUI.Theme.Compilation;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Resources;
using AtomUI.Theme.Styling;
using AtomUI.Theme.Branding;

namespace AtomUI.Theme;

internal delegate ValueTask<ThemeTransactionPreparation> ThemePrepareDelegate(
    ThemeRequest request,
    ThemeSnapshot? currentSnapshot,
    CancellationToken cancellationToken);

internal enum ThemeTransactionPhase : byte
{
    Capture,
    Prepare,
    CommitCore,
    Publish,
    Complete
}

internal sealed class ThemeTransactionPreparation
{
    private ThemeTransactionPreparation(
        ThemeSnapshot? snapshot,
        ThemeSnapshotCacheKey snapshotKey,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        Snapshot    = snapshot;
        SnapshotKey = snapshotKey;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception   = exception;
    }

    internal ThemeSnapshot? Snapshot { get; }
    internal ThemeSnapshotCacheKey SnapshotKey { get; }
    internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    internal Exception? Exception { get; }

    internal bool Success => Snapshot is not null && Exception is null &&
                             Diagnostics.All(static diagnostic =>
                                 diagnostic.Severity != ThemeDiagnosticSeverity.Error);

    internal static ThemeTransactionPreparation Succeeded(
        ThemeSnapshot snapshot,
        ThemeSnapshotCacheKey snapshotKey,
        IReadOnlyList<ThemeDiagnostic>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        return new ThemeTransactionPreparation(
            snapshot,
            snapshotKey,
            diagnostics ?? Array.Empty<ThemeDiagnostic>(),
            null);
    }

    internal static ThemeTransactionPreparation Failed(
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception = null)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        return new ThemeTransactionPreparation(
            null,
            default,
            diagnostics,
            exception);
    }
}

internal sealed class ThemeTransaction
{
    private readonly TaskCompletionSource<ThemeTransitionResult> _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal ThemeTransaction(
        long transitionId,
        long generation,
        ThemeRequest request,
        ThemeRequestCacheKey requestKey)
    {
        TransitionId = transitionId;
        Generation   = generation;
        Request      = request;
        RequestKey   = requestKey;
        Phase        = ThemeTransactionPhase.Capture;
    }

    internal long TransitionId { get; }
    internal long Generation { get; }
    internal ThemeRequest Request { get; }
    internal ThemeRequestCacheKey RequestKey { get; }
    internal ThemeTransactionPhase Phase { get; set; }
    internal ThemeTransactionPreparation? Preparation { get; set; }
    internal ThemeState? PreparedState { get; set; }
    internal ThemeTokenResourceProvider? PreparedResourceProvider { get; set; }
    internal ThemeContext? PreparedRootContext { get; set; }
    internal ThemeScopeCapture? ScopeCapture { get; set; }
    internal ThemeStyleBatch? StyleBatch { get; set; }
    internal IReadOnlyList<ThemeScopeStagedSnapshot> PreparedScopes { get; set; } =
        Array.Empty<ThemeScopeStagedSnapshot>();
    internal bool AddsResourceProvider { get; set; }
    internal Task<ThemeTransitionResult> Completion => _completion.Task;

    internal void Complete(ThemeTransitionResult result)
    {
        Phase = ThemeTransactionPhase.Complete;
        _completion.TrySetResult(result);
    }
}

internal sealed record ThemeScopeStagedSnapshot(
    ThemeScopeStamp Stamp,
    ThemeContext Context,
    ThemeSnapshot Snapshot);

internal sealed class ThemeScopeUpdateResult
{
    private ThemeScopeUpdateResult(
        long transitionId,
        ThemeTransitionStatus status,
        ThemeRequest request,
        ThemeState state,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics,
        Exception? exception)
    {
        TransitionId       = transitionId;
        Status             = status;
        Request            = request;
        State              = state;
        Diagnostics        = diagnostics;
        PublishDiagnostics = publishDiagnostics;
        Exception          = exception;
    }

    internal long TransitionId { get; }
    internal ThemeTransitionStatus Status { get; }
    internal bool Success => Status == ThemeTransitionStatus.Committed;
    internal ThemePresentationResult Presentation { get; set; } = ThemePresentationResult.Unchanged;
    internal ThemeRequest Request { get; }
    internal ThemeState State { get; }
    internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    internal IReadOnlyList<ThemeDiagnostic> PublishDiagnostics { get; }
    internal Exception? Exception { get; }

    internal ThemeScopeUpdateResult WithPublishDiagnostics(
        IReadOnlyList<ThemeDiagnostic> publishDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(publishDiagnostics);
        if (publishDiagnostics.Count == 0)
        {
            return this;
        }

        return new ThemeScopeUpdateResult(
            TransitionId,
            Status,
            Request,
            State,
            Diagnostics,
            PublishDiagnostics.Concat(publishDiagnostics).ToArray(),
            Exception) { Presentation = this.Presentation };
    }

    internal static ThemeScopeUpdateResult Succeeded(
        long transitionId,
        ThemeRequest request,
        ThemeState state,
        IReadOnlyList<ThemeDiagnostic>? publishDiagnostics = null)
    {
        return new ThemeScopeUpdateResult(
            transitionId,
            ThemeTransitionStatus.Committed,
            request,
            state,
            Array.Empty<ThemeDiagnostic>(),
            publishDiagnostics ?? Array.Empty<ThemeDiagnostic>(),
            null);
    }

    internal static ThemeScopeUpdateResult Failed(
        long transitionId,
        ThemeRequest request,
        ThemeState state,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        return new ThemeScopeUpdateResult(
            transitionId,
            ThemeTransitionStatus.Failed,
            request,
            state,
            diagnostics,
            Array.Empty<ThemeDiagnostic>(),
            exception);
    }

    internal static ThemeScopeUpdateResult NoOp(
        long transitionId,
        ThemeRequest request,
        ThemeState state)
    {
        return new ThemeScopeUpdateResult(
            transitionId,
            ThemeTransitionStatus.NoOp,
            request,
            state,
            Array.Empty<ThemeDiagnostic>(),
            Array.Empty<ThemeDiagnostic>(),
            null);
    }

    internal static ThemeScopeUpdateResult Superseded(
        long transitionId,
        ThemeRequest request,
        ThemeState state)
    {
        return new ThemeScopeUpdateResult(
            transitionId,
            ThemeTransitionStatus.Superseded,
            request,
            state,
            Array.Empty<ThemeDiagnostic>(),
            Array.Empty<ThemeDiagnostic>(),
            null);
    }
}

internal sealed record PendingThemeScopeUpdate(
    ThemeConfigProvider Provider,
    long RegistrationId,
    long ConfigRevision,
    Configuration.ThemeConfig Config);

internal sealed class ThemeRequestCacheKey : IEquatable<ThemeRequestCacheKey>
{
    private readonly ThemeAlgorithm[]? _algorithms;
    private readonly KeyValuePair<string, string>[]? _tokens;
    private readonly ControlRequestCacheKey[]? _controls;
    private readonly int _hashCode;
    private readonly ThemeStyleInputs _styleInputs;
    private readonly ThemeBrandRequirements? _brandRequirements;

    private ThemeRequestCacheKey(ThemeRequest request)
    {
        ThemeId = request.ThemeId;
        Reason  = request.Reason;
        _styleInputs = ThemeStyleInputs.Resolve(ThemeStyleInputs.Empty, request.Config);
        _brandRequirements = request.Config?.BrandRequirements;
        if (request.Config is not null)
        {
            Inherit     = request.Config.Inherit;
            _algorithms = request.Config.Algorithms?.ToArray();
            _tokens     = request.Config.Tokens.OrderBy(static item => item.Key, StringComparer.Ordinal).ToArray();
            _controls   = request.Config.Controls
                                        .OrderBy(static item => item.Key.Catalog, StringComparer.Ordinal)
                                        .ThenBy(static item => item.Key.Id, StringComparer.Ordinal)
                                        .Select(static item => new ControlRequestCacheKey(item.Key, item.Value))
                                        .ToArray();
        }
        _hashCode = ComputeHashCode();
    }

    internal string ThemeId { get; }
    internal ThemeTransitionReason Reason { get; }
    internal bool Inherit { get; }

    internal static ThemeRequestCacheKey Create(ThemeRequest request)
    {
        return new ThemeRequestCacheKey(request);
    }

    public bool Equals(ThemeRequestCacheKey? other)
    {
        return other is not null &&
               string.Equals(ThemeId, other.ThemeId, StringComparison.Ordinal) &&
               Reason == other.Reason &&
               Inherit == other.Inherit &&
               _styleInputs.Equals(other._styleInputs) &&
               Equals(_brandRequirements, other._brandRequirements) &&
               NullableSequenceEqual(_algorithms, other._algorithms) &&
               NullableSequenceEqual(_tokens, other._tokens) &&
               NullableSequenceEqual(_controls, other._controls);
    }

    public override bool Equals(object? obj) => Equals(obj as ThemeRequestCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(ThemeId, StringComparer.Ordinal);
        hash.Add(Reason);
        hash.Add(Inherit);
        hash.Add(_styleInputs);
        hash.Add(_brandRequirements);
        Add(ref hash, _algorithms);
        Add(ref hash, _tokens);
        Add(ref hash, _controls);
        return hash.ToHashCode();
    }

    private static bool NullableSequenceEqual<T>(T[]? left, T[]? right)
    {
        return left is null
            ? right is null
            : right is not null && left.AsSpan().SequenceEqual(right);
    }

    private static void Add<T>(ref HashCode hash, T[]? values)
    {
        if (values is null)
        {
            hash.Add(-1);
            return;
        }
        hash.Add(values.Length);
        foreach (var value in values)
        {
            hash.Add(value);
        }
    }
}

internal sealed class ControlRequestCacheKey : IEquatable<ControlRequestCacheKey>
{
    private readonly ThemeAlgorithm[]? _algorithms;
    private readonly KeyValuePair<string, string>[] _tokens;
    private readonly int _hashCode;

    internal ControlRequestCacheKey(
        Schema.ControlTokenIdentity identity,
        Configuration.ControlThemeConfig config)
    {
        Identity    = identity;
        Algorithm   = config.Algorithm;
        _algorithms = config.Algorithms?.ToArray();
        _tokens     = config.Tokens.OrderBy(static item => item.Key, StringComparer.Ordinal).ToArray();
        _hashCode   = ComputeHashCode();
    }

    internal Schema.ControlTokenIdentity Identity { get; }
    internal Configuration.ControlAlgorithmMode Algorithm { get; }

    public bool Equals(ControlRequestCacheKey? other)
    {
        return other is not null &&
               Identity == other.Identity &&
               Algorithm == other.Algorithm &&
               (_algorithms is null
                   ? other._algorithms is null
                   : other._algorithms is not null && _algorithms.AsSpan().SequenceEqual(other._algorithms)) &&
               _tokens.AsSpan().SequenceEqual(other._tokens);
    }

    public override bool Equals(object? obj) => Equals(obj as ControlRequestCacheKey);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        var hash = new HashCode();
        hash.Add(Identity);
        hash.Add(Algorithm);
        if (_algorithms is null)
        {
            hash.Add(-1);
        }
        else
        {
            hash.Add(_algorithms.Length);
            foreach (var algorithm in _algorithms)
            {
                hash.Add(algorithm);
            }
        }
        foreach (var token in _tokens)
        {
            hash.Add(token);
        }
        return hash.ToHashCode();
    }
}
