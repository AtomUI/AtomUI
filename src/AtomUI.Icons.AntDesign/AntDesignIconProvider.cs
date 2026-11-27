using System.Diagnostics.CodeAnalysis;
using AtomUI.Controls;

namespace AtomUI.Icons.AntDesign;

/// <summary>
/// Markup extension that creates the Ant Design icon named by a value of <see cref="AntDesignIconKind" />.
/// </summary>
public partial class AntDesignIconProvider : IconProvider<AntDesignIconKind>
{
    public AntDesignIconProvider()
    {
    }

    /// <summary>
    /// Creates a provider that produces the icon for <paramref name="kind" />.
    /// </summary>
    /// <param name="kind">
    /// The icon kind that <c>Kind</c> is set to.
    /// </param>
    public AntDesignIconProvider(AntDesignIconKind kind)
        : base(kind)
    {
    }

    /// <summary>
    /// Returns the icon that represents <paramref name="kind" />, created by the generated <c>CreateIcon</c> method.
    /// </summary>
    /// <param name="kind">
    /// The kind of icon to create.
    /// </param>
    /// <returns>
    /// The created icon.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the icon cannot be created, including when <paramref name="kind" /> has no generated icon; the error
    /// of the generated factory is kept as the inner exception.
    /// </exception>
    protected override Icon GetIcon(AntDesignIconKind kind)
    {
        try
        {
            return CreateIcon(kind);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Create icon {kind} failed", ex);
        }
    }

    /// <summary>
    /// Returns the type of the generated icon class of <paramref name="kind" />, which has a public parameterless
    /// constructor.
    /// </summary>
    /// <param name="kind">
    /// The kind whose icon type is requested.
    /// </param>
    /// <returns>
    /// The generated icon class of the kind.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="kind" /> has no generated icon.
    /// </exception>
    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    protected override Type GetTypeForKind(AntDesignIconKind kind)
    {
        return GetIconType(kind);
    }
}
