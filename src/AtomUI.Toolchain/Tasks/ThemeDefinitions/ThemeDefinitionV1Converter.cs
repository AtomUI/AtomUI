using System.Text;
using System.Xml;
using System.Xml.Linq;
using AtomUI.Theme.Definitions;

namespace AtomUI.Theme.Build;

/// <summary>
/// One file written by <see cref="ThemeDefinitionV1Converter" />: its name and v2 content.
/// </summary>
internal sealed record ConvertedThemeDefinition(string FileName, string Xml, IReadOnlyList<string> SourceFileNames);

/// <summary>
/// Something the conversion could not decide on its own, for the author to review.
/// </summary>
internal sealed record ThemeConversionNote(string FileName, bool IsError, string Message);

/// <summary>
/// The files a conversion writes, the source files it merged away, and its notes.
/// </summary>
internal sealed record ThemeConversionResult(
    IReadOnlyList<ConvertedThemeDefinition> Files,
    IReadOnlyList<string> MergedFileNames,
    IReadOnlyList<ThemeConversionNote> Notes)
{
    internal bool Success => Notes.All(static note => !note.IsError);
}

/// <summary>
/// The one-shot rewrite of v1 theme definitions as v2. The variants of one language and style (light, dark,
/// compact, dark compact) merge into one file whose differences go into <c>&lt;When&gt;</c> sections; a single file
/// keeps the axes its algorithm chain fixed. Brand and neutral colors declared <c>Fixed</c> become locked seeds.
/// A control that disabled its algorithms and overrides a seed changes behavior in v2 and is reported
/// rather than rewritten.
/// </summary>
internal static class ThemeDefinitionV1Converter
{
    internal const string V1Namespace = "https://atomui.net/schemas/theme/v1";
    internal const string V2Namespace = "https://atomui.net/schemas/theme/v2";

    // The adjustable seeds of the built-in languages; a seed override on a control with Algorithm="Disabled" re-derives in v2.
    private static readonly HashSet<string> s_seeds = new(StringComparer.Ordinal)
    {
        "ColorPrimary", "ColorSuccess", "ColorWarning", "ColorError", "ColorInfo", "ColorLink", "ColorTextBase",
        "ColorBgBase", "FontFamily", "FontFamilyCode", "FontSize", "LineWidth", "LineType",
        "BorderRadius", "SizeUnit", "SizeStep", "SizePopupArrow", "ControlHeight", "ZIndexBase", "ZIndexPopupBase",
        "OpacityImage", "MotionUnit", "MotionBase", "MotionEaseOutCirc", "MotionEaseInOutCirc", "MotionEaseOut",
        "MotionEaseInOut", "MotionEaseOutBack", "MotionEaseInBack", "MotionEaseInQuint", "MotionEaseOutQuint",
        "EnableMotion", "EnableWaveSpirit", "Wireframe"
    };

    private sealed record Source(
        string FileName, string Id, string Name, string Language, string? Style, string Appearance, string Density,
        bool BrandFixed, bool NeutralFixed, IReadOnlyList<(string Name, string Value)> Tokens,
        IReadOnlyList<SourceControl> Controls);

    private sealed record SourceControl(
        string Catalog, string Id, string? Appearance, string? Density, IReadOnlyList<(string Name, string Value)> Tokens);

    /// <summary>
    /// Converts the v1 documents of one directory; the input maps a file name to its content.
    /// </summary>
    internal static ThemeConversionResult Convert(IReadOnlyList<(string FileName, string Xml)> documents)
    {
        var notes = new List<ThemeConversionNote>();
        var sources = new List<Source>();
        foreach (var (fileName, xml) in documents)
        {
            if (Parse(fileName, xml, notes) is { } source)
            {
                sources.Add(source);
            }
        }

        var files = new List<ConvertedThemeDefinition>();
        var merged = new List<string>();
        foreach (var group in sources.GroupBy(static source => (source.Language, source.Style))
                                     .OrderBy(static group => group.Key.Language, StringComparer.Ordinal)
                                     .ThenBy(static group => group.Key.Style, StringComparer.Ordinal))
        {
            var members = group.ToArray();
            var variants = members.Select(static member => (member.Appearance, member.Density)).Distinct().Count();
            if (members.Length > 1 && variants == members.Length)
            {
                var root = members.OrderBy(static member => member.Appearance == "Light" ? 0 : 1)
                                  .ThenBy(static member => member.Density == "Default" ? 0 : 1).First();
                files.Add(Write(root, members, notes));
                merged.AddRange(members.Where(member => !ReferenceEquals(member, root)).Select(static member => member.FileName));
            }
            else
            {
                if (members.Length > 1)
                {
                    notes.Add(new ThemeConversionNote(members[0].FileName, false,
                        $"Several themes of '{group.Key.Language}{(group.Key.Style is null ? "" : "." + group.Key.Style)}' share a variant; each is converted on its own."));
                }
                files.AddRange(members.Select(member => Write(member, [member], notes)));
            }
        }

        foreach (var file in files)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(file.Xml));
            var read = ThemeDocumentReader.Read(stream, file.FileName);
            foreach (var diagnostic in read.Diagnostics)
            {
                notes.Add(new ThemeConversionNote(file.FileName, true,
                    $"The converted document does not read as v2: {diagnostic.Code} {diagnostic.Message}"));
            }
        }

        return new ThemeConversionResult(files, merged, notes);
    }

    private static Source? Parse(string fileName, string xml, List<ThemeConversionNote> notes)
    {
        XDocument document;
        try
        {
            document = XDocument.Parse(xml, LoadOptions.SetLineInfo);
        }
        catch (XmlException exception)
        {
            notes.Add(new ThemeConversionNote(fileName, true, "Not well-formed XML: " + exception.Message));
            return null;
        }

        XNamespace ns = V1Namespace;
        var root = document.Root;
        if (root is null || root.Name != ns + "Theme")
        {
            notes.Add(new ThemeConversionNote(fileName, root?.Name.NamespaceName == V2Namespace ? false : true,
                root?.Name.NamespaceName == V2Namespace ? "Already v2; skipped." : "Not a v1 theme definition."));
            return null;
        }

        var algorithms = root.Element(ns + "Algorithms")?.Elements(ns + "Algorithm")
                             .Select(static element => ((string?)element.Attribute("Catalog") ?? "AtomUI", (string)element.Attribute("Id")!))
                             .ToArray() ?? [];
        var (language, style) = ResolveLanguage((string?)root.Attribute("DesignLanguage"), algorithms);
        var dark = algorithms.Any(static algorithm =>
            (algorithm.Item1 == "AtomUI" && algorithm.Item2 == "Dark") || algorithm.Item2.EndsWith(".Dark", StringComparison.Ordinal));
        var compact = algorithms.Any(static algorithm => algorithm.Item1 == "AtomUI" && algorithm.Item2 == "Compact") ||
                      (string?)root.Attribute("Density") == "Compact";
        if (root.Attribute("IsDefault") is not null)
        {
            notes.Add(new ThemeConversionNote(fileName, false,
                "IsDefault is removed; the application chooses its initial theme with WithInitialTheme."));
        }

        var controls = new List<SourceControl>();
        foreach (var control in root.Element(ns + "Controls")?.Elements(ns + "Control") ?? [])
        {
            var catalog = (string?)control.Attribute("Catalog") ?? "AtomUI";
            var id = (string)control.Attribute("Id")!;
            var tokens = Tokens(control.Element(ns + "Tokens"), ns);
            var mode = (string?)control.Attribute("Algorithm");
            if (mode == "Disabled" && tokens.Any(token => s_seeds.Contains(token.Name)))
            {
                notes.Add(new ThemeConversionNote(fileName, false,
                    $"Control '{catalog}:{id}' disabled its algorithms and overrides a seed; v2 re-derives this control from the seed. " +
                    "To keep the previous values, override the Map, Alias or control tokens instead."));
            }

            var chain = control.Element(ns + "Algorithms")?.Elements(ns + "Algorithm")
                               .Select(static element => (string)element.Attribute("Id")!).ToArray() ?? [];
            controls.Add(new SourceControl(catalog, id,
                chain.Contains("Dark") ? "Dark" : chain.Length != 0 ? "Light" : null,
                chain.Contains("Compact") ? "Compact" : chain.Length != 0 ? "Default" : null,
                tokens));
        }

        return new Source(fileName, (string)root.Attribute("Id")!, (string?)root.Attribute("Name") ?? (string)root.Attribute("Id")!,
            language, style, dark ? "Dark" : "Light", compact ? "Compact" : "Default",
            (string?)root.Attribute("BrandColor") == "Fixed", (string?)root.Attribute("NeutralColor") == "Fixed",
            Tokens(root.Element(ns + "Tokens"), ns), controls);
    }

    private static (string Language, string? Style) ResolveLanguage(string? declared, IReadOnlyList<(string Catalog, string Id)> algorithms)
    {
        if (declared is not null)
        {
            var dot = declared.IndexOf('.');
            return dot < 0 ? (declared, null) : (declared[..dot], declared[(dot + 1)..]);
        }

        foreach (var (catalog, id) in algorithms)
        {
            if (catalog != "AtomUI")
            {
                var style = id.EndsWith(".Dark", StringComparison.Ordinal) ? id[..^".Dark".Length] : id;
                return (catalog, style);
            }
        }

        return ("AntDesign", null);
    }

    private static IReadOnlyList<(string Name, string Value)> Tokens(XElement? tokens, XNamespace ns) =>
        tokens?.Elements(ns + "Token").Select(static token => ((string)token.Attribute("Name")!, (string)token.Attribute("Value")!))
              .ToArray() ?? [];

    private static ConvertedThemeDefinition Write(Source root, IReadOnlyList<Source> members, List<ThemeConversionNote> notes)
    {
        XNamespace v2 = V2Namespace;
        var single = members.Count == 1;
        var theme = new XElement(v2 + "Theme",
            new XAttribute("Id", root.Id),
            new XAttribute("Name", root.Name),
            new XAttribute("Language", root.Language),
            root.Style is null ? null : new XAttribute("Style", root.Style),
            single && root.Appearance == "Dark" ? new XAttribute("Appearance", "Dark") : null,
            single && root.Density == "Compact" ? new XAttribute("Density", "Compact") : null);

        // Values every variant agrees on stay in the base section; the others go into the section of their variant.
        var common = CommonTokens(members.Select(static member => member.Tokens).ToArray());
        var tokens = TokensElement(v2, common, root.BrandFixed, root.NeutralFixed, root.FileName, notes);
        theme.Add(tokens);
        theme.Add(LanguageTokensElement(v2, common, root.NeutralFixed));
        var controls = CommonControls(members);
        if (controls.Count != 0)
        {
            theme.Add(new XElement(v2 + "Controls", controls.Select(control => ControlElement(v2, control, withAxes: true))));
        }

        if (!single)
        {
            foreach (var member in members)
            {
                var own = member.Tokens.Where(token => !common.Contains(token)).ToArray();
                var ownControls = OwnControls(member, controls);
                if (own.Length == 0 && ownControls.Count == 0)
                {
                    continue;
                }

                var when = new XElement(v2 + "When",
                    new XAttribute("Appearance", member.Appearance),
                    new XAttribute("Density", member.Density));
                var ownGlobal = own.Where(static token => token.Name != V1NeutralSeed).ToArray();
                if (ownGlobal.Length != 0)
                {
                    when.Add(new XElement(v2 + "Tokens", ownGlobal.Select(token => TokenElement(v2, token, false))));
                }
                when.Add(LanguageTokensElement(v2, own, locked: false));
                if (ownControls.Count != 0)
                {
                    when.Add(new XElement(v2 + "Controls", ownControls.Select(control => ControlElement(v2, control, withAxes: false))));
                }
                theme.Add(when);
            }
        }

        var xml = new XDocument(new XDeclaration("1.0", "utf-8", null), theme).ToString() + Environment.NewLine;
        return new ConvertedThemeDefinition(root.FileName, xml, members.Select(static member => member.FileName).ToArray());
    }

    private static HashSet<(string Name, string Value)> CommonTokens(IReadOnlyList<IReadOnlyList<(string Name, string Value)>> sets)
    {
        var common = new HashSet<(string, string)>(sets[0]);
        foreach (var set in sets.Skip(1))
        {
            common.IntersectWith(set);
        }
        return common;
    }

    // v1's global ColorNeutral seed is the shadcn language seed BaseColor in v2.
    private const string V1NeutralSeed = "ColorNeutral";
    private const string NeutralLanguageSeed = "BaseColor";

    private static XElement? LanguageTokensElement(XNamespace v2, IEnumerable<(string Name, string Value)> tokens, bool locked)
    {
        var neutral = tokens.Where(static token => token.Name == V1NeutralSeed).ToArray();
        return neutral.Length == 0
            ? null
            : new XElement(v2 + "LanguageTokens",
                neutral.Select(token => TokenElement(v2, (NeutralLanguageSeed, token.Value), locked)));
    }

    private static XElement? TokensElement(XNamespace v2, IEnumerable<(string Name, string Value)> tokens, bool brandFixed,
        bool neutralFixed, string fileName, List<ThemeConversionNote> notes)
    {
        var list = tokens.ToList();
        foreach (var (seed, isFixed) in new[] { ("ColorPrimary", brandFixed), (V1NeutralSeed, neutralFixed) })
        {
            if (isFixed && list.All(token => token.Name != seed))
            {
                notes.Add(new ThemeConversionNote(fileName, false,
                    $"The theme fixed '{seed}' without giving it a value in every variant; add the value and Locked=\"true\" to keep it fixed."));
            }
        }

        list.RemoveAll(static token => token.Name == V1NeutralSeed);
        return list.Count == 0
            ? null
            : new XElement(v2 + "Tokens", list.Select(token => TokenElement(v2, token,
                brandFixed && token.Name == "ColorPrimary")));
    }

    private static XElement TokenElement(XNamespace v2, (string Name, string Value) token, bool locked) =>
        new(v2 + "Token", new XAttribute("Name", token.Name), new XAttribute("Value", token.Value),
            locked ? new XAttribute("Locked", "true") : null);

    private static IReadOnlyList<SourceControl> CommonControls(IReadOnlyList<Source> members)
    {
        var result = new List<SourceControl>();
        foreach (var control in members[0].Controls)
        {
            var matches = members.Select(member => member.Controls.FirstOrDefault(other =>
                other.Catalog == control.Catalog && other.Id == control.Id)).ToArray();
            if (matches.Any(static match => match is null))
            {
                continue;
            }

            var common = CommonTokens(matches.Select(static match => match!.Tokens).ToArray());
            var sameAxes = matches.All(match => match!.Appearance == control.Appearance && match.Density == control.Density);
            var appearance = sameAxes ? control.Appearance : null;
            var density = sameAxes ? control.Density : null;
            if (common.Count != 0 || appearance is not null || density is not null)
            {
                result.Add(control with
                {
                    Appearance = appearance,
                    Density = density,
                    Tokens = control.Tokens.Where(common.Contains).ToArray()
                });
            }
        }
        return result;
    }

    private static IReadOnlyList<SourceControl> OwnControls(Source member, IReadOnlyList<SourceControl> common)
    {
        var result = new List<SourceControl>();
        foreach (var control in member.Controls)
        {
            var shared = common.FirstOrDefault(other => other.Catalog == control.Catalog && other.Id == control.Id);
            var own = control.Tokens.Where(token => shared is null || !shared.Tokens.Contains(token)).ToArray();
            if (own.Length != 0)
            {
                result.Add(control with { Appearance = null, Density = null, Tokens = own });
            }
        }
        return result;
    }

    private static XElement ControlElement(XNamespace v2, SourceControl control, bool withAxes) =>
        new(v2 + "Control",
            new XAttribute("Catalog", control.Catalog),
            new XAttribute("Id", control.Id),
            withAxes && control.Appearance is { } appearance ? new XAttribute("Appearance", appearance) : null,
            withAxes && control.Density is { } density ? new XAttribute("Density", density) : null,
            control.Tokens.Count == 0 ? null : new XElement(v2 + "Tokens", control.Tokens.Select(token => TokenElement(v2, token, false))));
}
