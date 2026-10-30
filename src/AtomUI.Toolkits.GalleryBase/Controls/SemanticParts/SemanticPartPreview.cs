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

    public static readonly StyledProperty<Control?> PreviewContentProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Control?>(nameof(PreviewContent));

    /// <summary>
    /// 预览内容在画布中的垂直对齐方式，默认居中；
    /// 内容型预览（如大尺寸组件）可按需覆盖为 Top。
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> PreviewContentAlignmentProperty =
        AvaloniaProperty.Register<SemanticPartPreview, VerticalAlignment>(
            nameof(PreviewContentAlignment), VerticalAlignment.Center);

    public VerticalAlignment PreviewContentAlignment
    {
        get => GetValue(PreviewContentAlignmentProperty);
        set => SetValue(PreviewContentAlignmentProperty, value);
    }

    /// <summary>
    /// 预览画布的最小高度，默认 360。大型内容（如日历弹层）需要更大的
    /// 锚点下方空间时调高；画布始终收缩到宿主钳制高度以内，不产生
    /// 可视区之外的布局。
    /// </summary>
    public static readonly StyledProperty<double> PreviewStageMinHeightProperty =
        AvaloniaProperty.Register<SemanticPartPreview, double>(
            nameof(PreviewStageMinHeight), 360d);

    public double PreviewStageMinHeight
    {
        get => GetValue(PreviewStageMinHeightProperty);
        set => SetValue(PreviewStageMinHeightProperty, value);
    }

    public static readonly StyledProperty<Control?> SemanticOwnerProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Control?>(nameof(SemanticOwner));

    public static readonly StyledProperty<Type?> SemanticOwnerTypeProperty =
        AvaloniaProperty.Register<SemanticPartPreview, Type?>(nameof(SemanticOwnerType));

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

    public Control? PreviewContent
    {
        get => GetValue(PreviewContentProperty);
        set => SetValue(PreviewContentProperty, value);
    }

    public Control? SemanticOwner
    {
        get => GetValue(SemanticOwnerProperty);
        set => SetValue(SemanticOwnerProperty, value);
    }

    public Type? SemanticOwnerType
    {
        get => GetValue(SemanticOwnerTypeProperty);
        set => SetValue(SemanticOwnerTypeProperty, value);
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// 单个 Preview 覆盖多个公开 owner 时的 owner 声明集合（按声明顺序合并 Part 列表）。
    /// 声明后忽略 <see cref="SemanticOwner" /> / <see cref="SemanticOwnerType" /> 的单 owner 路径。
    /// </summary>
    public AvaloniaList<SemanticPartPreviewOwner> SemanticOwners { get; } = [];

    public AvaloniaList<SemanticPartDescription> PartDescriptions { get; } = [];

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
        _infoView.Show(item);
        _codeView ??= new SemanticPartPreviewCodeExampleControl();
        _codeView.Show(_codeViewer);
        InfoContent = _infoView;
        CodeContent = _codeView;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        PseudoClasses.Set(CompactPseudoClass, finalSize.Width < CompactWidth);
        return base.ArrangeOverride(finalSize);
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        DeactivatePreview();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        DeactivatePreview();
        _infoView?.Clear();
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
            // 顺序契约：先按 PartDescriptions 的声明顺序列出该 owner 已描述的 Part，再追加其余 Part。
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

    // owner 解析：声明了 SemanticOwners 时逐个解析并按顺序合并；否则退回单 owner 路径。
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

    // 描述映射以 (ownerType, path) 为键：多 owner 下 root 等路径会在不同 owner 上重名。
    private Dictionary<(Type OwnerType, string Path), SemanticPartDescription> BuildDescriptionMap(
        IReadOnlyList<OwnerBinding> bindings)
    {
        // 单 owner 时描述可省略 OwnerType，默认归属该唯一 owner；多 owner 时必须显式区分。
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

        // 未知路径必须拒绝：description 只能在所属 owner 的真实 Part 上声明。
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
        _infoView?.Clear();
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

        // 高亮框钳制在预览画布内：溢出画布的部件只高亮画布内部分，
        // 画布外的描边会误导为页面其他区域也是语义部件。
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

    // 一个 Preview 内的单个 owner 绑定：descriptor + 实例锚点。
    private sealed record OwnerBinding(ControlSemanticDescriptor Descriptor, Control Anchor);

    // 单 owner 与多 owner 的解析结果：多 owner 时 Part 列表按 owner 声明顺序合并。
    private sealed record ControlDescriptorBundle(bool IsMultiOwner, Type[] OwnerTypes);
}
