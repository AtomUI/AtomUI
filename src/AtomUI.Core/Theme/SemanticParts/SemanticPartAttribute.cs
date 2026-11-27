namespace AtomUI.Theme;

/// <summary>
/// Declares one semantic part of a control: a user-visible region that applications restyle through the
/// dedicated style class the source generator derives from this declaration. The generator also validates the
/// theme templates against the declarations and emits a <see cref="AtomUI.Theme.Schema.SemanticPartDescriptor" />
/// for each declared part.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class SemanticPartAttribute : Attribute
{
    /// <summary>
    /// Creates a declaration for the semantic part named <paramref name="name" />.
    /// </summary>
    /// <param name="name">
    /// The value of <see cref="Name" />. It must not be null, empty or white space.
    /// </param>
    public SemanticPartAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// The name of the part, unique within the control and made of dot-separated camelCase segments such as
    /// <c>coverMask</c>. The name <c>root</c> is reserved for the implicit root part, which has no declaration.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The path of the part as dot-separated camelCase segments. It determines the name of the dedicated style
    /// class: the control type name, then the path in PascalCase, then <c>Style</c>. When not set, it equals
    /// <see cref="Name" />.
    /// </summary>
    public string? Path { get; set; }
    /// <summary>
    /// The class token that the theme template puts on the part element. It starts with <c>semantic-</c> and
    /// continues with lowercase letters, digits and single hyphens; <c>semantic-root</c> is reserved. It must be
    /// set on every declared part.
    /// </summary>
    public string? SelectorClass { get; set; }
    /// <summary>
    /// The selector route from the control to the part element, used to validate the theme template and to
    /// locate the element in the Gallery semantic preview. Space-separated steps alternate a combinator
    /// (<c>/template/</c>, <c>&gt;</c> or <c>&gt;&gt;</c>) with a class token such as
    /// <c>.semantic-header</c>. The route may start with a class token that the control itself carries, and
    /// its last token must be <see cref="SelectorClass" />. When not set, it is <c>/template/ .</c> followed
    /// by <see cref="SelectorClass" />, which is not allowed for a runtime-created part.
    /// </summary>
    public string? SelectorRoute { get; set; }
    /// <summary>
    /// The type of element that implements the part. It must be set and derive from
    /// <see cref="Avalonia.StyledElement" />. With <see cref="SemanticPartCustomization.SelectorAndTheme" /> it must
    /// derive from <see cref="Avalonia.Controls.Control" />, and the <c>TargetType</c> of the theme assigned to
    /// the part must be assignable to it.
    /// </summary>
    public Type? ContractType { get; set; }
    /// <summary>
    /// How many elements the part has in one control instance. The generator compares it with the number of
    /// elements that the theme template marks with <see cref="SelectorClass" />. The default is
    /// <see cref="SemanticPartCardinality.Single" />.
    /// </summary>
    public SemanticPartCardinality Cardinality { get; set; } = SemanticPartCardinality.Single;
    /// <summary>
    /// How applications restyle the part. With <see cref="SemanticPartCustomization.Selector" />, the default, the
    /// part is restyled only through its dedicated style class. With
    /// <see cref="SemanticPartCustomization.SelectorAndTheme" /> it is also restyled through a
    /// <see cref="Avalonia.Styling.ControlTheme" /> that the control exposes in the property named by
    /// <see cref="ThemePropertyName" />. <see cref="SemanticPartCustomization.Root" /> is reserved for the implicit
    /// root part and is rejected on a declaration.
    /// </summary>
    public SemanticPartCustomization Customization { get; set; } = SemanticPartCustomization.Selector;
    /// <summary>
    /// The name of the property of the control that provides the <see cref="Avalonia.Styling.ControlTheme" />
    /// applied to the part. It is required with <see cref="SemanticPartCustomization.SelectorAndTheme" /> and
    /// rejected otherwise. The property must be public, of type <see cref="Avalonia.Styling.ControlTheme" />, with
    /// a public getter and setter.
    /// </summary>
    public string? ThemePropertyName { get; set; }
    /// <summary>
    /// Set to <see langword="true" /> when the part element lives in another visual root than the control, for
    /// example in an overlay host. The Gallery semantic preview then searches the additional roots that the
    /// control provides.
    /// </summary>
    public bool CrossVisualRoot { get; set; }
    /// <summary>
    /// The release version in which the part was introduced, written as <c>major.minor.patch</c> such as
    /// <c>6.2.2</c>. It is required on every declaration.
    /// </summary>
    public string? Since { get; set; }
    /// <summary>
    /// Set to <see langword="true" /> when the control creates the part element in code at run time instead of the
    /// theme template. The generator does not look for the element in the control template, and
    /// <see cref="SelectorRoute" /> must be declared explicitly.
    /// </summary>
    public bool RuntimeCreated { get; set; }
    /// <summary>
    /// Set to <see langword="true" /> when <see cref="SelectorRoute" /> leaves the control's template and crosses
    /// into a nested control that has a template of its own (a <c>&gt;&gt;</c> step or a second
    /// <c>/template/</c> step). The generator and the Gallery semantic preview then follow the route across that
    /// boundary.
    /// </summary>
    public bool CrossNestedOwners { get; set; }

    /// <summary>
    /// Set to <see langword="true" /> when the part is transparent or hidden by design while the control is at
    /// rest, such as the hover mask over the ImagePreviewer cover. The semantic preview then skips its
    /// <c>Opacity</c> &gt; 0 eligibility check for the part and still outlines it.
    /// </summary>
    public bool RestHidden { get; set; }
}
