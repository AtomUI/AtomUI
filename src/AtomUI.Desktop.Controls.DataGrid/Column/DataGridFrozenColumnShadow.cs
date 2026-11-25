using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Paints the frozen column boundary shadow on the strip outside a frozen cell or header,
/// matching the <c>::after</c> strip of Ant Design's fixed columns.
/// </summary>
/// <remarks>
/// Drawn through <see cref="BoxShadowRenderHelper" />: Avalonia applies box shadow offsets in device pixels,
/// so the Ant Design shadow <c>inset ±10px 0 8px -8px</c> would lose its solid band and almost disappear at
/// 150% / 200% scaling.
/// </remarks>
internal class DataGridFrozenColumnShadow : Control
{
    #region 内部属性定义

    internal static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<DataGridFrozenColumnShadow>();

    internal BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    #endregion

    static DataGridFrozenColumnShadow()
    {
        AffectsRender<DataGridFrozenColumnShadow>(BoxShadowProperty);
    }

    public override void Render(DrawingContext context)
    {
        if (BoxShadow.Count == 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            return;
        }

        BoxShadowRenderHelper.DrawRectangle(context, null, null, new RoundedRect(new Rect(Bounds.Size)), BoxShadow);
    }
}
