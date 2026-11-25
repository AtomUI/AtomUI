# AtomUI Avalonia 12 Migration Guide

## Project-Specific Context

AtomUI is a comprehensive UI component library for Avalonia with a three-tier architecture:
- **Foundation** - Core utilities and base classes
- **Base Controls** (`AtomUI.Controls`) - Platform-agnostic controls
- **Platform Controls** (`AtomUI.Desktop.Controls`) - Desktop-specific implementations

## AtomUI-Specific Breaking Changes

### 1. NavMenu Control Migration

**Files affected:**
- `src/AtomUI.Desktop.Controls/NavMenu/INavMenu.cs`
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenu.cs`
- `src/AtomUI.Desktop.Controls/NavMenu/NavMenuItem.cs`
- `src/AtomUI.Desktop.Controls/NavMenu/DefaultNavMenuInteractionHandler.cs`

**Key changes:**

#### Remove INavMenu.VisualRoot property
```csharp
// Before
public interface INavMenu
{
    Visual? VisualRoot { get; }
}

// After
public interface INavMenu
{
    // VisualRoot removed - use TopLevel.GetTopLevel(visual) instead
}
```

#### Update focus event handlers
```csharp
// Before
protected override void OnGotFocus(GotFocusEventArgs e)
protected override void OnLostFocus(RoutedEventArgs e)

// After
protected override void OnGotFocus(FocusChangedEventArgs e)
protected override void OnLostFocus(FocusChangedEventArgs e)
```

#### Update Popup handling
```csharp
// Before
if (parent?.Popup?.Host != null)
{
    await parent._popup.MotionAwareCloseAsync();
}

// After
if (parent?.Popup?.IsOpen == true)
{
    parent._popup.IsOpen = false;
    await Task.Delay(50);
}
```

#### Update root initialization
```csharp
// Before
_root = Menu.VisualRoot as IRenderRoot;

// After
_root = TopLevel.GetTopLevel(visual);
```

#### Update GetVisualRoot() usage in NavMenuItem
```csharp
// Before
if (Presenter?.GetVisualRoot() != null)
{
    UpdateLayout();
}

// After
if (Presenter != null && TopLevel.GetTopLevel(Presenter) != null)
{
    UpdateLayout();
}
```

### 2. TextBlock Controls Migration

**Files affected:**
- `src/AtomUI.Desktop.Controls/TextBlock/SelectableTextBlock.cs`
- `src/AtomUI.Desktop.Controls/TextBlock/HyperLinkTextBlock.cs`

**Key changes:**

#### Focus event parameters
```csharp
// Before
protected override void OnGotFocus(GotFocusEventArgs e)
protected override void OnLostFocus(RoutedEventArgs e)

// After
protected override void OnGotFocus(FocusChangedEventArgs e)
protected override void OnLostFocus(FocusChangedEventArgs e)
```

#### Text formatting API
```csharp
// Before
new GenericTextRunProperties(typeface, FontFeatures, FontSize, TextDecorations, Foreground)

// After
new GenericTextRunProperties(
    typeface,
    FontSize,
    TextDecorations,
    Foreground,
    fontFeatures: FontFeatures)
```

#### Clipboard operations
```csharp
// Add using statement
using Avalonia.Input.Platform;

// SetTextAsync is now available as extension method
await clipboard.SetTextAsync(text);
```

### 3. ScrollViewer Migration

**Files affected:**
- `src/AtomUI.Controls/ScrollViewer/AbstractScrollViewer.cs`
- `src/AtomUI.Desktop.Controls/ScrollViewer/ScrollViewer.cs`
- `src/AtomUI.Desktop.Controls/ScrollViewer/ScrollBar.cs`

**Key changes:**

#### TopLevelUtils usage
```csharp
// Add using
using AtomUI.Controls.Utils;

// Use TopLevelUtils for desktop scaling
var scaling = TopLevelUtils.GetDesktopScaling(this);
```

#### Math utilities
```csharp
// Before
MathUtilities.Clamp(value, min, max)

// After
Math.Clamp(value, min, max)
```

### 4. ToolTip Service Migration

**Files affected:**
- `src/AtomUI.Desktop.Controls/Tooltip/ToolTipService.cs`

**Key changes:**

#### Replace IInputRoot with TopLevel
```csharp
// Before
public void Update(IInputRoot root, Visual? candidateToolTipHost)
{
    if (root == currentToolTip?.GetVisualRoot() as IInputRoot)
    {
        return;
    }
}

// After
public void Update(TopLevel root, Visual? candidateToolTipHost)
{
    if (root == TopLevel.GetTopLevel(currentToolTip as Visual))
    {
        return;
    }
}
```

#### Update RawInputEventArgs handling
```csharp
// Before
if (e.Root == currentTip.GetVisualRoot() as IInputRoot)
{
    isTooltipEvent = true;
}
else if (e.Root.GetRootElement() == _tipControl?.GetVisualRoot())
{
    _lastWindowEventTime = pointerEvent.Timestamp;
}

// After
if (e.Root == TopLevel.GetTopLevel(currentTip))
{
    isTooltipEvent = true;
}
else if (e.Root == TopLevel.GetTopLevel(_tipControl))
{
    _lastWindowEventTime = pointerEvent.Timestamp;
}
```

### 5. Helper Utilities & Extension Methods

**Files affected:**
- `src/AtomUI.Core/Controls/VisualExtensions.cs`
- Any custom utility classes using removed APIs

**Key changes:**

#### Update GetVisualRoot() extension
```csharp
// Before
internal static Visual? GetVisualRoot(this Visual visual)
{
    return visual.GetPresentationSource()?.RootVisual;
}

// After - Option 1: Update to use TopLevel
internal static TopLevel? GetVisualRootTopLevel(this Visual visual)
{
    return TopLevel.GetTopLevel(visual);
}

// After - Option 2: Remove and replace all usages with TopLevel.GetTopLevel()
```

#### Audit all extension methods
- Check for methods returning `IInputRoot`, `IRenderRoot`, `ILayoutRoot`
- Check for methods using `GetPresentationSource()`
- Check for methods using `GetRootElement()`
- Update or remove as appropriate

## Migration Workflow for AtomUI

### Phase 1: Preparation
1. Create a new branch: `feature/avalonia12-migration`
2. Update all Avalonia NuGet packages to v12
3. Update .NET target framework if needed (v12 requires .NET 8+)

### Phase 2: Core Infrastructure
1. Migrate `AtomUI.Core` first
   - Update focus event handlers
   - Fix text formatting APIs
   - Update utility usage
2. Run tests to verify core functionality

### Phase 3: Base Controls
1. Migrate `AtomUI.Controls`
   - Update all control base classes
   - Fix event handlers
   - Update text/clipboard APIs
2. Run unit tests

### Phase 4: Desktop Controls
1. Migrate `AtomUI.Desktop.Controls`
   - Update NavMenu control
   - Update TextBlock controls
   - Update ScrollViewer
   - Update all other desktop-specific controls
2. Run integration tests

### Phase 5: Gallery/Showcase
1. Update Gallery application
2. Test all showcase examples
3. Verify visual appearance

### Phase 6: Documentation
1. Update migration guide
2. Update API documentation
3. Update breaking changes list

## Testing Strategy

### Unit Tests
```bash
dotnet test src/AtomUI.Core.Tests
dotnet test src/AtomUI.Controls.Tests
python3 scripts/verification/test.py run --scope iterate --path src/AtomUI.Desktop.Controls
```

### Integration Tests
```bash
dotnet test src/AtomUI.Gallery.Tests
```

### Manual Testing
1. Run Gallery application
2. Test each control
3. Verify animations and transitions
4. Check theme switching
5. Test keyboard navigation

## Common AtomUI Patterns

### Pattern 1: Control with Focus Events
```csharp
public class MyControl : Control
{
    protected override void OnGotFocus(FocusChangedEventArgs e)
    {
        base.OnGotFocus(e);
        // Handle focus
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        // Handle blur
    }
}
```

### Pattern 2: Popup Management
```csharp
public class PopupContainer
{
    private Popup _popup;

    public void OpenPopup()
    {
        _popup.IsOpen = true;
    }

    public void ClosePopup()
    {
        _popup.IsOpen = false;
    }

    public bool IsPopupOpen => _popup?.IsOpen ?? false;
}
```

### Pattern 3: Text Formatting
```csharp
public class TextControl : Control
{
    protected override TextLayout CreateTextLayout(string? text)
    {
        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        
        var defaultProperties = new GenericTextRunProperties(
            typeface,
            FontSize,
            TextDecorations,
            Foreground,
            fontFeatures: FontFeatures);

        var paragraphProperties = new GenericTextParagraphProperties(
            FlowDirection, TextAlignment, true, false,
            defaultProperties, TextWrapping, LineHeight, 0, LetterSpacing);

        return new TextLayout(
            new FormattedTextSource(text ?? "", defaultProperties, null),
            paragraphProperties,
            TextTrimming,
            maxWidth,
            maxHeight,
            MaxLines);
    }
}
```

## Troubleshooting

### Issue: "Type or namespace 'GotFocusEventArgs' not found"
**Solution:** Update to `FocusChangedEventArgs` in both `OnGotFocus` and `OnLostFocus`

### Issue: "Property 'Host' not found on type 'Popup'"
**Solution:** Replace with `IsOpen` property

### Issue: "Method 'VisualRoot' is inaccessible"
**Solution:** Use `TopLevel.GetTopLevel(visual)` instead

### Issue: "Constructor overload not found for GenericTextRunProperties"
**Solution:** Check parameter order - should be `(typeface, fontSize, textDecorations, foregroundBrush, fontFeatures)`

### Issue: "SetTextAsync not found on IClipboard"
**Solution:** Add `using Avalonia.Input.Platform;` for extension methods

## Validation Checklist

- [ ] All Avalonia packages updated to v12
- [ ] All focus event handlers updated
- [ ] All Popup.Host references replaced
- [ ] All TopLevel API calls updated
- [ ] All text formatting constructors fixed
- [ ] All clipboard operations updated
- [ ] All MathUtilities.Clamp replaced
- [ ] All custom extension methods audited and updated
- [ ] All helper utilities using removed APIs updated
- [ ] GetVisualRoot() extension method updated or removed
- [ ] IInputRoot, IRenderRoot, ILayoutRoot references removed
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Gallery application runs
- [ ] All controls render correctly
- [ ] Animations work smoothly
- [ ] Theme switching works
- [ ] Keyboard navigation works

## References

- [Avalonia 12 Official Breaking Changes](https://docs.avaloniaui.net/docs/avalonia12-breaking-changes)
- [AtomUI GitHub Repository](https://github.com/chinboy/AtomUIV6)
- [Avalonia 12 Migration Examples](migration-examples.md)
- [Breaking Changes Reference](avalonia12-breaking-changes.md)
