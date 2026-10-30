using Avalonia;
using Avalonia.Media;

namespace AtomUI.Controls;

internal interface IShadowMaskInfoProvider
{
    CornerRadius GetMaskCornerRadius();
    Rect GetMaskBounds();
    IBrush? GetMaskBackground();
}

internal interface IArrowAwareShadowMaskInfoProvider : IShadowMaskInfoProvider
{
    bool IsArrowVisible();
    ArrowPosition GetArrowPosition();
    Rect GetArrowIndicatorBounds();
    Rect GetArrowIndicatorLayoutBounds();
    void SetArrowOpacity(double opacity);

    /// <summary>
    /// 返回模板中的箭头装饰盒。提供方的模板尚未应用、或当前主题没有提供
    /// <see cref="AbstractArrowDecoratedBox.ArrowDecoratorPart"/> 部件时返回 null；
    /// 消费者必须按无箭头降级处理，并在部件后续就绪时重新探测。
    /// </summary>
    AbstractArrowDecoratedBox? GetArrowDecoratedBox();
}