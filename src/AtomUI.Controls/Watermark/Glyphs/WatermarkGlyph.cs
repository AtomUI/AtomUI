using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Reactive.Disposables;

namespace AtomUI.Controls;

[GenerateScopedResourceHost]
public abstract partial class WatermarkGlyph : AvaloniaObject
{
    public double HorizontalSpace
    {
        get => GetValue(HorizontalSpaceProperty);
        set => SetValue(HorizontalSpaceProperty, value);
    }

    public static readonly StyledProperty<double> HorizontalSpaceProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(HorizontalSpace), 280d);

    public double VerticalSpace
    {
        get => GetValue(VerticalSpaceProperty);
        set => SetValue(VerticalSpaceProperty, value);
    }

    public static readonly StyledProperty<double> VerticalSpaceProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(VerticalSpace), 40d);

    public double HorizontalOffset
    {
        get => GetValue(HorizontalOffsetProperty);
        set => SetValue(HorizontalOffsetProperty, value);
    }

    public static readonly StyledProperty<double> HorizontalOffsetProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(HorizontalOffset));

    public double VerticalOffset
    {
        get => GetValue(VerticalOffsetProperty);
        set => SetValue(VerticalOffsetProperty, value);
    }

    public static readonly StyledProperty<double> VerticalOffsetProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(VerticalOffset));

    public double Rotate
    {
        get => GetValue(RotateProperty);
        set => SetValue(RotateProperty, value);
    }

    public static readonly StyledProperty<double> RotateProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(Rotate), -20);

    public double Opacity
    {
        get => GetValue(OpacityProperty);
        set => SetValue(OpacityProperty, value);
    }

    public static readonly StyledProperty<double> OpacityProperty = AvaloniaProperty
        .Register<WatermarkGlyph, double>(nameof(Opacity), 0.3);

    public bool IsMirrorUsed
    {
        get => GetValue(IsMirrorUsedProperty);
        set => SetValue(IsMirrorUsedProperty, value);
    }

    public static readonly StyledProperty<bool> IsMirrorUsedProperty = AvaloniaProperty
        .Register<WatermarkGlyph, bool>(nameof(IsMirrorUsed));

    public bool IsCrossUsed
    {
        get => GetValue(IsCrossUsedProperty);
        set => SetValue(IsCrossUsedProperty, value);
    }

    public static readonly StyledProperty<bool> IsCrossUsedProperty = AvaloniaProperty
        .Register<WatermarkGlyph, bool>(nameof(IsCrossUsed), true);

    public abstract void Render(DrawingContext context);

    public abstract Size GetDesiredSize();

    private readonly LinkedList<IResourceHost> _resourceOwners = new();
    private IDisposable? _resourceOwnerAttachment;

    // 一个共享 Glyph 只有一套属性值：最后挂载的活动 owner 提供资源作用域。
    // 生成器管理单一宿主的订阅，本列表只管理共享使用者的选择与退出。
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
            // 资源通知可同步挂载另一个 owner；先建立当前代次的释放槽，
            // 避免外层 Attach 返回后覆盖内层已经接管的 attachment。
            var attachment = new SingleAssignmentDisposable();
            _resourceOwnerAttachment = attachment;
            attachment.Disposable = AttachResourceHost(owner.Value);
        }
        else
        {
            _resourceOwnerAttachment = null;
        }
        // 新宿主接管时生成器先解绑旧宿主；再释放旧 token，避免资源暂时回退到 Application。
        previousAttachment?.Dispose();
    }
}
