using System;
using System.Collections.Generic;

namespace AtomUI.Generator;

internal static partial class ThemeLiteralCodec
{
    // Describes Avalonia's immutable family identity only. No host/font manager is consulted.
    internal static string FormatFontFamily(string description)
    {
        if (string.IsNullOrEmpty(description)) { throw new FormatException("Font family description is empty."); }
        var segments = description.Split(',');
        var names = new List<string>();
        var sources = new List<Uri?>();
        foreach (var part in segments)
        {
            var segment = part.Trim();
            var separator = segment.IndexOf('#');
            var name = segment.Length == description.Length ? description : segment;
            Uri? source = null;
            if (separator >= 0 && segment.IndexOf('#', separator + 1) == -1)
            {
                var path = segment.Substring(0, separator).Trim();
                var innerName = segment.Substring(separator + 1).Trim();
                if (path.Length == 0) { name = innerName; }
                else if (path.IndexOf('/') >= 0 && Uri.TryCreate(path, UriKind.Relative, out source)) { name = innerName; }
                else if (Uri.TryCreate(path, UriKind.Absolute, out source)) { name = innerName; }
            }
            names.Add(name); sources.Add(source);
        }
        if (segments.Length == 1) { return (sources[0] is null ? "" : sources[0]!.ToString() + "#") + names[0]; }
        // Avalonia validates each system fallback URI while creating its composite key.
        for (var i = 0; i < names.Count; i++) { if (sources[i] is null) { _ = new Uri("systemfont:" + names[i], UriKind.Absolute); } }
        return new Uri("compositefont:" + description, UriKind.Absolute).ToString() + "#" + string.Join(", ", names);
    }
}
