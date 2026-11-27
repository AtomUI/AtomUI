using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Title block of a Gallery showcase page. It shows the title with the category, status and introduced
/// version tags beside it, followed by the subtitle, the description and a metadata card with the
/// namespace, package and base class rows. Every text element except the title is visible only while
/// its text is not empty, and the metadata card only while at least one of its rows has text.
/// </summary>
public class GalleryShowCaseHeader : TemplatedControl
{
    /// <summary>
    /// Identifier of the language resources that provide the built-in texts of the header, such as the
    /// labels of the metadata rows.
    /// </summary>
    public const string LanguageId = "GalleryShowCaseHeader";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="Category" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> CategoryProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Category));

    /// <summary>
    /// Identifies the <see cref="CategoryTagColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> CategoryTagColorProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(CategoryTagColor), "blue");

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> StatusProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Status));

    /// <summary>
    /// Identifies the <see cref="StatusTagColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> StatusTagColorProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(StatusTagColor), "success");

    /// <summary>
    /// Identifies the <see cref="IntroducedVersion" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> IntroducedVersionProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(IntroducedVersion));

    /// <summary>
    /// Identifies the <see cref="IntroducedVersionTagColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> IntroducedVersionTagColorProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(IntroducedVersionTagColor), "blue");

    /// <summary>
    /// Identifies the <see cref="Subtitle" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Subtitle));

    /// <summary>
    /// Identifies the <see cref="Description" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Description));

    /// <summary>
    /// Identifies the <see cref="Namespace" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> NamespaceProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Namespace));

    /// <summary>
    /// Identifies the <see cref="Package" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PackageProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(Package));

    /// <summary>
    /// Identifies the <see cref="BaseClass" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BaseClassProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, string?>(nameof(BaseClass));

    /// <summary>
    /// Identifies the <see cref="MetadataLabelWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MetadataLabelWidthProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, double>(nameof(MetadataLabelWidth), double.NaN);

    /// <summary>
    /// Identifies the <see cref="MetadataValueWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MetadataValueWidthProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, double>(nameof(MetadataValueWidth), double.NaN);

    /// <summary>
    /// Title of the page, shown as the heading at the top of the header. An empty title hides no
    /// element; the tags stay on the heading row.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Category of the showcased component, shown as the first tag beside the title.
    /// The tag is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Category
    {
        get => GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    /// <summary>
    /// Color of the category tag, passed to the tag as its color name. The default is <c>blue</c>.
    /// </summary>
    public string? CategoryTagColor
    {
        get => GetValue(CategoryTagColorProperty);
        set => SetValue(CategoryTagColorProperty, value);
    }

    /// <summary>
    /// Status of the showcased component, shown as the second tag beside the title.
    /// The tag is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Color of the status tag, passed to the tag as its color name. The default is <c>success</c>.
    /// </summary>
    public string? StatusTagColor
    {
        get => GetValue(StatusTagColorProperty);
        set => SetValue(StatusTagColorProperty, value);
    }

    /// <summary>
    /// Version in which the showcased component was introduced, shown as the last tag beside the title.
    /// The tag is hidden while the value is null, empty or white space.
    /// </summary>
    public string? IntroducedVersion
    {
        get => GetValue(IntroducedVersionProperty);
        set => SetValue(IntroducedVersionProperty, value);
    }

    /// <summary>
    /// Color of the introduced version tag, passed to the tag as its color name. The default is <c>blue</c>.
    /// </summary>
    public string? IntroducedVersionTagColor
    {
        get => GetValue(IntroducedVersionTagColorProperty);
        set => SetValue(IntroducedVersionTagColorProperty, value);
    }

    /// <summary>
    /// Subtitle of the page, shown in the secondary text color under the title row.
    /// It is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    /// <summary>
    /// Description of the showcased component, shown as wrapping text under the subtitle.
    /// It is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>
    /// Namespace of the showcased component, shown as the first row of the metadata card.
    /// The row is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Namespace
    {
        get => GetValue(NamespaceProperty);
        set => SetValue(NamespaceProperty, value);
    }

    /// <summary>
    /// Package that ships the showcased component, shown as the second row of the metadata card.
    /// The row is hidden while the value is null, empty or white space.
    /// </summary>
    public string? Package
    {
        get => GetValue(PackageProperty);
        set => SetValue(PackageProperty, value);
    }

    /// <summary>
    /// Base class of the showcased component, shown as the last row of the metadata card.
    /// The row is hidden while the value is null, empty or white space.
    /// </summary>
    public string? BaseClass
    {
        get => GetValue(BaseClassProperty);
        set => SetValue(BaseClassProperty, value);
    }

    /// <summary>
    /// Width of the label column of every row of the metadata card. All rows share this width.
    /// The theme sets the value from the <c>MetadataLabelWidth</c> token; without the theme the value is
    /// <see cref="double.NaN" />.
    /// </summary>
    public double MetadataLabelWidth
    {
        get => GetValue(MetadataLabelWidthProperty);
        set => SetValue(MetadataLabelWidthProperty, value);
    }

    /// <summary>
    /// Width of the value column of every row of the metadata card. All rows share this width.
    /// The theme sets the value from the <c>MetadataValueWidth</c> token; without the theme the value is
    /// <see cref="double.NaN" />.
    /// </summary>
    public double MetadataValueWidth
    {
        get => GetValue(MetadataValueWidthProperty);
        set => SetValue(MetadataValueWidthProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> IsCategoryVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsCategoryVisible), false);

    internal static readonly StyledProperty<bool> IsStatusVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsStatusVisible), false);

    internal static readonly StyledProperty<bool> IsIntroducedVersionVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsIntroducedVersionVisible), false);

    internal static readonly StyledProperty<bool> IsTagsVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsTagsVisible), false);

    internal static readonly StyledProperty<bool> IsSubtitleVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsSubtitleVisible), false);

    internal static readonly StyledProperty<bool> IsDescriptionVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsDescriptionVisible), false);

    internal static readonly StyledProperty<bool> IsNamespaceVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsNamespaceVisible), false);

    internal static readonly StyledProperty<bool> IsPackageVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsPackageVisible), false);

    internal static readonly StyledProperty<bool> IsBaseClassVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsBaseClassVisible), false);

    internal static readonly StyledProperty<bool> IsMetadataVisibleProperty =
        AvaloniaProperty.Register<GalleryShowCaseHeader, bool>(nameof(IsMetadataVisible), false);

    internal bool IsCategoryVisible
    {
        get => GetValue(IsCategoryVisibleProperty);
        set => SetValue(IsCategoryVisibleProperty, value);
    }

    internal bool IsStatusVisible
    {
        get => GetValue(IsStatusVisibleProperty);
        set => SetValue(IsStatusVisibleProperty, value);
    }

    internal bool IsIntroducedVersionVisible
    {
        get => GetValue(IsIntroducedVersionVisibleProperty);
        set => SetValue(IsIntroducedVersionVisibleProperty, value);
    }

    internal bool IsTagsVisible
    {
        get => GetValue(IsTagsVisibleProperty);
        set => SetValue(IsTagsVisibleProperty, value);
    }

    internal bool IsSubtitleVisible
    {
        get => GetValue(IsSubtitleVisibleProperty);
        set => SetValue(IsSubtitleVisibleProperty, value);
    }

    internal bool IsDescriptionVisible
    {
        get => GetValue(IsDescriptionVisibleProperty);
        set => SetValue(IsDescriptionVisibleProperty, value);
    }

    internal bool IsNamespaceVisible
    {
        get => GetValue(IsNamespaceVisibleProperty);
        set => SetValue(IsNamespaceVisibleProperty, value);
    }

    internal bool IsPackageVisible
    {
        get => GetValue(IsPackageVisibleProperty);
        set => SetValue(IsPackageVisibleProperty, value);
    }

    internal bool IsBaseClassVisible
    {
        get => GetValue(IsBaseClassVisibleProperty);
        set => SetValue(IsBaseClassVisibleProperty, value);
    }

    internal bool IsMetadataVisible
    {
        get => GetValue(IsMetadataVisibleProperty);
        set => SetValue(IsMetadataVisibleProperty, value);
    }

    #endregion

    static GalleryShowCaseHeader()
    {
        AffectsMeasure<GalleryShowCaseHeader>(
            TitleProperty,
            CategoryProperty,
            StatusProperty,
            IntroducedVersionProperty,
            SubtitleProperty,
            DescriptionProperty,
            NamespaceProperty,
            PackageProperty,
            BaseClassProperty,
            MetadataLabelWidthProperty,
            MetadataValueWidthProperty);
    }

    public GalleryShowCaseHeader()
    {
        UpdateVisibilityProperties();
    }

    /// <summary>
    /// Called when a styled property of the header changes. It recomputes the visibility properties after
    /// <see cref="Category" />, <see cref="Status" />, <see cref="IntroducedVersion" />, <see cref="Subtitle" />,
    /// <see cref="Description" />, <see cref="Namespace" />, <see cref="Package" /> or <see cref="BaseClass" />
    /// changes. Call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CategoryProperty ||
            change.Property == StatusProperty ||
            change.Property == IntroducedVersionProperty ||
            change.Property == SubtitleProperty ||
            change.Property == DescriptionProperty ||
            change.Property == NamespaceProperty ||
            change.Property == PackageProperty ||
            change.Property == BaseClassProperty)
        {
            UpdateVisibilityProperties();
        }
    }

    private void UpdateVisibilityProperties()
    {
        IsCategoryVisible          = HasText(Category);
        IsStatusVisible            = HasText(Status);
        IsIntroducedVersionVisible = HasText(IntroducedVersion);
        IsTagsVisible              = IsCategoryVisible || IsStatusVisible || IsIntroducedVersionVisible;

        IsSubtitleVisible    = HasText(Subtitle);
        IsDescriptionVisible = HasText(Description);

        IsNamespaceVisible = HasText(Namespace);
        IsPackageVisible   = HasText(Package);
        IsBaseClassVisible = HasText(BaseClass);
        IsMetadataVisible  = IsNamespaceVisible || IsPackageVisible || IsBaseClassVisible;
    }

    private static bool HasText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}
