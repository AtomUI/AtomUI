using AtomUI.Controls;
using AtomUI.Generated.AtomUIDesktopControls;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.LogicalTree;

namespace AtomUI.Desktop.Controls;

public class FlyoutPresenter : ArrowDecoratedBox
{
    // 我们在这里并没有增加任何元素或者样式
    protected override Type StyleKeyOverride => typeof(ArrowDecoratedBox);

    public FlyoutPresenter()
    {
        SetValue(CursorProperty, new Cursor(StandardCursorType.Arrow), BindingPriority.Template);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        // Flyout 弹层是跨视觉根的代码创建节点，标记类无法在共享 ArrowDecoratedBox 主题上
        // 静态声明（会污染其他 ArrowDecoratedBox 弹层），因此按 InfoFlyout 语义部件契约
        // 在模板应用时注入。
        e.NameScope.Get<Border>("PART_ContentDecorator")
                  .Classes.Add(FlyoutHostSemanticParts.PopupContainerClass);
        e.NameScope.Get<ContentPresenter>("ContentPresenter")
                  .Classes.Add(FlyoutHostSemanticParts.PopupContentClass);
        e.NameScope.Get<ArrowIndicator>("PART_ArrowIndicator")
                  .Classes.Add(FlyoutHostSemanticParts.PopupArrowClass);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            var host = this.FindLogicalAncestorOfType<Popup>();
            if (host != null)
            {
                host.IsOpen = false;
                e.Handled   = true;
            }
        }

        base.OnKeyDown(e);
    }
}
