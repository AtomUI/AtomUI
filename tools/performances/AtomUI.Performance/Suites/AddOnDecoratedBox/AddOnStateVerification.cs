using AtomUI.Controls;
using AtomUI.Media;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using System.Collections;
using System.Reflection;
using AtomTextBox = AtomUI.Desktop.Controls.TextBox;

namespace AtomUI.Performance;

internal static partial class Program
{
    private static bool RunAddonStateVerification()
    {
        var failures = new List<string>();
        VerifyAddOnStatusAndRuntimeChanges(failures);
        VerifyEmptyAddOnSlotsRemainHidden(failures);
        VerifyAddOnGeometry(failures);
        VerifyContentFramePointerLifecycle(failures);
        VerifyCompactSpaceGeometry(failures);
        VerifyCompactSpacePrimitiveStateGuards(failures);
        VerifySelectRepeatedCompactSpaceAndFeedbackNotificationsAreStable(failures);
        VerifyInputShellRepeatedStateNotificationsAreStable(failures);
        VerifyPenUtilsReusesEquivalentPen(failures);
        VerifyRightAddOnTemplateBindings(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("Addon state verification passed.");
            return true;
        }

        Console.Error.WriteLine("Addon state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyAddOnStatusAndRuntimeChanges(ICollection<string> failures)
    {
        var leftText  = new Avalonia.Controls.TextBlock { Text = "left" };
        var rightIcon = new ProbeIcon();
        var box = new AddOnDecoratedBox
        {
            Width             = 260,
            Content           = new Avalonia.Controls.TextBlock { Text = "content" },
            ContentLeftAddOn  = leftText,
            ContentRightAddOn = rightIcon,
            Status            = InputControlStatus.Error,
            IsMotionEnabled   = false
        };

        using var realized = RealizeControl(box);
        var leftPresenter = FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart);
        Expect(leftPresenter != null, "Content left addon presenter should exist.", failures);
        ExpectBrush(rightIcon.Foreground, box.AddOnStatusIconBrush, "Error status should apply icon brush.", failures);
        ExpectBrush(leftPresenter?.Foreground, box.AddOnStatusForeground, "Error status should apply addon foreground.", failures);

        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Warning);
        RefreshLayout(realized.Window);
        ExpectBrush(rightIcon.Foreground, box.AddOnStatusIconBrush, "Warning status should refresh icon brush.", failures);
        ExpectBrush(leftPresenter?.Foreground, box.AddOnStatusForeground, "Warning status should refresh addon foreground.", failures);

        var sourceForeground = Brush(Color.FromRgb(34, 120, 222));
        var sourceIconBrush  = Brush(Color.FromRgb(222, 80, 34));
        box.SetCurrentValue(AddOnDecoratedBox.AddOnStatusForegroundProperty, sourceForeground);
        box.SetCurrentValue(AddOnDecoratedBox.AddOnStatusIconBrushProperty, sourceIconBrush);
        RefreshLayout(realized.Window);
        ExpectBrush(rightIcon.Foreground, sourceIconBrush, "Source icon brush update should refresh existing addon icon.", failures);
        ExpectBrush(leftPresenter?.Foreground, sourceForeground, "Source foreground update should refresh existing addon presenter.", failures);

        var runtimeIcon = new ProbeIcon();
        box.SetCurrentValue(AddOnDecoratedBox.ContentRightAddOnProperty, runtimeIcon);
        RefreshLayout(realized.Window);
        ExpectBrush(runtimeIcon.Foreground, sourceIconBrush, "Runtime content-right icon should receive current status brush.", failures);

        var runtimeText = new Avalonia.Controls.TextBlock { Text = "runtime-left" };
        box.SetCurrentValue(AddOnDecoratedBox.ContentLeftAddOnProperty, runtimeText);
        RefreshLayout(realized.Window);
        leftPresenter = FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart);
        ExpectBrush(leftPresenter?.Foreground, sourceForeground, "Runtime content-left presenter should receive current foreground.", failures);

        box.ClearValue(AddOnDecoratedBox.AddOnStatusForegroundProperty);
        box.ClearValue(AddOnDecoratedBox.AddOnStatusIconBrushProperty);
        box.SetCurrentValue(InputElement.IsEnabledProperty, false);
        RefreshLayout(realized.Window);
        ExpectBrush(runtimeIcon.Foreground, box.AddOnStatusIconBrush, "Disabled state should refresh runtime addon icon.", failures);
        ExpectBrush(leftPresenter?.Foreground, box.AddOnStatusForeground, "Disabled state should refresh runtime addon foreground.", failures);
    }

    private static void VerifyEmptyAddOnSlotsRemainHidden(ICollection<string> failures)
    {
        var box = new AddOnDecoratedBox
        {
            Width           = 260,
            Content         = new Avalonia.Controls.TextBlock { Text = "content" },
            IsMotionEnabled = false
        };
        using var realized = RealizeControl(box);

        box.SetCurrentValue(AddOnDecoratedBox.StatusProperty, InputControlStatus.Error);
        RefreshLayout(realized.Window);

        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.LeftAddOnPart)?.IsVisible == false,
            "Empty left addon presenter should stay hidden after status changes.",
            failures);
        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.RightAddOnPart)?.IsVisible == false,
            "Empty right addon presenter should stay hidden after status changes.",
            failures);
        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart)?.IsVisible == false,
            "Empty content-left addon presenter should stay hidden after status changes.",
            failures);
        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart)?.IsVisible == false,
            "Empty content-right addon presenter should stay hidden after status changes.",
            failures);
    }

    private static void VerifyAddOnGeometry(ICollection<string> failures)
    {
        var box = new AddOnDecoratedBox
        {
            Width             = 260,
            Content           = new Avalonia.Controls.TextBlock { Text = "content" },
            LeftAddOn         = new Avalonia.Controls.TextBlock { Text = "left" },
            RightAddOn        = new Avalonia.Controls.TextBlock { Text = "right" },
            ContentLeftAddOn  = new Avalonia.Controls.TextBlock { Text = "inner-left" },
            ContentRightAddOn = new Avalonia.Controls.TextBlock { Text = "inner-right" },
            CornerRadius      = new CornerRadius(10),
            BorderThickness   = new Thickness(2),
            StyleVariant      = InputControlStyleVariant.Outlined,
            IsMotionEnabled   = false
        };

        using var _ = RealizeControl(box);

        Expect(box.InnerBoxCornerRadius.TopLeft == 0 &&
               box.InnerBoxCornerRadius.BottomLeft == 0 &&
               box.InnerBoxCornerRadius.TopRight == 0 &&
               box.InnerBoxCornerRadius.BottomRight == 0,
            $"Inner box should cut both sides when outer addons exist, actual {box.InnerBoxCornerRadius}.", failures);
        Expect(box.LeftAddOnCornerRadius.TopLeft == 10 &&
               box.LeftAddOnCornerRadius.BottomLeft == 10 &&
               box.LeftAddOnCornerRadius.TopRight == 0 &&
               box.LeftAddOnCornerRadius.BottomRight == 0,
            $"Left addon corner radius should keep only outer corners, actual {box.LeftAddOnCornerRadius}.", failures);
        Expect(box.RightAddOnCornerRadius.TopLeft == 0 &&
               box.RightAddOnCornerRadius.BottomLeft == 0 &&
               box.RightAddOnCornerRadius.TopRight == 10 &&
               box.RightAddOnCornerRadius.BottomRight == 10,
            $"Right addon corner radius should keep only outer corners, actual {box.RightAddOnCornerRadius}.", failures);
        Expect(box.LeftAddOnBorderThickness == new Thickness(2, 2, 0, 2),
            $"Left addon border thickness should cut the shared edge, actual {box.LeftAddOnBorderThickness}.", failures);
        Expect(box.RightAddOnBorderThickness == new Thickness(0, 2, 2, 2),
            $"Right addon border thickness should cut the shared edge, actual {box.RightAddOnBorderThickness}.", failures);
        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentLeftAddOnPart)?.IsVisible == true,
            "Content-left addon presenter should be visible when inner addon exists.", failures);
        Expect(FindVisualByName<ContentPresenter>(box, AddOnDecoratedBoxThemeConstants.ContentRightAddOnPart)?.IsVisible == true,
            "Content-right addon presenter should be visible when inner addon exists.", failures);
    }

    private static void VerifyContentFramePointerLifecycle(ICollection<string> failures)
    {
        var box = new AddOnDecoratedBox
        {
            Width           = 260,
            Content         = new Avalonia.Controls.TextBlock { Text = "content" },
            IsMotionEnabled = false
        };

        using var realized = RealizeControl(box);
        // ContentFrame 基类从 Border 改为 PixelAlignedBorder（DashedBorder/Decorator 系），
        // 按 Control 查找再断言具体类型；Padding 断言保留，StyleKey 检查随 Border 基类移除而删除。
        var contentFrame = FindVisualByName<Decorator>(box, AddOnDecoratedBoxThemeConstants.ContentFramePart);
        Expect(contentFrame is AddOnDecoratedBoxContentFrame,
            $"Content frame should use {nameof(AddOnDecoratedBoxContentFrame)} to avoid per-instance pointer handlers.",
            failures);
        if (contentFrame == null)
        {
            return;
        }
        Expect(contentFrame.Padding.Left > 0 && contentFrame.Padding.Top > 0,
            $"Content frame should keep token padding after replacing the template Border, actual {contentFrame.Padding}.",
            failures);

        var localHandlerNames = GetLocalRoutedHandlerNames(contentFrame);
        Expect(!localHandlerNames.Contains("InputElement.PointerEntered"),
            "AddOnDecoratedBox content frame should handle PointerEntered through override instead of a local handler.",
            failures);
        Expect(!localHandlerNames.Contains("InputElement.PointerExited"),
            "AddOnDecoratedBox content frame should handle PointerExited through override instead of a local handler.",
            failures);
        Expect(!localHandlerNames.Contains("InputElement.PointerPressed"),
            "AddOnDecoratedBox content frame should handle PointerPressed through override instead of a local handler.",
            failures);
        Expect(!localHandlerNames.Contains("InputElement.PointerReleased"),
            "AddOnDecoratedBox content frame should handle PointerReleased through override instead of a local handler.",
            failures);

        RaiseContentFramePointerEvent(contentFrame, realized.Window, InputElement.PointerEnteredEvent);
        Expect(box.IsInnerBoxHover,
            "Content frame PointerEntered should still update AddOnDecoratedBox.IsInnerBoxHover.",
            failures);

        RaiseControlPrimaryPointerPressed(contentFrame, realized.Window);
        Expect(box.IsInnerBoxHover && box.IsInnerBoxPressed,
            "Content frame PointerPressed should still update hover and pressed state.",
            failures);

        RaiseControlPrimaryPointerReleased(contentFrame, realized.Window);
        Expect(box.IsInnerBoxHover && !box.IsInnerBoxPressed,
            "Content frame PointerReleased should still clear pressed state and keep hover state.",
            failures);

        RaiseContentFramePointerEvent(contentFrame, realized.Window, InputElement.PointerExitedEvent);
        Expect(!box.IsInnerBoxHover,
            "Content frame PointerExited should still clear AddOnDecoratedBox.IsInnerBoxHover.",
            failures);
    }

    private static void RaiseContentFramePointerEvent(
        Control target,
        Visual root,
        RoutedEvent<PointerEventArgs> routedEvent)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.None,
            PointerUpdateKind.Other);
        var localPoint = new Point(
            Math.Max(1, target.Bounds.Width / 2),
            Math.Max(1, target.Bounds.Height / 2));
        var rootPoint = target.TranslatePoint(localPoint, root) ?? localPoint;

        target.RaiseEvent(new PointerEventArgs(
            routedEvent,
            target,
            pointer,
            root,
            rootPoint,
            1,
            properties,
            KeyModifiers.None));
    }

    // 以下三个 helper 原先定义在 DataGrid 套件中，DataGrid 套件移除后由本文件承接，
    // 供 AddOnDecoratedBox / PopupConfirm / DatePicker 套件共享（partial class Program）。
    private static HashSet<string> GetLocalRoutedHandlerNames(Control control)
    {
        var field = typeof(Interactive).GetField("_eventHandlers", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(control) is not IDictionary eventHandlers)
        {
            return [];
        }

        var names = new HashSet<string>();
        foreach (DictionaryEntry entry in eventHandlers)
        {
            if (entry.Key != null)
            {
                names.Add(entry.Key.ToString() ?? string.Empty);
            }
        }
        return names;
    }

    private static void RaiseControlPrimaryPointerPressed(Control target, Visual root)
    {
        var localPoint = new Point(
            Math.Max(1, target.Bounds.Width / 2),
            Math.Max(1, target.Bounds.Height / 2));
        var rootPoint = target.TranslatePoint(localPoint, root) ?? localPoint;

        target.RaiseEvent(CreatePrimaryPointerPressedEventArgs(target, root, rootPoint));
    }

    private static void RaiseControlPrimaryPointerReleased(Control target, Visual root)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.None,
            PointerUpdateKind.LeftButtonReleased);
        var localPoint = new Point(
            Math.Max(1, target.Bounds.Width / 2),
            Math.Max(1, target.Bounds.Height / 2));
        var rootPoint = target.TranslatePoint(localPoint, root) ?? localPoint;

        target.RaiseEvent(new PointerReleasedEventArgs(
            target,
            pointer,
            root,
            rootPoint,
            1,
            properties,
            KeyModifiers.None,
            MouseButton.Left));
    }

    private static PointerPressedEventArgs CreatePrimaryPointerPressedEventArgs(
        Control source,
        Visual root,
        Point rootPoint)
    {
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(),
            PointerType.Mouse,
            true);
        var properties = new PointerPointProperties(
            RawInputModifiers.LeftMouseButton,
            PointerUpdateKind.LeftButtonPressed);

        return new PointerPressedEventArgs(
            source,
            pointer,
            root,
            rootPoint,
            1,
            properties,
            KeyModifiers.None);
    }

    private static void VerifyCompactSpaceGeometry(ICollection<string> failures)
    {
        VerifyCompactSpaceGeometry(Orientation.Horizontal, failures);
        VerifyCompactSpaceGeometry(Orientation.Vertical, failures);
    }

    private static void VerifyCompactSpacePrimitiveStateGuards(ICollection<string> failures)
    {
        var item = new CompactSpaceItem
        {
            Child = new AtomUI.Desktop.Controls.Button { Content = "Item" }
        };
        var addon = new CompactSpaceAddOn
        {
            Content = "Addon"
        };

        VerifyRepeatedCompactSpaceAwareNotifications(item, "CompactSpaceItem", failures);
        VerifyRepeatedCompactSpaceAwareNotifications(addon, "CompactSpaceAddOn", failures);
        VerifyCompactSpaceItemTransformReuseAndClear(failures);
    }

    private static void VerifyRepeatedCompactSpaceAwareNotifications(Control control,
                                                                     string label,
                                                                     ICollection<string> failures)
    {
        var compactSpaceAware = (ICompactSpaceAware)control;
        var changedProperties = new List<string>();
        control.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name is "IsUsedInCompactSpace" or
                                   "CompactSpaceItemPosition" or
                                   "CompactSpaceOrientation")
            {
                changedProperties.Add(e.Property.Name);
            }
        };

        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
        compactSpaceAware.NotifyOrientationChange(Orientation.Vertical);
        changedProperties.Clear();
        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
        compactSpaceAware.NotifyOrientationChange(Orientation.Vertical);

        Expect(changedProperties.Count == 0,
            $"{label} repeated compact-space notifications should not rewrite unchanged state. Actual: {string.Join(", ", changedProperties)}.",
            failures);
    }

    private static void VerifyCompactSpaceItemTransformReuseAndClear(ICollection<string> failures)
    {
        var first = new AtomUI.Desktop.Controls.Button { Content = "First" };
        var middle = new AtomUI.Desktop.Controls.Button { Content = "Middle" };
        var last = new AtomUI.Desktop.Controls.Button { Content = "Last" };
        var compactSpace = new CompactSpace
        {
            Orientation = Orientation.Horizontal
        };
        compactSpace.Children.Add(first);
        compactSpace.Children.Add(middle);
        compactSpace.Children.Add(last);

        using var realized = RealizeControl(compactSpace);
        var items = compactSpace.GetSelfAndVisualDescendants()
                                .OfType<Control>()
                                .Where(control => control.GetType().FullName == "AtomUI.Desktop.Controls.CompactSpaceItem")
                                .ToList();
        Expect(items.Count == 3,
            $"CompactSpace transform verification should create three wrappers, actual {items.Count}.",
            failures);
        if (items.Count != 3)
        {
            return;
        }

        RefreshLayout(realized.Window);
        var middleTransform = items[1].RenderTransform;
        Expect(middleTransform is TranslateTransform,
            "Middle CompactSpaceItem should use a translate offset transform.",
            failures);

        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(middleTransform, items[1].RenderTransform),
            "Repeated CompactSpaceItem measure should reuse the existing offset transform.",
            failures);

        ((ICompactSpaceAware)items[1]).NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
        RefreshLayout(realized.Window);
        Expect(items[1].RenderTransform == null,
            "CompactSpaceItem should clear stale offset transform when it becomes a single item.",
            failures);
    }

    private static void VerifySelectRepeatedCompactSpaceAndFeedbackNotificationsAreStable(ICollection<string> failures)
    {
        var select = new Select
        {
            OptionsSource = CreateSelectOptions()
        };
        using var realized = RealizeControl(select);

        var changedProperties = new List<string>();
        select.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name is "IsUsedInCompactSpace" or
                                   "CompactSpaceItemPosition" or
                                   "CompactSpaceOrientation" or
                                   "Status" or
                                   "FormFeedback")
            {
                changedProperties.Add(e.Property.Name);
            }
        };

        var compactSpaceAware = (ICompactSpaceAware)select;
        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
        compactSpaceAware.NotifyOrientationChange(Orientation.Vertical);
        var feedback = new FormValidateFeedback();
        ((IFormItemAware)select).NotifyValidateStatus(FormValidateStatus.Error);
        ((IFormItemFeedbackAware)select).SetFeedbackControl(feedback);

        changedProperties.Clear();
        compactSpaceAware.NotifyPositionChange(SpaceItemPosition.First | SpaceItemPosition.Last);
        compactSpaceAware.NotifyOrientationChange(Orientation.Vertical);
        ((IFormItemAware)select).NotifyValidateStatus(FormValidateStatus.Error);
        ((IFormItemFeedbackAware)select).SetFeedbackControl(feedback);

        Expect(changedProperties.Count == 0,
            $"Select repeated compact-space/form-feedback notifications should not rewrite unchanged state. Actual: {string.Join(", ", changedProperties)}.",
            failures);
    }

    private static void VerifyInputShellRepeatedStateNotificationsAreStable(ICollection<string> failures)
    {
        VerifyTextBoxRepeatedStateNotificationsAreStable(failures);
        VerifyTextAreaRepeatedStateNotificationsAreStable(failures);
        VerifyButtonSpinnerRepeatedCompactSpaceNotificationsAreStable(failures);
        VerifyNumericUpDownRepeatedStateNotificationsAreStable(failures);
    }

    private static void VerifyTextBoxRepeatedStateNotificationsAreStable(ICollection<string> failures)
    {
        var textBox = new AtomTextBox();
        var feedback = new FormValidateFeedback();
        VerifyRepeatedCompactSpaceAwareNotifications(textBox, "TextBox", failures);
        VerifyRepeatedFeedbackNotification(textBox, "TextBox", feedback, failures);
    }

    private static void VerifyTextAreaRepeatedStateNotificationsAreStable(ICollection<string> failures)
    {
        var textArea = new TextArea();
        var feedback = new FormValidateFeedback();
        VerifyRepeatedStatusNotification(textArea, "TextArea", failures);
        VerifyRepeatedFeedbackNotification(textArea, "TextArea", feedback, failures);
    }

    private static void VerifyButtonSpinnerRepeatedCompactSpaceNotificationsAreStable(ICollection<string> failures)
    {
        var buttonSpinner = new AtomUI.Desktop.Controls.ButtonSpinner();
        VerifyRepeatedCompactSpaceAwareNotifications(buttonSpinner, "ButtonSpinner", failures);
    }

    private static void VerifyNumericUpDownRepeatedStateNotificationsAreStable(ICollection<string> failures)
    {
        var numericUpDown = new AtomUI.Desktop.Controls.NumericUpDown();
        VerifyRepeatedCompactSpaceAwareNotifications(numericUpDown, "NumericUpDown", failures);
        VerifyRepeatedStatusNotification(numericUpDown, "NumericUpDown", failures);
    }

    private static void VerifyRepeatedStatusNotification(Control control,
                                                         string label,
                                                         ICollection<string> failures)
    {
        var changedProperties = new List<string>();
        control.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name == "Status")
            {
                changedProperties.Add(e.Property.Name);
            }
        };

        ((IFormItemAware)control).NotifyValidateStatus(FormValidateStatus.Error);
        changedProperties.Clear();
        ((IFormItemAware)control).NotifyValidateStatus(FormValidateStatus.Error);

        Expect(changedProperties.Count == 0,
            $"{label} repeated validation status notification should not rewrite unchanged state. Actual: {string.Join(", ", changedProperties)}.",
            failures);
    }

    private static void VerifyRepeatedFeedbackNotification(Control control,
                                                           string label,
                                                           FormValidateFeedback feedback,
                                                           ICollection<string> failures)
    {
        var changedProperties = new List<string>();
        control.PropertyChanged += (_, e) =>
        {
            if (e.Property.Name == "FormFeedback")
            {
                changedProperties.Add(e.Property.Name);
            }
        };

        ((IFormItemFeedbackAware)control).SetFeedbackControl(feedback);
        changedProperties.Clear();
        ((IFormItemFeedbackAware)control).SetFeedbackControl(feedback);

        Expect(changedProperties.Count == 0,
            $"{label} repeated form-feedback notification should not rewrite unchanged state. Actual: {string.Join(", ", changedProperties)}.",
            failures);
    }

    private static void VerifyPenUtilsReusesEquivalentPen(ICollection<string> failures)
    {
        var pen = InvokePenUtilsTryModifyOrCreate(
            null,
            Brushes.Red,
            1.0,
            null,
            0.0,
            out var firstChanged);
        var firstPen = pen;
        pen = InvokePenUtilsTryModifyOrCreate(
            pen,
            Brushes.Red,
            1.0,
            null,
            0.0,
            out var secondChanged);
        Expect(firstChanged,
            "PenUtils first immutable brush call should create a pen.",
            failures);
        Expect(!secondChanged && ReferenceEquals(firstPen, pen),
            "PenUtils repeated immutable brush call should reuse the existing equivalent pen.",
            failures);

        var dashes = new[] { 4.0, 2.0 };
        pen = InvokePenUtilsTryModifyOrCreate(
            pen,
            Brushes.Red,
            1.0,
            dashes,
            0.0,
            out var dashedChanged);
        var dashedPen = pen;
        pen = InvokePenUtilsTryModifyOrCreate(
            pen,
            Brushes.Red,
            1.0,
            dashes,
            0.0,
            out var repeatedDashedChanged);
        Expect(dashedChanged,
            "PenUtils first dashed call should update the pen.",
            failures);
        Expect(!repeatedDashedChanged && ReferenceEquals(dashedPen, pen),
            "PenUtils repeated dashed call should reuse the existing equivalent pen.",
            failures);
    }

    private static IPen? InvokePenUtilsTryModifyOrCreate(IPen? pen,
                                                         IBrush? brush,
                                                         double thickness,
                                                         IReadOnlyList<double>? strokeDashArray,
                                                         double strokeDashOffset,
                                                         out bool changed)
    {
        var method = typeof(PenUtils).GetMethod(
            "TryModifyOrCreate",
            BindingFlags.Static | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new InvalidOperationException("PenUtils.TryModifyOrCreate should exist.");
        }

        object?[] args =
        [
            pen,
            brush,
            thickness,
            strokeDashArray,
            strokeDashOffset,
            PenLineCap.Flat,
            PenLineJoin.Miter,
            10.0
        ];
        changed = method.Invoke(null, args) is true;
        return args[0] as IPen;
    }

    private static void VerifyCompactSpaceGeometry(Orientation orientation, ICollection<string> failures)
    {
        var compactSpace = CreateCompactSpace(orientation);
        using var _ = RealizeControl(compactSpace);
        var boxes = compactSpace.GetSelfAndVisualDescendants().OfType<AddOnDecoratedBox>().ToList();

        Expect(boxes.Count == 3, $"CompactSpace {orientation} should have three AddOnDecoratedBox instances, actual {boxes.Count}.", failures);
        if (boxes.Count != 3)
        {
            return;
        }

        Expect(boxes.All(box => box.IsUsedInCompactSpace), $"CompactSpace {orientation} should mark all boxes as used in compact space.", failures);
        Expect(boxes.All(box => box.CompactSpaceOrientation == orientation), $"CompactSpace {orientation} should relay orientation to all boxes.", failures);
        Expect(boxes[0].CompactSpaceItemPosition?.HasFlag(SpaceItemPosition.First) == true,
            $"CompactSpace {orientation} first item should be marked First.", failures);
        Expect(boxes[1].CompactSpaceItemPosition?.HasFlag(SpaceItemPosition.Middle) == true,
            $"CompactSpace {orientation} middle item should be marked Middle.", failures);
        Expect(boxes[2].CompactSpaceItemPosition?.HasFlag(SpaceItemPosition.Last) == true,
            $"CompactSpace {orientation} last item should be marked Last.", failures);

        if (orientation == Orientation.Horizontal)
        {
            Expect(boxes[0].InnerBoxCornerRadius.TopRight == 0 && boxes[0].InnerBoxCornerRadius.BottomRight == 0,
                $"Horizontal first box should cut right corners, actual {boxes[0].InnerBoxCornerRadius}.", failures);
            Expect(IsZeroCornerRadius(boxes[1].InnerBoxCornerRadius),
                $"Horizontal middle box should cut all corners, actual {boxes[1].InnerBoxCornerRadius}.", failures);
            Expect(boxes[2].InnerBoxCornerRadius.TopLeft == 0 && boxes[2].InnerBoxCornerRadius.BottomLeft == 0,
                $"Horizontal last box should cut left corners, actual {boxes[2].InnerBoxCornerRadius}.", failures);
        }
        else
        {
            Expect(boxes[0].InnerBoxCornerRadius.BottomLeft == 0 && boxes[0].InnerBoxCornerRadius.BottomRight == 0,
                $"Vertical first box should cut bottom corners, actual {boxes[0].InnerBoxCornerRadius}.", failures);
            Expect(IsZeroCornerRadius(boxes[1].InnerBoxCornerRadius),
                $"Vertical middle box should cut all corners, actual {boxes[1].InnerBoxCornerRadius}.", failures);
            Expect(boxes[2].InnerBoxCornerRadius.TopLeft == 0 && boxes[2].InnerBoxCornerRadius.TopRight == 0,
                $"Vertical last box should cut top corners, actual {boxes[2].InnerBoxCornerRadius}.", failures);
        }
    }

    private static void VerifyRightAddOnTemplateBindings(ICollection<string> failures)
    {
        VerifyRightAddOnTemplateBinding(new Select { Width = 260 }, "Select", failures);
        VerifyRightAddOnTemplateBinding(new TreeSelect { Width = 260 }, "TreeSelect", failures);
        VerifyRightAddOnTemplateBinding(new Cascader { Width = 260 }, "Cascader", failures);
    }

    private static void VerifyRightAddOnTemplateBinding(AbstractSelect select, string label, ICollection<string> failures)
    {
        var leftTemplate  = new MarkerDataTemplate($"{label}-left");
        var rightTemplate = new MarkerDataTemplate($"{label}-right");
        select.SetCurrentValue(AbstractSelect.ContentLeftAddOnTemplateProperty, leftTemplate);
        select.SetCurrentValue(AbstractSelect.ContentRightAddOnProperty, $"{label}-content");
        select.SetCurrentValue(AbstractSelect.ContentRightAddOnTemplateProperty, rightTemplate);

        using var realized = RealizeControl(select);
        var presenter = FindVisualByName<ContentPresenter>(select, "PART_ContentRightAddOnPresenter");
        Expect(presenter != null, $"{label} should create PART_ContentRightAddOnPresenter.", failures);
        Expect(ReferenceEquals(presenter?.ContentTemplate, rightTemplate),
            $"{label} right addon presenter should bind ContentRightAddOnTemplate.", failures);

        var updatedTemplate = new MarkerDataTemplate($"{label}-right-updated");
        select.SetCurrentValue(AbstractSelect.ContentRightAddOnTemplateProperty, updatedTemplate);
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(presenter?.ContentTemplate, updatedTemplate),
            $"{label} right addon presenter should update when ContentRightAddOnTemplate changes.", failures);
    }
}
