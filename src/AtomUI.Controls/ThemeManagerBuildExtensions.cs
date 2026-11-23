using AtomUI.Generated.AtomUIControls;

namespace AtomUI.Controls;

internal static class ThemeManagerBuilderExtensions
{
    internal const string PackageId = "AtomUI.Controls.Common";

    public static IAtomUIBuilder UseCommonControls(this IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        PackageMetadata.Definition.Ensure(builder, static package => package
            .DependsOn(static builder =>
            {
                builder.UseImageLoading();
                builder.AddImageCodec(static () => new SvgImageCodec());
            }));

        return builder;
    }
}
