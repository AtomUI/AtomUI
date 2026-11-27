using System.Collections.Concurrent;
using AtomUI.Media;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

/// <summary>
/// The five theme colors a preset image is derived from, as resolved from the control's brushes
/// (before they are composited onto <see cref="Background" />).
/// </summary>
internal readonly record struct PresetImageColors(
    Color Border,
    Color BorderSecondary,
    Color Shadow,
    Color Content,
    Color Background);

internal static class BuiltInImageBuilder
{
    // Keyed by (preset, colors): every Empty under the same theme shares one built svg string.
    // Colors are part of the key, so a theme switch simply produces new entries.
    private const int MaxCachedPresetImages = 32;

    private static readonly ConcurrentDictionary<(PresetEmptyImage Preset, PresetImageColors Colors), string> s_presetImages = new();

    public static string GetPresetImage(PresetEmptyImage preset, in PresetImageColors colors)
    {
        var key = (preset, colors);
        if (s_presetImages.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var built = BuildPresetImage(preset, colors);
        if (s_presetImages.Count >= MaxCachedPresetImages)
        {
            // Bounded in case colors are animated or user-generated.
            s_presetImages.Clear();
        }
        return s_presetImages.GetOrAdd(key, built);
    }

    private static string BuildPresetImage(PresetEmptyImage preset, in PresetImageColors colors)
    {
        var background = colors.Background;
        var border = ColorUtils.OnBackground(colors.Border, background);
        var borderSecondary = ColorUtils.OnBackground(colors.BorderSecondary, background);
        var shadow = ColorUtils.OnBackground(colors.Shadow, background);
        var content = ColorUtils.OnBackground(colors.Content, background);
        return preset == PresetEmptyImage.Default
            ? BuildDefaultImage(shadow, border, borderSecondary)
            : BuildSimpleImage(content, border, shadow);
    }

    public static string BuildSimpleImage(Color contentColor, Color borderColor, Color shadowColor)
    {
        var source =
            $"""
             <svg width="64" height="41" viewBox="0 0 64 41" xmlns="http://www.w3.org/2000/svg">
                <title>Simple Empty</title>
                <g transform="translate(0 1)" fill="none" fillRule="evenodd">
                   <ellipse fill="{shadowColor.HexName()}" cx="32" cy="33" rx="32" ry="7" />
                   <g fillRule="nonzero" stroke="{borderColor.HexName()}">
                      <path d="M55 12.76L44.854 1.258C44.367.474 43.656 0 42.907 0H21.093c-.749 0-1.46.474-1.947 1.257L9 12.761V22h46v-9.24z" />
                      <path
                        d="M41.613 15.931c0-1.605.994-2.93 2.227-2.931H55v18.137C55 33.26 53.68 35 52.05 35h-40.1C10.32 35 9 33.259 9 31.137V13h11.16c1.233 0 2.227 1.323 2.227 2.928v.022c0 1.605 1.005 2.901 2.237 2.901h14.752c1.232 0 2.237-1.308 2.237-2.913v-.007z"
                        fill="{contentColor.HexName()}"
                      />
                   </g>
                </g>
             </svg>
             """;
        return source;
    }

    public static string BuildDefaultImage(Color contentColor, Color borderColor, Color borderColorSecondary)
    {
        var source =
            $"""
             <svg
               width="184"
               height="152"
               viewBox="0 0 184 152"
               xmlns="http://www.w3.org/2000/svg">
               <title>empty image</title>
               <g fill="none" fillRule="evenodd">
                 <g transform="translate(24 31.67)">
                   <ellipse
                     fillOpacity=".8"
                     fill="{borderColorSecondary.HexName()}"
                     cx="67.797"
                     cy="106.89"
                     rx="67.797"
                     ry="12.668"
                   />
                   <path
                     d="M122.034 69.674L98.109 40.229c-1.148-1.386-2.826-2.225-4.593-2.225h-51.44c-1.766 0-3.444.839-4.592 2.225L13.56 69.674v15.383h108.475V69.674z"
                     fill="{contentColor.HexName()}"
                   />
                   <path
                     d="M101.537 86.214L80.63 61.102c-1.001-1.207-2.507-1.867-4.048-1.867H31.724c-1.54 0-3.047.66-4.048 1.867L6.769 86.214v13.792h94.768V86.214z"
                     fill="url(#linearGradient-1)"
                     transform="translate(13.56)"
                   />
                   <path
                     d="M33.83 0h67.933a4 4 0 0 1 4 4v93.344a4 4 0 0 1-4 4H33.83a4 4 0 0 1-4-4V4a4 4 0 0 1 4-4z"
                     fill="{borderColor.HexName()}"
                   />
                   <path
                     d="M42.678 9.953h50.237a2 2 0 0 1 2 2V36.91a2 2 0 0 1-2 2H42.678a2 2 0 0 1-2-2V11.953a2 2 0 0 1 2-2zM42.94 49.767h49.713a2.262 2.262 0 1 1 0 4.524H42.94a2.262 2.262 0 0 1 0-4.524zM42.94 61.53h49.713a2.262 2.262 0 1 1 0 4.525H42.94a2.262 2.262 0 0 1 0-4.525zM121.813 105.032c-.775 3.071-3.497 5.36-6.735 5.36H20.515c-3.238 0-5.96-2.29-6.734-5.36a7.309 7.309 0 0 1-.222-1.79V69.675h26.318c2.907 0 5.25 2.448 5.25 5.42v.04c0 2.971 2.37 5.37 5.277 5.37h34.785c2.907 0 5.277-2.421 5.277-5.393V75.1c0-2.972 2.343-5.426 5.25-5.426h26.318v33.569c0 .617-.077 1.216-.221 1.789z"
                     fill="{contentColor.HexName()}"
                   />
                 </g>
                 <path
                   d="M149.121 33.292l-6.83 2.65a1 1 0 0 1-1.317-1.23l1.937-6.207c-2.589-2.944-4.109-6.534-4.109-10.408C138.802 8.102 148.92 0 161.402 0 173.881 0 184 8.102 184 18.097c0 9.995-10.118 18.097-22.599 18.097-4.528 0-8.744-1.066-12.28-2.902z"
                   fill="{borderColor.HexName()}"
                 />
                 <g transform="translate(149.65 15.383)" fill="{contentColor.HexName()}">
                   <ellipse cx="20.654" cy="3.167" rx="2.849" ry="2.815" />
                   <path d="M5.698 5.63H0L2.898.704zM9.259.704h4.985V5.63H9.259z" />
                 </g>
               </g>
             </svg>
             """;
        return source;
    }
}
