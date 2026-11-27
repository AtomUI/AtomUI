using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Reactive.Disposables;

namespace AtomUI.Controls;

/// <summary>
/// One drawing of a watermark, together with the settings that lay out its repetitions as a tiled grid.
/// A glyph is not a control: <see cref="Render" /> paints it, <see cref="GetDesiredSize" /> reports its
/// size, and <see cref="Watermark" /> repeats it over the watermarked element. While a watermark shows the
/// glyph, the glyph resolves dynamic resources in the scope of that element.
/// </summary>
[GenerateScopedResourceHost]
public abstract partial class WatermarkGlyph : AvaloniaObject
{
    /// <summary>
    /// Horizontal gap between neighbouring glyphs: their distance is this value plus the glyph width.
    /// A negative value lets them overlap, as long as the glyph width plus this value is finite and above
    /// zero; otherwise nothing is drawn. The default is 280.
    /// </summary>
    public double HorizontalSpace
    {
        get => GetValue(HorizontalSpaceProperty);
        set => SetValue(HorizontalSpaceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="HorizontalSpace" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HorizontalSpaceProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(HorizontalSpace), 280d);

    /// <summary>
    /// Vertical gap between neighbouring glyphs: their distance is this value plus the glyph height.
    /// A negative value lets them overlap, as long as the glyph height plus this value is finite and above
    /// zero; otherwise nothing is drawn. The default is 40.
    /// </summary>
    public double VerticalSpace
    {
        get => GetValue(VerticalSpaceProperty);
        set => SetValue(VerticalSpaceProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="VerticalSpace" /> property.
    /// </summary>
    public static readonly StyledProperty<double> VerticalSpaceProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(VerticalSpace), 40d);

    /// <summary>
    /// Horizontal origin of the tile grid, measured from the left edge of the watermarked area; the columns
    /// follow from there in steps of glyph width plus <see cref="HorizontalSpace" />. A value that is not
    /// finite draws nothing. The default is 0.
    /// </summary>
    public double HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="HorizontalOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HorizontalOffsetProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(HorizontalOffset));

    /// <summary>
    /// Vertical origin of the tile grid, measured from the top edge of the watermarked area; the rows follow
    /// from there in steps of glyph height plus <see cref="VerticalSpace" />. A value that is not finite
    /// draws nothing. The default is 0.
    /// </summary>
    public double VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="VerticalOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> VerticalOffsetProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(VerticalOffset));

    /// <summary>
    /// Rotation of every glyph around the center of its tile, in degrees. Columns affected by
    /// <see cref="IsMirrorUsed" /> are rotated by the negated angle. A value that is not finite draws nothing.
    /// The default is -20.
    /// </summary>
    public double Rotate
    {
        get => GetValue(RotateProperty);
        set => SetValue(RotateProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Rotate" /> property.
    /// </summary>
    public static readonly StyledProperty<double> RotateProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(Rotate), -20);

    /// <summary>
    /// Opacity of the whole watermark pattern; 0 hides it. The default is 0.3.
    /// </summary>
    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Opacity" /> property.
    /// </summary>
    public static readonly StyledProperty<double> OpacityProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(Opacity), 0.3);

    /// <summary>
    /// When true, every second column is rotated by the negated <see cref="Rotate" /> angle, so that adjacent
    /// columns tilt in opposite directions. The default is false.
    /// </summary>
    public bool IsMirrorUsed
    {
        get => GetValue(IsMirrorUsedProperty);
        set => SetValue(IsMirrorUsedProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IsMirrorUsed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMirrorUsedProperty = AvaloniaProperty
        .Register<WatermarkGlyph, bool>(nameof(IsMirrorUsed));

    /// <summary>
    /// When true, every second row is shifted right by half a column step (glyph width plus
    /// <see cref="HorizontalSpace" />), so that the glyphs are staggered instead of forming straight columns.
    /// The default is true.
    /// </summary>
    public bool IsCrossUsed
    {
        get => GetValue(IsCrossUsedProperty);
        set => SetValue(IsCrossUsedProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="IsCrossUsed" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCrossUsedProperty = AvaloniaProperty
        .Register<WatermarkGlyph, bool>(nameof(IsCrossUsed), true);

    /// <summary>
    /// Paints the glyph once, with its top-left corner at the origin of the context. The watermark calls it for
    /// each visible tile it draws through the glyph; an exact <see cref="TextGlyph" /> is drawn from its text
    /// layout instead.
    /// </summary>
    /// <param name="context">
    /// The drawing context of one tile. Its origin is the top-left corner of the glyph, and its axes are already
    /// rotated around the center of the tile; implementers do not apply <see cref="Rotate" /> themselves.
    /// </param>
    public abstract void Render(DrawingContext context);

    /// <summary>
    /// Reports the size of one glyph. It is the size of a tile; <see cref="HorizontalSpace" /> and
    /// <see cref="VerticalSpace" /> are added to it to give the distance between tiles.
    /// </summary>
    /// <returns>
    /// The size of the glyph. A width or height that is not finite or not greater than zero makes the watermark
    /// draw nothing.
    /// </returns>
    public abstract Size GetDesiredSize();

    private readonly LinkedList<IResourceHost> _resourceOwners = new();
    private IDisposable? _resourceOwnerAttachment;

    // A glyph shared by several owners has a single set of property values, so only the owner attached last
    // supplies the resource scope. The generator manages the subscription to that single host; this list only
    // selects the active shared owner and handles a shared owner leaving.
    internal IDisposable AttachToResourceOwner(IResourceHost owner)
    {
        var node = _resourceOwners.AddLast(owner);
        UpdateResourceOwner();
        return Disposable.Create(() => DetachResourceOwner(node));
    }

    private void DetachResourceOwner(LinkedListNode<IResourceHost> node)
    {
        if (node != _resourceOwners.Last)
        {
            _resourceOwners.Remove(node);
            return;
        }

        _resourceOwners.Remove(node);
        UpdateResourceOwner();
    }

    private void UpdateResourceOwner()
    {
        var previousAttachment = _resourceOwnerAttachment;
        if (_resourceOwners.Last is { } owner)
        {
            // A resource notification can synchronously attach another owner. The disposal slot of this generation is
            // created first, so that the outer call does not overwrite, once AttachResourceHost has returned, the
            // attachment a nested call has already taken over.
            var attachment = new SingleAssignmentDisposable();
            _resourceOwnerAttachment = attachment;
            var scopedHost = AttachResourceHost(owner.Value);
            attachment.Disposable = this is TextGlyph textGlyph
                ? new CompositeDisposable(textGlyph.CreateThemeDefaultBindings(), scopedHost)
                : scopedHost;
        }
        else
        {
            _resourceOwnerAttachment = null;
        }
        // When a new host takes over, the generator unbinds the old host first. The previous attachment is disposed
        // only after that, so the resources do not fall back to the Application resources in the meantime.
        previousAttachment?.Dispose();
        OnResourceOwnerChanged(_resourceOwners.Last?.Value);
    }

    /// <summary>
    /// Called after the active resource owner changed; <paramref name="owner" /> is null once the last owner
    /// has detached. Subclasses that follow the owner's theme resolve their values here.
    /// </summary>
    private protected virtual void OnResourceOwnerChanged(IResourceHost? owner)
    {
    }
}
