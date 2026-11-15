using System.Xml;
using System.Xml.Schema;
using AtomUI.Theme.Algorithms;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;

namespace AtomUI.Theme.Definitions;

internal static class ThemeDocumentReader
{
    internal const string XmlNamespace = "https://atomui.net/schemas/theme/v1";

    private const string MalformedXmlCode = "ATMTHM1001";
    private const string SchemaValidationCode = "ATMTHM1002";
    private const string ResourceLimitCode = "ATMTHM1003";
    private const string XsdResourceName = "AtomUI.Theme.Definitions.Schemas.atomui-theme-v1.xsd";

    private static readonly Lazy<XmlSchemaSet> s_schemaSet = new(CreateSchemaSet);

    internal static ThemeDocumentReadResult Read(
        Stream stream,
        string source,
        ThemeDocumentReaderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        options ??= new ThemeDocumentReaderOptions();
        ValidateOptions(options);

        if (stream.CanSeek && stream.Length - stream.Position > options.MaxDocumentBytes)
        {
            return LimitFailure(source, $"Theme document exceeds the {options.MaxDocumentBytes} byte limit.");
        }

        var diagnostics = new List<ThemeDefinitionDiagnostic>();
        var builder = new ThemeDocumentBuilder(source, diagnostics);
        var currentPath = "/";
        var elementCount = 0;

        try
        {
            using var limitedStream = new LimitedReadStream(stream, options.MaxDocumentBytes);
            var settings = CreateReaderSettings(diagnostics, source, () => currentPath);
            using var reader = XmlReader.Create(limitedStream, settings, source);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    elementCount++;
                    if (elementCount > options.MaxElements)
                    {
                        throw new ThemeDocumentLimitException(
                            $"Theme document exceeds the {options.MaxElements} element limit.");
                    }

                    currentPath = builder.ReadElement(reader);
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    currentPath = builder.ReadEndElement(reader);
                }
            }
        }
        catch (ThemeDocumentLimitException exception)
        {
            return LimitFailure(source, exception.Message);
        }
        catch (XmlException exception)
        {
            return Failure(
                source,
                MalformedXmlCode,
                exception.LineNumber,
                exception.LinePosition,
                "/",
                "The theme definition is not well-formed XML.");
        }

        if (diagnostics.Any(static diagnostic =>
                diagnostic.Severity == ThemeDefinitionDiagnosticSeverity.Error))
        {
            return new ThemeDocumentReadResult(null, diagnostics);
        }

        var document = builder.Build();
        if (document is null)
        {
            diagnostics.Add(new ThemeDefinitionDiagnostic(
                                MalformedXmlCode,
                                ThemeDefinitionDiagnosticSeverity.Error,
                                source,
                                0,
                                0,
                                "/",
                                "The theme definition does not contain a Theme root."));
        }

        return new ThemeDocumentReadResult(document, diagnostics);
    }

    private static XmlReaderSettings CreateReaderSettings(
        List<ThemeDefinitionDiagnostic> diagnostics,
        string source,
        Func<string> pathProvider)
    {
        var settings = new XmlReaderSettings
        {
            CloseInput       = false,
            DtdProcessing    = DtdProcessing.Prohibit,
            XmlResolver      = null,
            ValidationType   = ValidationType.Schema,
            Schemas          = s_schemaSet.Value,
            ValidationFlags  = XmlSchemaValidationFlags.ProcessIdentityConstraints |
                               XmlSchemaValidationFlags.ReportValidationWarnings
        };
        settings.ValidationEventHandler += (_, args) =>
        {
            var exception = args.Exception;
            diagnostics.Add(new ThemeDefinitionDiagnostic(
                                SchemaValidationCode,
                                args.Severity == XmlSeverityType.Warning
                                    ? ThemeDefinitionDiagnosticSeverity.Warning
                                    : ThemeDefinitionDiagnosticSeverity.Error,
                                source,
                                exception?.LineNumber ?? 0,
                                exception?.LinePosition ?? 0,
                                pathProvider(),
                                args.Message));
        };
        return settings;
    }

    private static XmlSchemaSet CreateSchemaSet()
    {
        var assembly = typeof(ThemeDocumentReader).Assembly;
        using var stream = assembly.GetManifestResourceStream(XsdResourceName) ??
                           throw new InvalidOperationException(
                               $"Embedded theme XML schema '{XsdResourceName}' was not found.");
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver   = null
        });
        var schemas = new XmlSchemaSet
        {
            XmlResolver = null
        };
        schemas.Add(XmlNamespace, reader);
        schemas.Compile();
        return schemas;
    }

    private static void ValidateOptions(ThemeDocumentReaderOptions options)
    {
        if (options.MaxDocumentBytes <= 0 ||
            options.MaxDocumentBytes > ThemeDocumentReaderOptions.DefaultMaxDocumentBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxDocumentBytes));
        }

        if (options.MaxElements <= 0 ||
            options.MaxElements > ThemeDocumentReaderOptions.DefaultMaxElements)
        {
            throw new ArgumentOutOfRangeException(nameof(options.MaxElements));
        }
    }

    private static ThemeDocumentReadResult LimitFailure(string source, string message)
    {
        return Failure(source, ResourceLimitCode, 0, 0, "/", message);
    }

    private static ThemeDocumentReadResult Failure(
        string source,
        string code,
        int line,
        int column,
        string path,
        string message)
    {
        return new ThemeDocumentReadResult(
            null,
            [new ThemeDefinitionDiagnostic(
                code,
                ThemeDefinitionDiagnosticSeverity.Error,
                source,
                line,
                column,
                path,
                message)]);
    }

    private sealed class ThemeDocumentBuilder
    {
        private readonly string _source;
        private readonly List<ThemeDefinitionDiagnostic> _diagnostics;
        private string? _id;
        private string? _name;
        private ThemeAppearance _appearance;
        private bool _isDefault;
        private string? _designLanguage;
        private ThemeDensity _density;
        private ThemeBrandColorSupport _brandColor;
        private ThemeSourceLocation _location;
        private readonly List<ThemeAlgorithmDocument> _algorithms = new();
        private readonly HashSet<ThemeAlgorithm> _algorithmIdentities = new();
        private readonly List<ThemeTokenDocument> _tokens = new();
        private readonly List<ControlThemeDocument> _controls = new();
        private ControlBuilder? _currentControl;

        internal ThemeDocumentBuilder(
            string source,
            List<ThemeDefinitionDiagnostic> diagnostics)
        {
            _source      = source;
            _diagnostics = diagnostics;
        }

        internal string ReadElement(XmlReader reader)
        {
            if (!string.Equals(reader.NamespaceURI, XmlNamespace, StringComparison.Ordinal))
            {
                return "/";
            }

            return reader.LocalName switch
            {
                "Theme"      => ReadTheme(reader),
                "Algorithms" => ReadAlgorithms(reader),
                "Algorithm"  => ReadAlgorithm(reader),
                "Tokens"     => CurrentContainerPath("Tokens"),
                "Token"      => ReadToken(reader),
                "Controls"   => "/Theme/Controls",
                "Control"    => ReadControl(reader),
                _            => "/Theme"
            };
        }

        internal string ReadEndElement(XmlReader reader)
        {
            if (string.Equals(reader.NamespaceURI, XmlNamespace, StringComparison.Ordinal) &&
                string.Equals(reader.LocalName, "Control", StringComparison.Ordinal))
            {
                CompleteCurrentControl();
                return "/Theme/Controls";
            }

            return _currentControl?.Path ?? "/Theme";
        }

        internal ThemeDocument? Build()
        {
            if (_id is null || _name is null)
            {
                return null;
            }

            CompleteCurrentControl();
            return new ThemeDocument(
                _id,
                _name,
                _appearance,
                _isDefault,
                _algorithms,
                _tokens,
                _controls,
                _location,
                _designLanguage,
                _density,
                _brandColor);
        }

        private string ReadTheme(XmlReader reader)
        {
            _id         = reader.GetAttribute("Id");
            _name       = reader.GetAttribute("Name");
            _appearance = string.Equals(reader.GetAttribute("Appearance"), "Dark", StringComparison.Ordinal)
                ? ThemeAppearance.Dark
                : ThemeAppearance.Light;
            _isDefault = string.Equals(reader.GetAttribute("IsDefault"), "true", StringComparison.Ordinal);
            _location  = Location(reader, "/Theme");
            _designLanguage = reader.GetAttribute("DesignLanguage");
            _density = string.Equals(reader.GetAttribute("Density"), "Compact", StringComparison.Ordinal)
                ? ThemeDensity.Compact
                : ThemeDensity.Normal;
            _brandColor = string.Equals(reader.GetAttribute("BrandColor"), "Seed", StringComparison.Ordinal)
                ? ThemeBrandColorSupport.Seed
                : ThemeBrandColorSupport.Fixed;
            return "/Theme";
        }

        private string ReadAlgorithms(XmlReader reader)
        {
            if (_currentControl is not null)
            {
                _currentControl.HasCustomAlgorithms = true;
                if (_currentControl.AlgorithmMode == ControlAlgorithmMode.Unspecified)
                {
                    _currentControl.AlgorithmMode = ControlAlgorithmMode.Custom;
                }
            }

            return CurrentContainerPath("Algorithms");
        }

        private string ReadAlgorithm(XmlReader reader)
        {
            var declaredCatalog = reader.GetAttribute("Catalog");
            var catalog = declaredCatalog ?? ThemeAlgorithm.BuiltInCatalog;
            var id = reader.GetAttribute("Id") ?? string.Empty;
            var path = declaredCatalog is null
                ? $"{CurrentContainerPath("Algorithms")}/Algorithm[@Id='{id}']"
                : $"{CurrentContainerPath("Algorithms")}/Algorithm[@Catalog='{declaredCatalog}'][@Id='{id}']";
            var location = Location(reader, path);
            if (!SchemaIdentifier.IsValid(catalog) || !SchemaIdentifier.IsValid(id))
            {
                // The XSD normally reports this first; keep one diagnostic per element.
                if (!_diagnostics.Any(diagnostic =>
                        diagnostic.Code == SchemaValidationCode &&
                        diagnostic.Line == location.Line))
                {
                    AddSchemaError(location, $"Theme algorithm identity '{catalog}:{id}' is not a valid identifier.");
                }
                return path;
            }

            var algorithm = new ThemeAlgorithm(catalog, id);
            var seen = _currentControl?.AlgorithmIdentities ?? _algorithmIdentities;
            if (!seen.Add(algorithm))
            {
                // xs:unique ignores an omitted Catalog, so the reader owns duplicate detection.
                AddSchemaError(location, $"Theme algorithm '{algorithm}' is duplicated.");
                return path;
            }

            var document = new ThemeAlgorithmDocument(algorithm, location);
            if (_currentControl is null)
            {
                _algorithms.Add(document);
            }
            else
            {
                _currentControl.Algorithms.Add(document);
            }

            return path;
        }

        private void AddSchemaError(ThemeSourceLocation location, string message)
        {
            _diagnostics.Add(new ThemeDefinitionDiagnostic(
                SchemaValidationCode,
                ThemeDefinitionDiagnosticSeverity.Error,
                location.Source,
                location.Line,
                location.Column,
                location.Path,
                message));
        }

        private string ReadToken(XmlReader reader)
        {
            var name = reader.GetAttribute("Name") ?? string.Empty;
            var value = reader.GetAttribute("Value") ?? string.Empty;
            var path = $"{CurrentContainerPath("Tokens")}/Token[@Name='{name}']";
            var document = new ThemeTokenDocument(name, value, Location(reader, path));
            if (_currentControl is null)
            {
                _tokens.Add(document);
            }
            else
            {
                _currentControl.Tokens.Add(document);
            }

            return path;
        }

        private string ReadControl(XmlReader reader)
        {
            CompleteCurrentControl();
            var catalog = reader.GetAttribute("Catalog") ?? string.Empty;
            var id = reader.GetAttribute("Id") ?? string.Empty;
            var path = $"/Theme/Controls/Control[@Catalog='{catalog}'][@Id='{id}']";
            _currentControl = new ControlBuilder(
                new ThemeControlDocumentIdentity(catalog, id),
                ParseAlgorithmMode(reader.GetAttribute("Algorithm")),
                Location(reader, path),
                path);
            if (reader.IsEmptyElement)
            {
                CompleteCurrentControl();
            }

            return path;
        }

        private string CurrentContainerPath(string container)
        {
            return _currentControl is null
                ? $"/Theme/{container}"
                : $"{_currentControl.Path}/{container}";
        }

        private void CompleteCurrentControl()
        {
            if (_currentControl is null)
            {
                return;
            }

            _controls.Add(_currentControl.Build());
            _currentControl = null;
        }

        private ThemeSourceLocation Location(XmlReader reader, string path)
        {
            var lineInfo = (IXmlLineInfo)reader;
            return new ThemeSourceLocation(
                _source,
                lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
                lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0,
                path);
        }

        private static ControlAlgorithmMode ParseAlgorithmMode(string? value)
        {
            return value switch
            {
                "Disabled" => ControlAlgorithmMode.Disabled,
                "Global"   => ControlAlgorithmMode.Global,
                _          => ControlAlgorithmMode.Unspecified
            };
        }

        private sealed class ControlBuilder
        {
            internal ControlBuilder(
                ThemeControlDocumentIdentity identity,
                ControlAlgorithmMode algorithmMode,
                ThemeSourceLocation location,
                string path)
            {
                Identity      = identity;
                AlgorithmMode = algorithmMode;
                Location      = location;
                Path          = path;
            }

            internal ThemeControlDocumentIdentity Identity { get; }
            internal ControlAlgorithmMode AlgorithmMode { get; set; }
            internal bool HasCustomAlgorithms { get; set; }
            internal ThemeSourceLocation Location { get; }
            internal string Path { get; }
            internal List<ThemeAlgorithmDocument> Algorithms { get; } = new();
            internal HashSet<ThemeAlgorithm> AlgorithmIdentities => field ??= new();
            internal List<ThemeTokenDocument> Tokens { get; } = new();

            internal ControlThemeDocument Build()
            {
                var mode = HasCustomAlgorithms && AlgorithmMode == ControlAlgorithmMode.Unspecified
                    ? ControlAlgorithmMode.Custom
                    : AlgorithmMode;
                return new ControlThemeDocument(Identity, mode, Algorithms, Tokens, Location);
            }
        }
    }

    private sealed class LimitedReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _limit;
        private long _read;

        internal LimitedReadStream(Stream inner, long limit)
        {
            _inner = inner;
            _limit = limit;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _inner.Length;

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            Track(read);
            return read;
        }

        public override int Read(Span<byte> buffer)
        {
            var read = _inner.Read(buffer);
            Track(read);
            return read;
        }

        public override int ReadByte()
        {
            var value = _inner.ReadByte();
            if (value >= 0)
            {
                Track(1);
            }
            return value;
        }

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void Flush() => _inner.Flush();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void Track(int count)
        {
            _read += count;
            if (_read > _limit)
            {
                throw new ThemeDocumentLimitException(
                    $"Theme document exceeds the {_limit} byte limit.");
            }
        }
    }

    private sealed class ThemeDocumentLimitException : Exception
    {
        internal ThemeDocumentLimitException(string message)
            : base(message)
        {
        }
    }
}
