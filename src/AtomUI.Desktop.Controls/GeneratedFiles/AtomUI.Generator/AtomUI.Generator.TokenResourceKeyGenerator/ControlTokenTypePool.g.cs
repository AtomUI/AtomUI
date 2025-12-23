using System.Collections.Generic;
using AtomUI.Theme;

namespace AtomUI.Theme
{
    internal class ControlTokenTypePool
    {
        internal static IList<Type> GetTokenTypes()
        {
            List<Type> tokenTypes = new List<Type>();
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.AddOnDecoratedBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.AdornerLayerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.AlertToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ArrowDecoratedBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.AvatarToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.BadgeToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.BreadcrumbToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ButtonSpinnerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ButtonToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.CalendarToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.CardToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.CarouselToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.CheckBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ChromeToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.CollapseToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ComboBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.DatePickerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.DescriptionsToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.DialogToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.DrawerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.EmptyToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ExpanderToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.FlyoutPresenterToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.GroupBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ImagePreviewerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.LineEditToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ListToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.MarqueeLabelToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.MenuToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.MessageBoxToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.MessageToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.NavMenuToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.NotificationToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.NumericUpDownToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.OptionButtonToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.PaginationToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.PopupConfirmToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.Primitives.IndicatorScrollViewerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.Primitives.InfoPickerInputToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ProgressBarToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.QRCodeToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.RadioButtonToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.RateToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ResultToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ScrollBarToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SegmentedToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SelectToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SeparatorToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SkeletonToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SliderToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SpinToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.SplitViewToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.StatisticToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.StepsToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TabControlToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TagToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TextAreaToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TimelineToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TimePickerToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ToggleSwitchToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.ToolTipToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TreeFlyoutToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.TreeViewToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.UploadToken));
            tokenTypes.Add(typeof(AtomUI.Desktop.Controls.WindowToken));
            return tokenTypes;
        }
    }
}