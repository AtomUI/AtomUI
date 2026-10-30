namespace AtomUI.Desktop.Controls;

/// <summary>Content-only context; interaction remains with the containing date cell.</summary>
public record DateViewerCellContext(
    DateTime Value,
    DateTime Today,
    DateViewerCellType CellType,
    string DisplayValue,
    bool IsToday,
    bool IsInView,
    bool IsSelected,
    bool IsDisabled,
    bool IsFocused,
    bool IsRangeStart,
    bool IsRangeEnd,
    bool IsRangeMiddle,
    bool IsRangePreviewStart,
    bool IsRangePreviewEnd,
    bool IsRangePreviewMiddle);
