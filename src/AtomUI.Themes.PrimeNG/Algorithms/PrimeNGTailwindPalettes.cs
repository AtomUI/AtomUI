using System.Collections.Immutable;
using Avalonia.Media;

namespace AtomUI.Themes.PrimeNG.Algorithms;

/// <summary>
/// The Tailwind colour scales that PrimeNG ships as primitive tokens, stops 50 to 950 in order.
/// </summary>
/// <remarks>
/// Transcribed verbatim from the <c>primitive</c> export of <c>@primeuix/themes</c> 3.1.1
/// (<c>dist/aura/base/index.mjs</c>; the Lara and Nora presets carry the identical table). These hand-tuned scales
/// differ from what the PrimeUIX <c>palette</c> formula derives from their 500 stop, so a seed that equals one of
/// these 500 stops resolves to its table (see <see cref="PrimeNGPalette.Resolve"/>).
/// </remarks>
internal static class PrimeNGTailwindPalettes
{
    internal static readonly ImmutableArray<Color> Emerald = [C(0xECFDF5), C(0xD1FAE5), C(0xA7F3D0), C(0x6EE7B7), C(0x34D399), C(0x10B981), C(0x059669), C(0x047857), C(0x065F46), C(0x064E3B), C(0x022C22)];
    internal static readonly ImmutableArray<Color> Green = [C(0xF0FDF4), C(0xDCFCE7), C(0xBBF7D0), C(0x86EFAC), C(0x4ADE80), C(0x22C55E), C(0x16A34A), C(0x15803D), C(0x166534), C(0x14532D), C(0x052E16)];
    internal static readonly ImmutableArray<Color> Lime = [C(0xF7FEE7), C(0xECFCCB), C(0xD9F99D), C(0xBEF264), C(0xA3E635), C(0x84CC16), C(0x65A30D), C(0x4D7C0F), C(0x3F6212), C(0x365314), C(0x1A2E05)];
    internal static readonly ImmutableArray<Color> Red = [C(0xFEF2F2), C(0xFEE2E2), C(0xFECACA), C(0xFCA5A5), C(0xF87171), C(0xEF4444), C(0xDC2626), C(0xB91C1C), C(0x991B1B), C(0x7F1D1D), C(0x450A0A)];
    internal static readonly ImmutableArray<Color> Orange = [C(0xFFF7ED), C(0xFFEDD5), C(0xFED7AA), C(0xFDBA74), C(0xFB923C), C(0xF97316), C(0xEA580C), C(0xC2410C), C(0x9A3412), C(0x7C2D12), C(0x431407)];
    internal static readonly ImmutableArray<Color> Amber = [C(0xFFFBEB), C(0xFEF3C7), C(0xFDE68A), C(0xFCD34D), C(0xFBBF24), C(0xF59E0B), C(0xD97706), C(0xB45309), C(0x92400E), C(0x78350F), C(0x451A03)];
    internal static readonly ImmutableArray<Color> Yellow = [C(0xFEFCE8), C(0xFEF9C3), C(0xFEF08A), C(0xFDE047), C(0xFACC15), C(0xEAB308), C(0xCA8A04), C(0xA16207), C(0x854D0E), C(0x713F12), C(0x422006)];
    internal static readonly ImmutableArray<Color> Teal = [C(0xF0FDFA), C(0xCCFBF1), C(0x99F6E4), C(0x5EEAD4), C(0x2DD4BF), C(0x14B8A6), C(0x0D9488), C(0x0F766E), C(0x115E59), C(0x134E4A), C(0x042F2E)];
    internal static readonly ImmutableArray<Color> Cyan = [C(0xECFEFF), C(0xCFFAFE), C(0xA5F3FC), C(0x67E8F9), C(0x22D3EE), C(0x06B6D4), C(0x0891B2), C(0x0E7490), C(0x155E75), C(0x164E63), C(0x083344)];
    internal static readonly ImmutableArray<Color> Sky = [C(0xF0F9FF), C(0xE0F2FE), C(0xBAE6FD), C(0x7DD3FC), C(0x38BDF8), C(0x0EA5E9), C(0x0284C7), C(0x0369A1), C(0x075985), C(0x0C4A6E), C(0x082F49)];
    internal static readonly ImmutableArray<Color> Blue = [C(0xEFF6FF), C(0xDBEAFE), C(0xBFDBFE), C(0x93C5FD), C(0x60A5FA), C(0x3B82F6), C(0x2563EB), C(0x1D4ED8), C(0x1E40AF), C(0x1E3A8A), C(0x172554)];
    internal static readonly ImmutableArray<Color> Indigo = [C(0xEEF2FF), C(0xE0E7FF), C(0xC7D2FE), C(0xA5B4FC), C(0x818CF8), C(0x6366F1), C(0x4F46E5), C(0x4338CA), C(0x3730A3), C(0x312E81), C(0x1E1B4B)];
    internal static readonly ImmutableArray<Color> Violet = [C(0xF5F3FF), C(0xEDE9FE), C(0xDDD6FE), C(0xC4B5FD), C(0xA78BFA), C(0x8B5CF6), C(0x7C3AED), C(0x6D28D9), C(0x5B21B6), C(0x4C1D95), C(0x2E1065)];
    internal static readonly ImmutableArray<Color> Purple = [C(0xFAF5FF), C(0xF3E8FF), C(0xE9D5FF), C(0xD8B4FE), C(0xC084FC), C(0xA855F7), C(0x9333EA), C(0x7E22CE), C(0x6B21A8), C(0x581C87), C(0x3B0764)];
    internal static readonly ImmutableArray<Color> Fuchsia = [C(0xFDF4FF), C(0xFAE8FF), C(0xF5D0FE), C(0xF0ABFC), C(0xE879F9), C(0xD946EF), C(0xC026D3), C(0xA21CAF), C(0x86198F), C(0x701A75), C(0x4A044E)];
    internal static readonly ImmutableArray<Color> Pink = [C(0xFDF2F8), C(0xFCE7F3), C(0xFBCFE8), C(0xF9A8D4), C(0xF472B6), C(0xEC4899), C(0xDB2777), C(0xBE185D), C(0x9D174D), C(0x831843), C(0x500724)];
    internal static readonly ImmutableArray<Color> Rose = [C(0xFFF1F2), C(0xFFE4E6), C(0xFECDD3), C(0xFDA4AF), C(0xFB7185), C(0xF43F5E), C(0xE11D48), C(0xBE123C), C(0x9F1239), C(0x881337), C(0x4C0519)];
    internal static readonly ImmutableArray<Color> Slate = [C(0xF8FAFC), C(0xF1F5F9), C(0xE2E8F0), C(0xCBD5E1), C(0x94A3B8), C(0x64748B), C(0x475569), C(0x334155), C(0x1E293B), C(0x0F172A), C(0x020617)];
    internal static readonly ImmutableArray<Color> Gray = [C(0xF9FAFB), C(0xF3F4F6), C(0xE5E7EB), C(0xD1D5DB), C(0x9CA3AF), C(0x6B7280), C(0x4B5563), C(0x374151), C(0x1F2937), C(0x111827), C(0x030712)];
    internal static readonly ImmutableArray<Color> Zinc = [C(0xFAFAFA), C(0xF4F4F5), C(0xE4E4E7), C(0xD4D4D8), C(0xA1A1AA), C(0x71717A), C(0x52525B), C(0x3F3F46), C(0x27272A), C(0x18181B), C(0x09090B)];
    internal static readonly ImmutableArray<Color> Neutral = [C(0xFAFAFA), C(0xF5F5F5), C(0xE5E5E5), C(0xD4D4D4), C(0xA3A3A3), C(0x737373), C(0x525252), C(0x404040), C(0x262626), C(0x171717), C(0x0A0A0A)];
    internal static readonly ImmutableArray<Color> Stone = [C(0xFAFAF9), C(0xF5F5F4), C(0xE7E5E4), C(0xD6D3D1), C(0xA8A29E), C(0x78716C), C(0x57534E), C(0x44403C), C(0x292524), C(0x1C1917), C(0x0C0A09)];

    /// <summary>
    /// Every transcribed scale, in the order of the PrimeUIX primitive table. No two scales share a 500 stop.
    /// </summary>
    internal static readonly ImmutableArray<ImmutableArray<Color>> All =
    [
        Emerald, Green, Lime, Red, Orange, Amber, Yellow, Teal, Cyan, Sky, Blue,
        Indigo, Violet, Purple, Fuchsia, Pink, Rose, Slate, Gray, Zinc, Neutral, Stone
    ];

    private static Color C(uint rgb) => Color.FromUInt32(0xFF000000u | rgb);
}
