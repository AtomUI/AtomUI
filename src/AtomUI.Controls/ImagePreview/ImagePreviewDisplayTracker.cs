using System.ComponentModel;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// 预览窗口显示状态机：Immediate 在目标尚无图时立即进入加载占位；WaitForLoaded 以
/// "最近持有可用 FullImage 的 entry"作为保留帧回退，直到目标加载完成。
/// ImagePreviewerDialog 与 ImagePreviewerOverlayHost 共用。
/// 资源约束：WaitForLoaded 任一时刻至多额外持有 1 个保留帧 entry；
/// 当前项与保留帧的订阅分别只经
/// SetCurrentItem 与 SetRetained 变更，严格配对；热路径 Recompute 零分配。
/// </summary>
internal sealed class ImagePreviewDisplayTracker : IDisposable
{
    private static long s_nextTrackerId;

    private readonly Func<ImageSwitchMode> _modeAccessor;
    private readonly Func<ImagePreviewEntry?>? _retainedSeedAccessor;
    private readonly Action _stateChanged;
    private readonly long _trackerId = Interlocked.Increment(ref s_nextTrackerId);
    private ImagePreviewEntry? _currentItem;
    private ImagePreviewEntry? _retainedItem;
    private long _nextTargetRevision;

    internal ImagePreviewDisplayTracker(
        Func<ImageSwitchMode> modeAccessor,
        Action stateChanged,
        Func<ImagePreviewEntry?>? retainedSeedAccessor = null)
    {
        _modeAccessor          = modeAccessor ?? throw new ArgumentNullException(nameof(modeAccessor));
        _stateChanged          = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
        _retainedSeedAccessor  = retainedSeedAccessor;
    }

    internal ImagePreviewEntry? CurrentItem => _currentItem;

    internal IImage? EffectiveImage { get; private set; }

    internal Size EffectiveImageSize { get; private set; }

    internal bool IsCurrentLoading { get; private set; }

    internal bool IsCurrentFailed { get; private set; }

    internal void SetCurrentItem(ImagePreviewEntry? item)
    {
        if (!ReferenceEquals(_currentItem, item))
        {
            // 订阅不变量：entry 被订阅 ⇔ 它是 current 或 retained。
            // 旧 current 若同时是 retained（加载完成后的常态），
            // 退订会切断保留帧通知链，必须保留其订阅。
            var previous = _currentItem;
            _currentItem = item;
            if (_currentItem is not null)
            {
                // 会话内单调序号只用于比较当前 tracker 的展示新旧；owner id
                // 隔离已经关闭的宿主会话，不持有 entry 或延长其生命周期。
                _currentItem.DisplayTargetOwnerId = _trackerId;
                _currentItem.DisplayTargetRevision = ++_nextTargetRevision;
            }
            if (previous is not null && !ReferenceEquals(previous, _retainedItem))
            {
                previous.PropertyChanged -= HandleEntryPropertyChanged;
            }
            if (_currentItem is not null && !ReferenceEquals(_currentItem, _retainedItem))
            {
                _currentItem.PropertyChanged += HandleEntryPropertyChanged;
            }
        }
        Recompute();
    }

    internal void Clear()
    {
        if (_currentItem is not null)
        {
            _currentItem.PropertyChanged -= HandleEntryPropertyChanged;
            _currentItem = null;
        }
        SetRetained(null);
        Recompute();
    }

    internal void Refresh()
    {
        Recompute();
    }

    private void Recompute()
    {
        var mode = _modeAccessor();
        if (_retainedItem is not null &&
            (_currentItem is null || _retainedItem.FullImage is null))
        {
            // 无目标项（集合清空/关闭）或保留帧源已卸载/释放/提交失败时丢弃
            //（经 SetRetained 以同步退订）
            SetRetained(null);
        }

        var currentImage = _currentItem?.FullImage;
        var needsFallback = false;
        if (currentImage is not null)
        {
            SetRetained(mode == ImageSwitchMode.WaitForLoaded ? _currentItem : null);
        }
        else if (_currentItem is { FullImage: null, IsFullFailed: false } &&
                 mode == ImageSwitchMode.WaitForLoaded)
        {
            needsFallback = true;
        }
        else
        {
            // Immediate 不持有旧图：目标从切换瞬间的 Idle 开始即进入加载占位。
            SetRetained(null);
        }
        if (needsFallback)
        {
            currentImage = _retainedItem?.FullImage;
            if (_retainedSeedAccessor is not null)
            {
                // 高频切换时，前一个目标可能在离开 current 后才完成。
                // 只采纳比当前保留帧更新的目标，避免乱序完成使画面倒退。
                var seed = _retainedSeedAccessor.Invoke();
                var retainedRevision = _retainedItem?.DisplayTargetRevision ?? 0;
                if (seed?.FullImage is not null &&
                    seed.DisplayTargetOwnerId == _trackerId &&
                    seed.DisplayTargetRevision > retainedRevision)
                {
                    SetRetained(seed);
                    currentImage = seed.FullImage;
                }
            }
        }

        var imageSize = currentImage is null
            ? default
            : ReferenceEquals(currentImage, _currentItem?.FullImage)
                ? _currentItem.FullImageSize
                : _retainedItem?.FullImageSize ?? currentImage.Size;
        var isLoading = _currentItem?.IsFullLoading == true ||
                        _currentItem is { FullImage: null, IsFullFailed: false };
        var isFailed  = _currentItem?.IsFullFailed == true;
        if (!ReferenceEquals(EffectiveImage, currentImage) ||
            EffectiveImageSize != imageSize ||
            IsCurrentLoading != isLoading ||
            IsCurrentFailed != isFailed)
        {
            // 三值变更短路：未变化不触发宿主回调，避免冗余 SetCurrentValue/能力重算
            EffectiveImage    = currentImage;
            EffectiveImageSize = imageSize;
            IsCurrentLoading = isLoading;
            IsCurrentFailed  = isFailed;
            _stateChanged();
        }
    }

    // 保留帧引用的唯一变更通道：替换时对新旧源（与当前项不同的那个）成对退订/订阅。
    // 禁止在 Recompute 中直接赋值 _retainedItem，避免订阅失去配对。
    private void SetRetained(ImagePreviewEntry? entry)
    {
        if (ReferenceEquals(_retainedItem, entry))
        {
            return;
        }
        if (_retainedItem is not null && !ReferenceEquals(_retainedItem, _currentItem))
        {
            _retainedItem.PropertyChanged -= HandleEntryPropertyChanged;
        }
        _retainedItem = entry;
        if (_retainedItem is not null && !ReferenceEquals(_retainedItem, _currentItem))
        {
            _retainedItem.PropertyChanged += HandleEntryPropertyChanged;
        }
    }

    private void HandleEntryPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ImagePreviewEntry.FullImage) ||
            args.PropertyName == nameof(ImagePreviewEntry.FullState) ||
            args.PropertyName == nameof(ImagePreviewEntry.IsFullLoading) ||
            args.PropertyName == nameof(ImagePreviewEntry.IsFullFailed))
        {
            Recompute();
        }
    }

    public void Dispose()
    {
        if (_currentItem is not null)
        {
            _currentItem.PropertyChanged -= HandleEntryPropertyChanged;
            _currentItem = null;
        }
        SetRetained(null);
    }
}
