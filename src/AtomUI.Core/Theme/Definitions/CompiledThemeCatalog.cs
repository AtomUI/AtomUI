using System.Buffers;
using System.Security.Cryptography;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Schema;
using Avalonia.Media;

namespace AtomUI.Theme.Definitions;

internal sealed class CompiledThemeCatalog
{
    private readonly Dictionary<string, CompiledThemeDefinition> _definitions;
    private readonly IReadOnlyList<ThemeResolverCatalogSlice> _resolverSlices;

    private CompiledThemeCatalog(
        Dictionary<string, CompiledThemeDefinition> definitions,
        IReadOnlyList<CompiledThemeDefinition> orderedDefinitions,
        CompiledThemeDefinition defaultDefinition,
        IReadOnlyList<ThemeResolverCatalogSlice> resolverSlices)
    {
        _definitions   = definitions;
        DefaultDefinition = defaultDefinition;
        OrderedDefinitions = Array.AsReadOnly(orderedDefinitions.ToArray());
        AvailableThemes = Array.AsReadOnly(
            orderedDefinitions.Select(static entry => entry.Info).ToArray());
        _resolverSlices = Array.AsReadOnly(resolverSlices.ToArray());
    }

    internal CompiledThemeDefinition DefaultDefinition { get; }
    internal IReadOnlyList<CompiledThemeDefinition> OrderedDefinitions { get; }
    internal IReadOnlyList<ThemeInfo> AvailableThemes { get; }
    internal IReadOnlyList<ThemeResolverCatalogSlice> ResolverSlices => _resolverSlices;

    internal CompiledThemeDefinition Get(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _definitions.TryGetValue(id, out var definition)
            ? definition
            : throw new ThemeLoadException($"Theme '{id}' was not found in the compiled catalog.");
    }

    internal bool Contains(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _definitions.ContainsKey(id);
    }

    internal bool ContentEquals(CompiledThemeCatalog other)
    {
        ArgumentNullException.ThrowIfNull(other);
        if (OrderedDefinitions.Count != other.OrderedDefinitions.Count)
        {
            return false;
        }

        for (var index = 0; index < OrderedDefinitions.Count; index++)
        {
            var left = OrderedDefinitions[index];
            var right = other.OrderedDefinitions[index];
            if (left.Info != right.Info || left.Revision != right.Revision)
            {
                return false;
            }
        }
        return true;
    }

    internal static ThemeCatalogLoadResult LoadInitial(
        ThemeSchemaRegistry registry, IReadOnlyList<IThemeDefinitionResolver> resolvers,
        ThemeDefinitionResolveContext context, ThemeDefinitionLoadCache? loadCache = null) =>
        LoadInitial(registry, resolvers.Select(static resolver => new ThemeDefinitionRegistration(resolver)).ToArray(), context, loadCache);

    internal static ThemeCatalogLoadResult LoadReload(
        ThemeSchemaRegistry registry, CompiledThemeCatalog currentCatalog, IReadOnlyList<IThemeDefinitionResolver> resolvers,
        ThemeDefinitionResolveContext context, ThemeDefinitionLoadCache? loadCache = null) =>
        LoadReload(registry, currentCatalog, resolvers.Select(static resolver => new ThemeDefinitionRegistration(resolver)).ToArray(), context, loadCache);

    internal static ThemeCatalogLoadResult LoadInitial(
        ThemeSchemaRegistry registry,
        IReadOnlyList<ThemeDefinitionRegistration> resolvers,
        ThemeDefinitionResolveContext context,
        ThemeDefinitionLoadCache? loadCache = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(resolvers);
        ArgumentNullException.ThrowIfNull(context);

        var slices = new List<ThemeResolverCatalogSlice>(resolvers.Count);
        var diagnostics = new List<ThemeDiagnostic>();
        var definitions = new Dictionary<string, CompiledThemeDefinition>(StringComparer.Ordinal);
        var orderedDefinitions = new List<CompiledThemeDefinition>();

        foreach (var resolver in resolvers)
        {
            var sliceResult = LoadResolverSlice(registry, resolver, context, loadCache);
            if (!sliceResult.Success)
            {
                if (resolver.IsUserDirectory)
                {
                    diagnostics.AddRange(sliceResult.Diagnostics);
                    continue;
                }

                return ThemeCatalogLoadResult.Failed(
                    diagnostics.Concat(sliceResult.Diagnostics).ToArray(),
                    sliceResult.Exception);
            }

            var slice = sliceResult.Slice!;
            diagnostics.AddRange(sliceResult.Diagnostics);
            ThemeDiagnostic? conflict = null;
            foreach (var entry in slice.Definitions)
            {
                if (definitions.ContainsKey(entry.Info.Id))
                {
                    conflict = Error(
                        "ATMTHM4005",
                        entry.Revision.SourceIdentity,
                        "$",
                        $"Theme id '{entry.Info.Id}' is duplicated across theme definition resolvers.");
                    break;
                }
            }

            if (conflict is not null)
            {
                if (resolver.IsUserDirectory)
                {
                    diagnostics.Add(conflict);
                    continue;
                }

                return ThemeCatalogLoadResult.Failed(
                    diagnostics.Append(conflict).ToArray());
            }

            slices.Add(slice);
            foreach (var entry in slice.Definitions)
            {
                definitions.Add(entry.Info.Id, entry);
                orderedDefinitions.Add(entry);
            }
        }

        var defaults = orderedDefinitions.Where(static entry => entry.Info.IsDefault).ToArray();
        if (defaults.Length != 1)
        {
            return ThemeCatalogLoadResult.Failed(
                diagnostics.Append(Error(
                    "ATMTHM4006",
                    nameof(CompiledThemeCatalog),
                    "$",
                    $"The merged theme catalog must contain exactly one default theme; found {defaults.Length}.")).ToArray());
        }

        return ThemeCatalogLoadResult.Succeeded(
            new CompiledThemeCatalog(
                definitions,
                orderedDefinitions,
                defaults[0],
                slices),
            diagnostics);
    }

    internal static ThemeCatalogLoadResult LoadReload(
        ThemeSchemaRegistry registry,
        CompiledThemeCatalog currentCatalog,
        IReadOnlyList<ThemeDefinitionRegistration> resolvers,
        ThemeDefinitionResolveContext context,
        ThemeDefinitionLoadCache? loadCache = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(currentCatalog);
        ArgumentNullException.ThrowIfNull(resolvers);
        ArgumentNullException.ThrowIfNull(context);

        var currentSlices = currentCatalog.ResolverSlices.ToDictionary(
            static slice => slice.ResolverId,
            StringComparer.Ordinal);
        var nextSlices = new List<ThemeResolverCatalogSlice>(resolvers.Count);
        var diagnostics = new List<ThemeDiagnostic>();
        foreach (var resolver in resolvers)
        {
            if (!resolver.SupportsReload)
            {
                if (!currentSlices.TryGetValue(resolver.Id, out var staticSlice))
                {
                    diagnostics.Add(Error(
                        "ATMTHM4008",
                        resolver.Id,
                        "$",
                        $"Static resolver '{resolver.Id}' is missing from the committed catalog."));
                    return ThemeCatalogLoadResult.Failed(diagnostics);
                }
                diagnostics.AddRange(staticSlice.Diagnostics);
                nextSlices.Add(staticSlice);
                continue;
            }

            var sliceResult = LoadResolverSlice(registry, resolver, context, loadCache);
            diagnostics.AddRange(sliceResult.Diagnostics);
            if (!sliceResult.Success)
            {
                return ThemeCatalogLoadResult.Failed(diagnostics, sliceResult.Exception);
            }
            nextSlices.Add(sliceResult.Slice!);
        }

        return BuildFromSlices(nextSlices, diagnostics);
    }

    private static ThemeCatalogLoadResult BuildFromSlices(
        IReadOnlyList<ThemeResolverCatalogSlice> slices,
        IReadOnlyList<ThemeDiagnostic> existingDiagnostics)
    {
        var diagnostics = new List<ThemeDiagnostic>(existingDiagnostics);
        var definitions = new Dictionary<string, CompiledThemeDefinition>(StringComparer.Ordinal);
        var orderedDefinitions = new List<CompiledThemeDefinition>();
        foreach (var slice in slices)
        {
            foreach (var entry in slice.Definitions)
            {
                if (!definitions.TryAdd(entry.Info.Id, entry))
                {
                    diagnostics.Add(Error(
                        "ATMTHM4005",
                        entry.Revision.SourceIdentity,
                        "$",
                        $"Theme id '{entry.Info.Id}' is duplicated across theme definition resolvers."));
                    return ThemeCatalogLoadResult.Failed(diagnostics);
                }
                orderedDefinitions.Add(entry);
            }
        }

        var defaults = orderedDefinitions.Where(static entry => entry.Info.IsDefault).ToArray();
        if (defaults.Length != 1)
        {
            diagnostics.Add(Error(
                "ATMTHM4006",
                nameof(CompiledThemeCatalog),
                "$",
                $"The merged theme catalog must contain exactly one default theme; found {defaults.Length}."));
            return ThemeCatalogLoadResult.Failed(diagnostics);
        }

        return ThemeCatalogLoadResult.Succeeded(
            new CompiledThemeCatalog(
                definitions,
                orderedDefinitions,
                defaults[0],
                slices),
            diagnostics);
    }

    private static ThemeResolverSliceLoadResult LoadResolverSlice(
        ThemeSchemaRegistry registry,
        ThemeDefinitionRegistration resolver,
        ThemeDefinitionResolveContext context,
        ThemeDefinitionLoadCache? loadCache)
    {
        if (resolver.Package is { } package)
        {
            var entries = new List<CompiledThemeDefinition>(package.Programs.Count);
            foreach (var program in package.Programs)
            {
                try
                {
                    var definition = program.Bind(registry);
                    entries.Add(new CompiledThemeDefinition(program.Info, definition,
                        new ThemeDefinitionRevision(program.SourceIdentity, program.SourceRevision,
                            program.ContentDigest, program.ProgramDigest)));
                }
                catch (Exception exception) when (exception is not OutOfMemoryException)
                {
                    return ThemeResolverSliceLoadResult.Failed(
                        [Error("ATMTHM4010", program.SourceIdentity, "/Theme", exception.Message)], exception);
                }
            }
            return ThemeResolverSliceLoadResult.Succeeded(
                new ThemeResolverCatalogSlice(resolver.Id, false, Array.AsReadOnly(entries.ToArray()), Array.Empty<ThemeDiagnostic>()),
                Array.Empty<ThemeDiagnostic>());
        }

        ThemeDefinitionResolveResult resolved;
        try
        {
            resolved = resolver.Resolver!.Resolve(context) ??
                       throw new InvalidOperationException(
                           $"Theme definition resolver '{resolver.Id}' returned null.");
        }
        catch (Exception exception)
        {
            return ThemeResolverSliceLoadResult.Failed(
                [Error(
                    "ATMTHM4001",
                    resolver.Id,
                    "$",
                    $"Theme definition resolver '{resolver.Id}' failed: " +
                    exception.GetBaseException().Message)],
                exception);
        }

        if (!resolved.Success)
        {
            return ThemeResolverSliceLoadResult.Failed(resolved.Diagnostics);
        }

        var diagnostics = new List<ThemeDiagnostic>(resolved.Diagnostics);
        var definitions = new List<CompiledThemeDefinition>(resolved.Sources.Count);
        var identities = new HashSet<string>(StringComparer.Ordinal);
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in resolved.Sources)
        {
            var entryResult = LoadSource(registry, source, loadCache);
            diagnostics.AddRange(entryResult.Diagnostics);
            if (!entryResult.Success)
            {
                return ThemeResolverSliceLoadResult.Failed(diagnostics, entryResult.Exception);
            }

            var entry = entryResult.Definition!;
            if (!identities.Add(entry.Revision.SourceIdentity))
            {
                diagnostics.Add(Error(
                    "ATMTHM4004",
                    entry.Revision.SourceIdentity,
                    "$",
                    $"Source identity '{entry.Revision.SourceIdentity}' is duplicated in resolver '{resolver.Id}'."));
                return ThemeResolverSliceLoadResult.Failed(diagnostics);
            }
            if (!ids.Add(entry.Info.Id))
            {
                diagnostics.Add(Error(
                    "ATMTHM4005",
                    entry.Revision.SourceIdentity,
                    "$",
                    $"Theme id '{entry.Info.Id}' is duplicated in resolver '{resolver.Id}'."));
                return ThemeResolverSliceLoadResult.Failed(diagnostics);
            }
            if (resolver.SupportsReload && entry.Info.IsDefault)
            {
                diagnostics.Add(Error(
                    "ATMTHM4007",
                    entry.Revision.SourceIdentity,
                    "$",
                    $"Reloadable resolver '{resolver.Id}' cannot declare default theme '{entry.Info.Id}'."));
                return ThemeResolverSliceLoadResult.Failed(diagnostics);
            }
            definitions.Add(entry);
        }

        return ThemeResolverSliceLoadResult.Succeeded(
            new ThemeResolverCatalogSlice(
                resolver.Id,
                resolver.SupportsReload,
                Array.AsReadOnly(definitions.ToArray()),
                Array.AsReadOnly(diagnostics.ToArray())),
            diagnostics);
    }

    private static ThemeSourceLoadResult LoadSource(
        ThemeSchemaRegistry registry,
        IThemeDefinitionSource source,
        ThemeDefinitionLoadCache? loadCache)
    {
        string identity;
        string revision;
        try
        {
            identity = source.SourceIdentity;
            revision = source.SourceRevision;
            ArgumentException.ThrowIfNullOrWhiteSpace(identity);
            ArgumentException.ThrowIfNullOrWhiteSpace(revision);
        }
        catch (Exception exception)
        {
            return ThemeSourceLoadResult.Failed(
                [Error(
                    "ATMTHM4002",
                    source.GetType().FullName ?? source.GetType().Name,
                    "$",
                    "Theme definition source identity and revision must be non-empty.")],
                exception);
        }

        // Source revisions are only candidate metadata.  Reloadable sources must be
        // read again so same-size/timestamp edits cannot reuse stale documents.
        ThemeDocument document;
        string contentDigest;
        var diagnostics = new List<ThemeDiagnostic>();
        byte[] bytes;
        try
        {
            using var stream = source.OpenRead() ??
                               throw new InvalidOperationException(
                                   $"Theme definition source '{identity}' returned a null stream.");
            bytes = ReadBounded(stream, ThemeDocumentReaderOptions.DefaultMaxDocumentBytes);
        }
        catch (Exception exception)
        {
            return ThemeSourceLoadResult.Failed(
                [Error(
                    "ATMTHM4003",
                    identity,
                    "$",
                    $"Theme definition source '{identity}' could not be read: " +
                    exception.GetBaseException().Message)],
                exception);
        }

        contentDigest = Convert.ToHexString(SHA256.HashData(bytes));
        var sourceKey = new ThemeSourceCacheKey(identity, contentDigest);
        if (loadCache?.TryGetRead(sourceKey, out var cachedRead) == true)
        {
            document = cachedRead!.Document;
            diagnostics.AddRange(cachedRead.Diagnostics);
        }
        else
        {
            using var input = new MemoryStream(bytes, writable: false);
            var read = ThemeDocumentReader.Read(input, identity);
            diagnostics.AddRange(ConvertDiagnostics(read.Diagnostics));
            if (!read.Success)
            {
                return ThemeSourceLoadResult.Failed(diagnostics);
            }

            document = read.Document!;
            loadCache?.StoreRead(
                sourceKey,
                new ThemeSourceReadCacheEntry(document, diagnostics.ToArray()));
        }

        PreparedThemeDefinition definition;
        var bindingKey = new ThemeBindingCacheKey(identity, contentDigest, registry.Revision, registry.BindingIdentity);
        if (loadCache?.TryGetBinding(bindingKey, out var cachedBinding) == true)
        {
            definition = cachedBinding!.Definition;
            diagnostics.AddRange(cachedBinding.Diagnostics);
        }
        else
        {
            var bound = ThemeDefinitionBinder.Bind(document, registry);
            var bindingDiagnostics = ConvertDiagnostics(bound.Diagnostics);
            diagnostics.AddRange(bindingDiagnostics);
            if (!bound.Success)
            {
                return ThemeSourceLoadResult.Failed(diagnostics);
            }

            definition = bound.Definition!;
            loadCache?.StoreBinding(
                bindingKey,
                new ThemeBindingCacheEntry(definition, bindingDiagnostics));
        }

        var accentColor = definition.Tokens
                                    .FirstOrDefault(static token =>
                                         token.Descriptor.Name == nameof(DesignToken.ColorPrimary))
                                    ?.Value is Color color
            ? color
            : (Color?)null;
        var definitionRevision = new ThemeDefinitionRevision(
            identity,
            revision,
            contentDigest);
        return ThemeSourceLoadResult.Succeeded(
            new CompiledThemeDefinition(
                new ThemeInfo(
                    definition.Id,
                    definition.Name,
                    definition.EffectiveAppearance,
                    definition.IsDefault,
                    accentColor),
                definition,
                definitionRevision),
            diagnostics);
    }

    private static byte[] ReadBounded(Stream stream, long maxBytes)
    {
        using var output = new MemoryStream();
        // Do not query Length/CanSeek: external streams need only support Read to EOF.
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        try
        {
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    return output.ToArray();
                }
                if (output.Length + read > maxBytes)
                {
                    throw new InvalidDataException(
                        $"Theme definition exceeds the {maxBytes}-byte input limit.");
                }
                output.Write(buffer, 0, read);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    internal static IReadOnlyList<ThemeDiagnostic> ConvertDiagnostics(
        IReadOnlyList<ThemeDefinitionDiagnostic> diagnostics)
    {
        return Array.AsReadOnly(diagnostics.Select(static diagnostic => new ThemeDiagnostic(
            diagnostic.Code,
            diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Error
                ? ThemeDiagnosticSeverity.Error
                : ThemeDiagnosticSeverity.Warning,
            diagnostic.FilePath,
            diagnostic.Path,
            diagnostic.Message)).ToArray());
    }

    internal static string Describe(IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        return string.Join(
            Environment.NewLine,
            diagnostics.Select(static diagnostic =>
                $"{diagnostic.Code} {diagnostic.Source}{diagnostic.Path}: {diagnostic.Message}"));
    }

    private static ThemeDiagnostic Error(
        string code,
        string source,
        string path,
        string message)
    {
        return new ThemeDiagnostic(code, ThemeDiagnosticSeverity.Error, source, path, message);
    }
}

internal sealed record CompiledThemeDefinition(
    ThemeInfo Info,
    PreparedThemeDefinition Definition,
    ThemeDefinitionRevision Revision);

internal sealed record ThemeResolverCatalogSlice(
    string ResolverId,
    bool SupportsReload,
    IReadOnlyList<CompiledThemeDefinition> Definitions,
    IReadOnlyList<ThemeDiagnostic> Diagnostics);

internal sealed class ThemeCatalogLoadResult
{
    private ThemeCatalogLoadResult(
        CompiledThemeCatalog? catalog,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        Catalog     = catalog;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception   = exception;
    }

    internal CompiledThemeCatalog? Catalog { get; }
    internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    internal Exception? Exception { get; }
    internal bool Success => Catalog is not null && Exception is null;

    internal static ThemeCatalogLoadResult Succeeded(
        CompiledThemeCatalog catalog,
        IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        return new ThemeCatalogLoadResult(catalog, diagnostics, null);
    }

    internal static ThemeCatalogLoadResult Failed(
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception = null)
    {
        return new ThemeCatalogLoadResult(null, diagnostics, exception);
    }
}

internal sealed class ThemeResolverSliceLoadResult
{
    private ThemeResolverSliceLoadResult(
        ThemeResolverCatalogSlice? slice,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        Slice       = slice;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception   = exception;
    }

    internal ThemeResolverCatalogSlice? Slice { get; }
    internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    internal Exception? Exception { get; }
    internal bool Success => Slice is not null && Exception is null &&
                             Diagnostics.All(static diagnostic =>
                                 diagnostic.Severity != ThemeDiagnosticSeverity.Error);

    internal static ThemeResolverSliceLoadResult Succeeded(
        ThemeResolverCatalogSlice slice,
        IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        return new ThemeResolverSliceLoadResult(slice, diagnostics, null);
    }

    internal static ThemeResolverSliceLoadResult Failed(
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception = null)
    {
        return new ThemeResolverSliceLoadResult(null, diagnostics, exception);
    }
}

internal sealed class ThemeSourceLoadResult
{
    private ThemeSourceLoadResult(
        CompiledThemeDefinition? definition,
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception)
    {
        Definition  = definition;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
        Exception   = exception;
    }

    internal CompiledThemeDefinition? Definition { get; }
    internal IReadOnlyList<ThemeDiagnostic> Diagnostics { get; }
    internal Exception? Exception { get; }
    internal bool Success => Definition is not null && Exception is null &&
                             Diagnostics.All(static diagnostic =>
                                 diagnostic.Severity != ThemeDiagnosticSeverity.Error);

    internal static ThemeSourceLoadResult Succeeded(
        CompiledThemeDefinition definition,
        IReadOnlyList<ThemeDiagnostic> diagnostics)
    {
        return new ThemeSourceLoadResult(definition, diagnostics, null);
    }

    internal static ThemeSourceLoadResult Failed(
        IReadOnlyList<ThemeDiagnostic> diagnostics,
        Exception? exception = null)
    {
        return new ThemeSourceLoadResult(null, diagnostics, exception);
    }
}
