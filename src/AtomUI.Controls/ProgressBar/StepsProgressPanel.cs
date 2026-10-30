using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Controls.Commons;

internal sealed class ProgressStepRectangle : Rectangle
{
    internal static readonly StyledProperty<IBrush?> StepBrushProperty =
        AvaloniaProperty.Register<ProgressStepRectangle, IBrush?>(nameof(StepBrush));

    internal IBrush? StepBrush
    {
        get => GetValue(StepBrushProperty);
        set => SetValue(StepBrushProperty, value);
    }
}

internal sealed class StepsProgressPanel : Panel
{
    private readonly List<ProgressStepRectangle> _tracks = [];
    private AbstractGeneralStepsProgressBar? _owner;
    private string? _trackSemanticClass;

    internal void SetTrackSemanticClass(string semanticClass)
    {
        if (_trackSemanticClass == semanticClass)
        {
            return;
        }

        foreach (var track in _tracks)
        {
            if (_trackSemanticClass is not null)
            {
                track.Classes.Remove(_trackSemanticClass);
            }

            track.Classes.Add(semanticClass);
        }

        _trackSemanticClass = semanticClass;
    }

    internal void SynchronizeTracks()
    {
        if (TemplatedParent is not AbstractGeneralStepsProgressBar owner)
        {
            ClearTracks();
            return;
        }

        SetOwner(owner);
        ReconcileTrackCount();
        UpdateTrackBrushes();
        InvalidateMeasure();
        InvalidateArrange();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SynchronizeTracks();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        SetOwner(null);
        ClearTracks();
        base.OnDetachedFromVisualTree(e);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_owner is null)
        {
            foreach (var child in Children)
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        var grooveRect = _owner.GetStepsProgressBarRect(finalSize);
        var chunkWidth = _owner.GetChunkWidth();
        var chunkHeight = _owner.GetChunkHeight();
        var offsetX = grooveRect.X;
        var offsetY = grooveRect.Y;

        for (var index = 0; index < _tracks.Count; index++)
        {
            Rect trackRect;
            if (_owner.Orientation == Orientation.Horizontal)
            {
                trackRect = new Rect(offsetX, offsetY, chunkWidth, chunkHeight);
                offsetX += chunkWidth + AbstractGeneralStepsProgressBar.ChunkSpace;
            }
            else
            {
                trackRect = new Rect(offsetX, offsetY, chunkHeight, chunkWidth);
                offsetY += chunkWidth + AbstractGeneralStepsProgressBar.ChunkSpace;
            }

            _tracks[index].Arrange(trackRect);
        }

        foreach (var child in Children)
        {
            if (child is not ProgressStepRectangle)
            {
                child.Arrange(child.Name == AbstractProgressBar.ProgressIndicatorPart
                    ? _owner.GetStepsProgressIndicatorRect(finalSize)
                    : new Rect(finalSize));
            }
        }

        return finalSize;
    }

    private void SetOwner(AbstractGeneralStepsProgressBar? owner)
    {
        if (ReferenceEquals(_owner, owner))
        {
            return;
        }

        if (_owner is not null)
        {
            _owner.PropertyChanged -= HandleOwnerPropertyChanged;
        }

        _owner = owner;
        if (_owner is not null)
        {
            _owner.PropertyChanged += HandleOwnerPropertyChanged;
        }
    }

    private void HandleOwnerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (_owner is null)
        {
            return;
        }

        if (e.Property == AbstractGeneralStepsProgressBar.StepsProperty)
        {
            ReconcileTrackCount();
        }

        if (e.Property == AbstractGeneralStepsProgressBar.StepsProperty ||
            e.Property == AbstractProgressBar.PercentageProperty ||
            e.Property == AbstractProgressBar.StrokeBrushProperty ||
            e.Property == AbstractProgressBar.GrooveBrushProperty ||
            e.Property == AbstractGeneralStepsProgressBar.StepsStrokeBrushProperty)
        {
            UpdateTrackBrushes();
        }

        if (e.Property == AbstractGeneralStepsProgressBar.StepsProperty ||
            e.Property == AbstractGeneralStepsProgressBar.ChunkWidthProperty ||
            e.Property == AbstractGeneralStepsProgressBar.ChunkHeightProperty ||
            e.Property == AbstractProgressBar.EffectiveSizeTypeProperty ||
            e.Property == AbstractLineProgress.OrientationProperty ||
            e.Property == AbstractGeneralStepsProgressBar.PercentPositionProperty ||
            e.Property == AbstractProgressBar.IsProgressInfoVisibleProperty)
        {
            InvalidateMeasure();
            InvalidateArrange();
        }
    }

    private void ReconcileTrackCount()
    {
        if (_owner is null)
        {
            return;
        }

        while (_tracks.Count > _owner.Steps)
        {
            var index = _tracks.Count - 1;
            var track = _tracks[index];
            _tracks.RemoveAt(index);
            Children.Remove(track);
        }

        while (_tracks.Count < _owner.Steps)
        {
            var track = new ProgressStepRectangle();
            if (_trackSemanticClass is not null)
            {
                track.Classes.Add(_trackSemanticClass);
            }

            Children.Insert(_tracks.Count, track);
            _tracks.Add(track);
        }
    }

    private void UpdateTrackBrushes()
    {
        if (_owner is null)
        {
            return;
        }

        var filledSteps = (int)Math.Round(_owner.Steps * _owner.Percentage / 100);
        for (var index = 0; index < _tracks.Count; index++)
        {
            _tracks[index].StepBrush = index < filledSteps
                ? _owner.GetStepBrush(index)
                : _owner.GrooveBrush;
        }
    }

    private void ClearTracks()
    {
        foreach (var track in _tracks)
        {
            Children.Remove(track);
        }

        _tracks.Clear();
    }
}
