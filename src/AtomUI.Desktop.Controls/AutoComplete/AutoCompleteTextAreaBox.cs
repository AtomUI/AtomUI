using AtomUI.Desktop.Controls.Themes;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class AutoCompleteTextAreaBox : TextArea
{
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AbstractAutoComplete.IsDropDownOpenProperty.AddOwner<AutoCompleteTextAreaBox>();
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        e.Handled = false;
    }
    
    private TextPresenter? _textPresenter;
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _textPresenter = e.NameScope.Find<TextPresenter>(TextAreaThemeConstants.TextPresenterPart);
    }

    internal Rect GetTextPresenterBounds()
    {
        if (_textPresenter != null)
        {
            var offset = _textPresenter.TranslatePoint(new Point(0, 0), this) ?? new Point(0, 0);
            return new Rect(offset.X, offset.Y, _textPresenter.DesiredSize.Width, _textPresenter.DesiredSize.Height);
        }
        return default;
    }

    internal Rect GetCaretBounds(Control? relativeControl = null)
    {
        var presenter   = this.GetTextPresenter();
        var textLayout  = presenter.TextLayout;
        var bounds = textLayout.HitTestTextPosition(CaretIndex);
        if (relativeControl != null)
        {
            var offset = presenter.TranslatePoint(bounds.Position, relativeControl) ?? bounds.Position;
            bounds = new Rect(offset, bounds.Size);
        }
        return bounds;
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Up || e.Key == Key.Down)
        {
            if (IsDropDownOpen)
            {
                return;
            }
        }
        base.OnKeyDown(e);
    }
}