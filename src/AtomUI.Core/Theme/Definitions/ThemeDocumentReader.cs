using System.Xml;
using System.Xml.Schema;

namespace AtomUI.Theme.Definitions;

internal static class ThemeDocumentReader
{
    internal const string XmlNamespace = "https://atomui.net/schemas/theme/v2";
    internal const string V1XmlNamespace = "https://atomui.net/schemas/theme/v1";

    /// <summary>
    /// The toolchain command that rewrites v1 theme files as v2.
    /// </summary>
    internal const string ConversionCommand =
        "dotnet msbuild -t:ConvertAtomUIThemeDefinitions -p:AtomUIThemeDefinitionInput=<directory> " +
        "[-p:AtomUIThemeDefinitionOutput=<directory>]";

    private const string MalformedXmlCode = "ATMTHM1001";
    private const string SchemaValidationCode = "ATMTHM1002";
    private const string ResourceLimitCode = "ATMTHM1003";
    private const string UnsupportedVersionCode = "ATMTHM1004";
    private const string XsdResourceName = "AtomUI.Theme.Definitions.Schemas.atomui-theme-v2.xsd";

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
                    if (elementCount == 0 && !string.Equals(reader.NamespaceURI, XmlNamespace, StringComparison.Ordinal))
                    {
                        return UnsupportedRoot(source, reader);
                    }
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

    // A v1 file is not read; the diagnostic names the format and the conversion command.
    private static ThemeDocumentReadResult UnsupportedRoot(string source, XmlReader reader)
    {
        var lineInfo = (IXmlLineInfo)reader;
        var message = string.Equals(reader.NamespaceURI, V1XmlNamespace, StringComparison.Ordinal)
            ? "The theme definition uses the v1 format, which is no longer read. Convert it to v2 with " +
              $"'{ConversionCommand}'."
            : $"The theme definition root must be <Theme xmlns=\"{XmlNamespace}\">.";
        return Failure(source, UnsupportedVersionCode, lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0,
            lineInfo.HasLineInfo() ? lineInfo.LinePosition : 0, "/", message);
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
        private string? _language;
        private string? _style;
        private ThemeAppearance? _appearance;
        private string? _density;
        private ThemeSourceLocation _location;
        private readonly SectionBuilder _base = new("/Theme");
        private readonly List<ThemeConditionDocument> _conditions = new();
        private ConditionBuilder? _currentCondition;
        private string _container = "/Theme";
        private bool _inLanguageTokens;

        internal ThemeDocumentBuilder(
            string source,
            List<ThemeDefinitionDiagnostic> diagnostics)
        {
            _source      = source;
            _diagnostics = diagnostics;
        }

        private SectionBuilder Section => _currentCondition?.Section ?? _base;

        internal string ReadElement(XmlReader reader)
        {
            if (!string.Equals(reader.NamespaceURI, XmlNamespace, StringComparison.Ordinal))
            {
                return "/";
            }

            switch (reader.LocalName)
            {
                case "Theme":
                    return ReadTheme(reader);
                case "When":
                    return ReadWhen(reader);
                case "Tokens":
                    _inLanguageTokens = false;
                    _container = Section.CurrentControl is { } control ? control.Path + "/Tokens" : Section.Path + "/Tokens";
                    return _container;
                case "LanguageTokens":
                    _inLanguageTokens = true;
                    _container = Section.Path + "/LanguageTokens";
                    return _container;
                case "Controls":
                    _container = Section.Path + "/Controls";
                    return _container;
                case "Control":
                    return ReadControl(reader);
                case "Token":
                    return ReadToken(reader);
                default:
                    return _container;
            }
        }

        internal string ReadEndElement(XmlReader reader)
        {
            if (!string.Equals(reader.NamespaceURI, XmlNamespace, StringComparison.Ordinal))
            {
                return _container;
            }

            switch (reader.LocalName)
            {
                case "Control":
                    Section.CompleteControl();
                    _container = Section.Path + "/Controls";
                    break;
                case "When":
                    CompleteCondition();
                    _container = "/Theme";
                    break;
                case "Tokens" or "LanguageTokens":
                    _inLanguageTokens = false;
                    _container = Section.CurrentControl?.Path ?? Section.Path;
                    break;
                case "Controls":
                    _container = Section.Path;
                    break;
            }

            return _container;
        }

        internal ThemeDocument? Build()
        {
            if (_id is null || _name is null || _language is null)
            {
                return null;
            }

            CompleteCondition();
            _base.CompleteControl();
            return new ThemeDocument(_id, _name, _language, _style, _appearance, _density, _base.Build(), _conditions,
                _location);
        }

        private string ReadTheme(XmlReader reader)
        {
            _id         = reader.GetAttribute("Id");
            _name       = reader.GetAttribute("Name");
            _language   = reader.GetAttribute("Language");
            _style      = reader.GetAttribute("Style");
            _appearance = ParseAppearance(reader.GetAttribute("Appearance"));
            _density    = reader.GetAttribute("Density");
            _location   = Location(reader, "/Theme");
            _container  = "/Theme";
            return "/Theme";
        }

        private string ReadWhen(XmlReader reader)
        {
            CompleteCondition();
            var index = _conditions.Count + 1;
            var path = $"/Theme/When[{index}]";
            _currentCondition = new ConditionBuilder(
                reader.GetAttribute("Style"),
                ParseAppearance(reader.GetAttribute("Appearance")),
                reader.GetAttribute("Density"),
                reader.GetAttribute("Idiom"),
                Location(reader, path),
                path);
            _container = path;
            if (reader.IsEmptyElement)
            {
                CompleteCondition();
                _container = "/Theme";
            }

            return path;
        }

        private string ReadToken(XmlReader reader)
        {
            var name = reader.GetAttribute("Name") ?? string.Empty;
            var value = reader.GetAttribute("Value") ?? string.Empty;
            var locked = string.Equals(reader.GetAttribute("Locked"), "true", StringComparison.Ordinal);
            var path = $"{_container}/Token[@Name='{name}']";
            var document = new ThemeTokenDocument(name, value, Location(reader, path), locked);
            var section = Section;
            if (section.CurrentControl is { } control)
            {
                control.Tokens.Add(document);
            }
            else if (_inLanguageTokens)
            {
                section.LanguageTokens.Add(document);
            }
            else
            {
                section.Tokens.Add(document);
            }

            return path;
        }

        private string ReadControl(XmlReader reader)
        {
            var section = Section;
            section.CompleteControl();
            var catalog = reader.GetAttribute("Catalog") ?? string.Empty;
            var id = reader.GetAttribute("Id") ?? string.Empty;
            var path = $"{section.Path}/Controls/Control[@Catalog='{catalog}'][@Id='{id}']";
            section.CurrentControl = new ControlBuilder(
                new ThemeControlDocumentIdentity(catalog, id),
                ParseAppearance(reader.GetAttribute("Appearance")),
                reader.GetAttribute("Density"),
                Location(reader, path),
                path);
            _container = path;
            if (reader.IsEmptyElement)
            {
                section.CompleteControl();
                _container = section.Path + "/Controls";
            }

            return path;
        }

        private void CompleteCondition()
        {
            if (_currentCondition is null)
            {
                return;
            }

            _currentCondition.Section.CompleteControl();
            _conditions.Add(_currentCondition.Build());
            _currentCondition = null;
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

        private static ThemeAppearance? ParseAppearance(string? value) => value switch
        {
            "Light" => ThemeAppearance.Light,
            "Dark"  => ThemeAppearance.Dark,
            _       => null
        };

        private sealed class SectionBuilder(string path)
        {
            internal string Path { get; } = path;
            internal List<ThemeTokenDocument> Tokens { get; } = new();
            internal List<ThemeTokenDocument> LanguageTokens { get; } = new();
            internal List<ControlThemeDocument> Controls { get; } = new();
            internal ControlBuilder? CurrentControl { get; set; }

            internal void CompleteControl()
            {
                if (CurrentControl is null)
                {
                    return;
                }

                Controls.Add(CurrentControl.Build());
                CurrentControl = null;
            }

            internal ThemeSectionDocument Build() => new(Tokens, LanguageTokens, Controls);
        }

        private sealed class ConditionBuilder(
            string? style,
            ThemeAppearance? appearance,
            string? density,
            string? idiom,
            ThemeSourceLocation location,
            string path)
        {
            internal SectionBuilder Section { get; } = new(path);

            internal ThemeConditionDocument Build() =>
                new(style, appearance, density, idiom, Section.Build(), location);
        }

        private sealed class ControlBuilder(
            ThemeControlDocumentIdentity identity,
            ThemeAppearance? appearance,
            string? density,
            ThemeSourceLocation location,
            string path)
        {
            internal string Path { get; } = path;
            internal List<ThemeTokenDocument> Tokens { get; } = new();

            internal ControlThemeDocument Build() => new(identity, appearance, density, Tokens, location);
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
