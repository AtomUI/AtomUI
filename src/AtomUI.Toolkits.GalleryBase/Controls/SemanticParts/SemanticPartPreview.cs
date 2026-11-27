using System.Globalization;
using System.Text;
using AtomUI.Theme.Schema;
using AtomUI.Toolkits.GalleryBase.Localization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Toolkits.GalleryBase.Controls;

/// <summary>
/// Gallery inspector for the Semantic Parts of a control. It shows the control in a preview stage, lists
/// the parts of one or more owners, highlights the hovered or pinned part in the preview and shows the
/// details of a selected part together with a styling example.
/// The part list is built when the preview is activated and rebuilt when the preview content, an owner or
/// a description changes. Dispose the control to release the highlight session and the detail views.
/// </summary>
[PseudoClasses(CompactPseudoClass)]
public class SemanticPartPreview : TemplatedControl, IDisposable
{
    private const string CompactPseudoClass = ":compact";
    private const double CompactWidth = 820;

    /// <summary>
    /// Derives the parts pane max height from the content height bound the
    /// preview inherits from the showcase host; see SemanticPartPreviewTheme.
    /// </summary>
    public static readonly IValueConverter PaneMaxHeightConverter = new SemanticPartPaneMaxHeightConverter();

    /// <summary>
    /// Identifies the <see cref="PreviewContent" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> PreviewContentProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Control?>(nameof(PreviewContent));

    /// <summary>
    /// Identifies the <see cref="PreviewContentAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> PreviewContentAlignmentProperty =
        AvaloniaProperty.Register<SemanticPartPreview, VerticalAlignment>(
            nameof(PreviewContentAlignment), VerticalAlignment.Center);

    /// <summary>
    /// Vertical position of <see cref="PreviewContent" /> in the area of the preview stage below the title.
    /// The content is centered by default. Set it to <see cref="VerticalAlignment.Top" /> for tall content,
    /// such as a large component, so that it starts directly under the title.
    /// </summary>
    public VerticalAlignment PreviewContentAlignment
    {
        get => GetValue(PreviewContentAlignmentProperty);
        set => SetValue(PreviewContentAlignmentProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="PreviewStageMinHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> PreviewStageMinHeightProperty =
        AvaloniaProperty.Register<SemanticPartPreview, double>(
            nameof(PreviewStageMinHeight), 360d);

    /// <summary>
    /// Minimum height, in device-independent pixels, of the preview stage. The default is 360. Raise it when
    /// tall content, such as a calendar popup, needs more room below its anchor.
    /// The stage still shrinks to the height the showcase host clamps the preview to. When the host bounds the
    /// height of the Semantic Parts content, it uses this value plus the insets of the inspection panel as the
    /// lower limit of the clamp; below that limit the page scrolls.
    /// </summary>
    public double PreviewStageMinHeight
    {
        get => GetValue(PreviewStageMinHeightProperty);
        set => SetValue(PreviewStageMinHeightProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SemanticOwner" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> SemanticOwnerProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Control?>(nameof(SemanticOwner));

    /// <summary>
    /// Identifies the <see cref="SemanticOwnerType" /> property.
    /// </summary>
    public static readonly StyledProperty<Type?> SemanticOwnerTypeProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Type?>(nameof(SemanticOwnerType));

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SemanticPartPreview, string?>(nameof(Title));

    internal static readonly DirectProperty<SemanticPartPreview, IReadOnlyList<SemanticPartPreviewItem>> ItemsProperty =
        AvaloniaProperty.RegisterDirect<SemanticPartPreview, IReadOnlyList<SemanticPartPreviewItem>>(
            nameof(Items),
            preview => preview.Items);

    internal static readonly DirectProperty<SemanticPartPreview, Control?> InfoContentProperty =
        AvaloniaProperty.RegisterDirect<SemanticPartPreview, Control?>(
            nameof(InfoContent),
            preview => preview.InfoContent);

    internal static readonly DirectProperty<SemanticPartPreview, Control?> CodeContentProperty =
        AvaloniaProperty.RegisterDirect<SemanticPartPreview, Control?>(
            nameof(CodeContent),
            preview => preview.CodeContent);

    private IReadOnlyList<SemanticPartPreviewItem> _items = Array.Empty<SemanticPartPreviewItem>();
    private Control? _infoContent;
    private Control? _codeContent;
    private readonly List<OwnerBinding> _ownerBindings = [];
    private ControlDescriptorBundle? _controlDescriptor;
    private SemanticPartRegistry? _registry;
    private SemanticPartPreviewItem? _hoveredPart;
    private SemanticPartPreviewItem? _pinnedPart;
    private SemanticPartHighlightSession? _activeHighlightSession;
    private GalleryCodeViewer? _codeViewer;
    private SemanticPartPreviewInfoControl? _infoView;
    private SemanticPartPreviewCodeExampleControl? _codeView;
    private bool _highlightUpdateQueued;
    private bool _isDisposed;

    public SemanticPartPreview()
    {
        PartDescriptions.CollectionChanged += (_, _) => ResetPresentation();
        SemanticOwners.CollectionChanged += (_, _) => ResetPresentation();
    }

    /// <summary>
    /// The control demonstrated in the preview stage.
    /// While <see cref="SemanticOwners" /> is empty and <see cref="SemanticOwner" /> is
    /// <see langword="null" />, this control is also the single Semantic Part owner: the parts come from the
    /// descriptor of <see cref="SemanticOwnerType" /> or of the control's own runtime type, and every instance
    /// of that type found in the content is highlighted. It must then be set before the preview is activated.
    /// </summary>
    public Control? PreviewContent
    {
        get => GetValue(PreviewContentProperty);
        set => SetValue(PreviewContentProperty, value);
    }

    /// <summary>
    /// The control that anchors the inspection when the Semantic Part owner is not
    /// <see cref="PreviewContent" />. The parts come from the descriptor of <see cref="SemanticOwnerType" />
    /// or of the runtime type of this control. Highlighting covers every instance of that owner type found in
    /// the preview content, or in this control and its descendants when the content is <see langword="null" />.
    /// Ignored while <see cref="SemanticOwners" /> declares owners.
    /// </summary>
    public Control? SemanticOwner
    {
        get => GetValue(SemanticOwnerProperty);
        set => SetValue(SemanticOwnerProperty, value);
    }

    /// <summary>
    /// The owner type whose Semantic Part descriptor supplies the parts of the single owner path.
    /// When it is <see langword="null" />, the runtime type of <see cref="SemanticOwner" />, or of
    /// <see cref="PreviewContent" />, is used.
    /// The owner control must be an instance of this type, and a descriptor must be registered for the type;
    /// otherwise building the part list throws <see cref="InvalidOperationException" />.
    /// Ignored while <see cref="SemanticOwners" /> declares owners.
    /// </summary>
    public Type? SemanticOwnerType
    {
        get => GetValue(SemanticOwnerTypeProperty);
        set => SetValue(SemanticOwnerTypeProperty, value);
    }

    /// <summary>
    /// The heading shown above the preview content in the preview stage.
    /// The heading is hidden when the value is <see langword="null" /> or empty.
    /// </summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The owner declarations used when one preview covers several public owners. The part lists of the owners
    /// are merged in declaration order. Once the collection has entries, the single owner path through
    /// <see cref="SemanticOwner" /> and <see cref="SemanticOwnerType" /> is ignored.
    /// Any change to the collection rebuilds the part list.
    /// </summary>
    public AvaloniaList<SemanticPartPreviewOwner> SemanticOwners { get; } = [];

    /// <summary>
    /// Optional descriptions for the listed parts. A description replaces the default text of a part and can
    /// replace the code of its styling example.
    /// Described parts are listed first, in the order of this collection; the other parts of an owner follow.
    /// <see cref="SemanticPartDescription.OwnerType" /> can be omitted only while the preview has a single
    /// owner. A path that does not exist on its owner, a duplicate path or a missing owner type with several
    /// owners makes building the part list throw <see cref="InvalidOperationException" />.
    /// </summary>
    public AvaloniaList<SemanticPartDescription> PartDescriptions { get; } = [];

    /// <summary>
    /// Additional visual roots, such as the presenter of an open popup, that a highlight also searches for
    /// Semantic Part instances. Register a root when the demonstrated control shows a part in a visual root
    /// that the preview content does not contain, for example in a flyout, and remove it when that root closes.
    /// A highlight session takes a snapshot of the list when it starts, so a change applies to the next update.
    /// </summary>
    public AvaloniaList<Visual> AdditionalRoots { get; } = [];

    internal IReadOnlyList<SemanticPartPreviewItem> Items
    {
        get => _items;
        private set => SetAndRaise(ItemsProperty, ref _items, value);
    }

    internal Control? InfoContent
    {
        get => _infoContent;
        private set => SetAndRaise(InfoContentProperty, ref _infoContent, value);
    }

    internal Control? CodeContent
    {
        get => _codeContent;
        private set => SetAndRaise(CodeContentProperty, ref _codeContent, value);
    }

    internal bool IsPreviewActive { get; private set; }

    internal SemanticPartHighlightSession? ActiveHighlightSession => _activeHighlightSession;

    internal GalleryCodeViewer? CodeViewer => _codeViewer;

    internal SemanticPartPreviewItem? EffectivePart => _pinnedPart ?? _hoveredPart;

    internal void ActivatePreview()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        IsPreviewActive = true;
        EnsurePresentation();
        if (EffectivePart is not null)
        {
            QueueHighlightUpdate();
        }
    }

    internal void DeactivatePreview()
    {
        IsPreviewActive = false;
        _hoveredPart = null;
        _highlightUpdateQueued = false;
        ReleaseHighlightSession();
    }

    internal void SetHoveredPart(SemanticPartPreviewItem item, bool isHovered)
    {
        EnsureKnownItem(item);
        if (isHovered)
        {
            _hoveredPart = item;
        }
        else if (ReferenceEquals(_hoveredPart, item))
        {
            _hoveredPart = null;
        }

        QueueHighlightUpdate();
    }

    internal void TogglePinnedPart(SemanticPartPreviewItem item)
    {
        EnsureKnownItem(item);
        _pinnedPart = ReferenceEquals(_pinnedPart, item) ? null : item;
        foreach (var candidate in Items)
        {
            candidate.IsPinned = ReferenceEquals(candidate, _pinnedPart);
        }
        QueueHighlightUpdate();
    }

    internal void ShowPartInfo(SemanticPartPreviewItem item)
    {
        EnsureKnownItem(item);
        var code = item.CodeSnippet ?? BuildCodeSnippet(item.Descriptor, item.OwnerType);
        _codeViewer ??= new GalleryCodeViewer
        {
            Language = "xml",
            ShowLineNumbers = false
        };
        _codeViewer.CodeText = code;
        _infoView ??= new SemanticPartPreviewInfoControl();
        _infoView.DataContext = item;
        _codeView ??= new SemanticPartPreviewCodeExampleControl();
        _codeView.Show(_codeViewer);
        InfoContent = _infoView;
        CodeContent = _codeView;
    }

    /// <summary>
    /// Sets the <c>:compact</c> pseudo-class when the arranged width is below 820 and clears it otherwise.
    /// The theme rearranges the panes for this class. An override must call the base implementation or set
    /// the class itself.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        PseudoClasses.Set(CompactPseudoClass, finalSize.Width < CompactWidth);
        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Clears the part list, the part details and the highlight when <see cref="PreviewContent" /> changes, or
    /// when <see cref="SemanticOwner" /> or <see cref="SemanticOwnerType" /> changes to a consistent pair.
    /// The pair is consistent when either value is <see langword="null" /> or the control is an instance of the
    /// type. A change that leaves it inconsistent is skipped; the change that makes it consistent is applied.
    /// The list is rebuilt at once while the preview is active, and otherwise when it is activated next.
    /// An override must call the base implementation.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PreviewContentProperty ||
            ((change.Property == SemanticOwnerProperty ||
              change.Property == SemanticOwnerTypeProperty) &&
             IsSemanticOwnerStateResolvable()))
        {
            ResetPresentation();
        }
    }

    /// <summary>
    /// Deactivates the preview when the control leaves the visual tree, which releases the highlight session.
    /// An override must call the base implementation.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DeactivatePreview();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Releases the highlight session, the part detail views and the code viewer, and clears the part list and
    /// the resolved owners. Further calls have no effect. A disposed preview rejects activation with
    /// <see cref="ObjectDisposedException" />.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        DeactivatePreview();
        _infoView?.DataContext = null;
        _infoView = null;
        _codeView?.Clear();
        _codeView = null;
        _codeViewer?.Dispose();
        _codeViewer = null;
        InfoContent = null;
        CodeContent = null;
        Items = Array.Empty<SemanticPartPreviewItem>();
        _controlDescriptor = null;
        _ownerBindings.Clear();
        _registry = null;
        _pinnedPart = null;
        _isDisposed = true;
    }

    private void EnsurePresentation()
    {
        if (_controlDescriptor is not null)
        {
            return;
        }

        var registry = Application.Current?.GetThemeManager()?.SemanticParts ?? SemanticPartRegistry.Empty;
        var bindings = ResolveOwnerBindings(registry);
        if (bindings.Count == 0)
        {
            return;
        }

        var descriptions = BuildDescriptionMap(bindings);
        var items = new List<SemanticPartPreviewItem>();
        foreach (var binding in bindings)
        {
            var partsByPath = binding.Descriptor.Parts.ToDictionary(static part => part.Path, StringComparer.Ordinal);
            // Order contract: the parts of this owner that have a description come first, in the order of
            // PartDescriptions, then the remaining parts of the owner.
            var describedPaths = descriptions.Keys
                                             .Where(key => key.OwnerType == binding.Descriptor.ControlType)
                                             .Select(static key => key.Path)
                                             .ToList();
            var orderedParts = describedPaths.Select(path => partsByPath[path])
                                             .Concat(binding.Descriptor.Parts.Where(part =>
                                                 !describedPaths.Contains(part.Path, StringComparer.Ordinal)));
            foreach (var part in orderedParts)
            {
                descriptions.TryGetValue((binding.Descriptor.ControlType, part.Path), out var description);
                items.Add(new SemanticPartPreviewItem(
                    part,
                    binding.Descriptor.ControlType,
                    description?.Description ?? GetFallbackDescription(part),
                    description?.CodeSnippet));
            }
        }

        _ownerBindings.Clear();
        _ownerBindings.AddRange(bindings);
        _registry = registry;
        var isMultiOwner = bindings.Count > 1;
        _controlDescriptor = new ControlDescriptorBundle(
            isMultiOwner,
            bindings.Select(static binding => binding.Descriptor.ControlType).ToArray());
        foreach (var item in items)
        {
            item.IsOwnerLabelVisible = isMultiOwner;
        }
        Items = items.ToArray();
    }

    // Owner resolution: when SemanticOwners declares owners, each one is resolved and the part lists are
    // merged in that order; otherwise the single owner path is used.
    private List<OwnerBinding> ResolveOwnerBindings(SemanticPartRegistry registry)
    {
        var bindings = new List<OwnerBinding>();
        if (SemanticOwners.Count > 0)
        {
            foreach (var declaration in SemanticOwners)
            {
                ArgumentNullException.ThrowIfNull(declaration);
                var owner = declaration.Owner ??
                            throw new InvalidOperationException(
                                $"{nameof(SemanticPartPreviewOwner)} requires {nameof(SemanticPartPreviewOwner.Owner)}.");
                var ownerType = declaration.OwnerType ?? owner.GetType();
                bindings.Add(CreateBinding(registry, owner, ownerType));
            }
            return bindings;
        }

        var single = SemanticOwner ?? PreviewContent;
        if (single is null)
        {
            if (_controlDescriptor is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SemanticPartPreview)} requires {nameof(SemanticOwner)} or {nameof(PreviewContent)}.");
            }
            return bindings;
        }

        bindings.Add(CreateBinding(registry, single, SemanticOwnerType ?? single.GetType()));
        return bindings;
    }

    private static OwnerBinding CreateBinding(
        SemanticPartRegistry registry,
        Control owner,
        Type ownerType)
    {
        if (!registry.TryGetControl(ownerType, out var descriptor))
        {
            throw new InvalidOperationException(
                $"No Semantic Part descriptor is registered for '{ownerType.FullName}'.");
        }
        if (!descriptor.ControlType.IsInstanceOfType(owner))
        {
            throw new InvalidOperationException(
                $"Semantic owner '{owner.GetType().FullName}' is not compatible with descriptor owner " +
                $"'{descriptor.ControlType.FullName}'.");
        }

        return new OwnerBinding(descriptor, owner);
    }

    // The description map is keyed by (ownerType, path): with several owners, paths such as root repeat on
    // different owners.
    private Dictionary<(Type OwnerType, string Path), SemanticPartDescription> BuildDescriptionMap(
        IReadOnlyList<OwnerBinding> bindings)
    {
        // With a single owner a description may omit OwnerType and belongs to that owner; with several owners it
        // must be stated explicitly.
        var singleOwnerType = bindings.Count == 1 ? bindings[0].Descriptor.ControlType : null;
        var result = new Dictionary<(Type, string), SemanticPartDescription>();
        foreach (var description in PartDescriptions)
        {
            ArgumentNullException.ThrowIfNull(description);
            ArgumentException.ThrowIfNullOrWhiteSpace(description.Path);
            var ownerType = description.OwnerType ?? singleOwnerType ??
                            throw new InvalidOperationException(
                                $"Semantic Part description path '{description.Path}' requires " +
                                $"{nameof(SemanticPartDescription.OwnerType)} when the Preview covers multiple owners.");
            if (!result.TryAdd((ownerType, description.Path), description))
            {
                throw new InvalidOperationException(
                    $"Semantic Part description path '{description.Path}' on '{ownerType.FullName}' is duplicated.");
            }
        }

        // Unknown paths must be rejected: a description may only be declared on a real part of its owner.
        var knownPaths = bindings.ToDictionary(
            static binding => binding.Descriptor.ControlType,
            static binding => binding.Descriptor.Parts.Select(static part => part.Path)
                                    .ToHashSet(StringComparer.Ordinal));
        foreach (var (ownerType, path) in result.Keys)
        {
            if (!knownPaths.TryGetValue(ownerType, out var paths) || !paths.Contains(path))
            {
                throw new InvalidOperationException(
                    $"Semantic Part description path '{path}' does not exist on '{ownerType.FullName}'.");
            }
        }

        return result;
    }

    private void ResetPresentation()
    {
        ReleaseHighlightSession();
        _infoView?.DataContext = null;
        _codeView?.Clear();
        InfoContent = null;
        CodeContent = null;
        _controlDescriptor = null;
        _ownerBindings.Clear();
        _registry = null;
        _hoveredPart = null;
        _pinnedPart = null;
        Items = Array.Empty<SemanticPartPreviewItem>();
        if (IsPreviewActive && !_isDisposed)
        {
            EnsurePresentation();
        }
    }

    private bool IsSemanticOwnerStateResolvable()
    {
        return SemanticOwner is not { } owner ||
               SemanticOwnerType is not { } ownerType ||
               ownerType.IsInstanceOfType(owner);
    }

    private void QueueHighlightUpdate()
    {
        if (!IsPreviewActive || _isDisposed || _highlightUpdateQueued)
        {
            return;
        }

        _highlightUpdateQueued = true;
        Dispatcher.UIThread.Post(ApplyQueuedHighlightUpdate, DispatcherPriority.Render);
    }

    private void ApplyQueuedHighlightUpdate()
    {
        if (!_highlightUpdateQueued)
        {
            return;
        }

        _highlightUpdateQueued = false;
        ReleaseHighlightSession();
        if (!IsPreviewActive ||
            EffectivePart is not { } item ||
            _registry is not { } registry)
        {
            return;
        }

        var anchor = _ownerBindings.FirstOrDefault(binding => binding.Descriptor.ControlType == item.OwnerType)?.Anchor;
        if (anchor is null)
        {
            return;
        }

        // The highlight is clipped to the preview stage: a part that overflows the stage is highlighted only
        // inside it, because a stroke outside the stage suggests that other regions of the page are Semantic
        // Parts too.
        var stage = this.GetTemplateDescendants()
                        .OfType<Border>()
                        .FirstOrDefault(static border => border.Name == "PART_PreviewStage");
        _activeHighlightSession = SemanticPartHighlightSession.Start(
            CollectOwnerInstances(anchor, item.OwnerType),
            item.Descriptor,
            registry,
            AdditionalRoots,
            stage);
    }

    private IReadOnlyList<Control> CollectOwnerInstances(Control anchor, Type ownerType)
    {
        var scope = PreviewContent ?? anchor;
        var instances = new List<Control>();
        if (ownerType.IsInstanceOfType(scope))
        {
            instances.Add(scope);
        }

        foreach (var descendant in scope.GetVisualDescendants().OfType<Control>())
        {
            if (ownerType.IsInstanceOfType(descendant) && !instances.Contains(descendant))
            {
                instances.Add(descendant);
            }
        }

        if (instances.Count == 0)
        {
            instances.Add(anchor);
        }

        return instances;
    }

    private void ReleaseHighlightSession()
    {
        _activeHighlightSession?.Dispose();
        _activeHighlightSession = null;
    }

    private void EnsureKnownItem(SemanticPartPreviewItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!Items.Contains(item))
        {
            throw new ArgumentException("The Semantic Part item does not belong to this Preview.", nameof(item));
        }
    }

    private static string GetFallbackDescription(SemanticPartDescriptor part)
    {
        return string.Equals(part.Path, "root", StringComparison.Ordinal)
            ? GetLocalized(
                SemanticPartPreviewLangResourceKind.RootFallbackDescription,
                "The control root.")
            : FormatLocalized(
                SemanticPartPreviewLangResourceKind.PartFallbackDescription,
                "The {0} semantic region.",
                part.Name);
    }

    private static string GetLocalized(
        SemanticPartPreviewLangResourceKind resourceKind,
        string fallback)
    {
        return Application.Current?.GetLocalizer()?.Get(resourceKind) ?? fallback;
    }

    private static string FormatLocalized(
        SemanticPartPreviewLangResourceKind resourceKind,
        string fallback,
        params object?[] arguments)
    {
        return Application.Current?.GetLocalizer()?.Format(resourceKind, arguments) ??
               string.Format(CultureInfo.CurrentCulture, fallback, arguments);
    }

    private string BuildCodeSnippet(SemanticPartDescriptor part, Type? ownerTypeOverride = null)
    {
        var ownerType = ownerTypeOverride ?? SemanticOwnerType ??
                        throw new InvalidOperationException("Semantic owner metadata is unavailable.");
        const string atomPrefix = "atom";
        var targetType = GetSetterTargetType(part.ContractType, atomPrefix);
        var ownerSelector = $"{atomPrefix}|{ownerType.Name}";

        var builder = new StringBuilder();
        builder.AppendLine("<Styles xmlns=\"https://github.com/avaloniaui\"");
        builder.AppendLine("        xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
        builder.Append($"        xmlns:{atomPrefix}=\"https://atomui.net\"");
        builder.AppendLine(">");
        if (string.Equals(part.Path, "root", StringComparison.Ordinal))
        {
            builder.AppendLine($"  <Style Selector=\"{ownerSelector}\"");
            builder.AppendLine($"         x:SetterTargetType=\"{targetType}\">");
            builder.AppendLine("    <Setter Property=\"Opacity\" Value=\"1\" />");
            builder.AppendLine("  </Style>");
        }
        else
        {
            var styleTypeName = part.StyleType?.Name ??
                                $"{ownerType.Name}{ToPartTypeName(part.Path)}Style";
            builder.AppendLine($"  <Style Selector=\"{ownerSelector}\">");
            builder.AppendLine($"    <{atomPrefix}:{styleTypeName} x:SetterTargetType=\"{targetType}\">");
            builder.AppendLine("      <Setter Property=\"Opacity\" Value=\"1\" />");
            builder.AppendLine($"    </{atomPrefix}:{styleTypeName}>");
            builder.AppendLine("  </Style>");
        }
        builder.Append("</Styles>");
        return builder.ToString();
    }

    private static string GetSetterTargetType(Type contractType, string atomPrefix)
    {
        return contractType.Namespace?.StartsWith("Avalonia.", StringComparison.Ordinal) == true
            ? contractType.Name
            : $"{atomPrefix}:{contractType.Name}";
    }

    private static string ToPartTypeName(string path)
    {
        return string.Concat(path.Split('.').Select(static segment =>
            segment.Length == 0
                ? string.Empty
                : char.ToUpperInvariant(segment[0]) + segment.Substring(1)));
    }

    // One owner binding of a preview: the descriptor and the instance that anchors it.
    private sealed record OwnerBinding(ControlSemanticDescriptor Descriptor, Control Anchor);

    // The result of the owner resolution for one owner or several; with several owners the part list is merged
    // in owner declaration order.
    private sealed record ControlDescriptorBundle(bool IsMultiOwner, Type[] OwnerTypes);
}
