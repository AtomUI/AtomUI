using AtomUI.Controls;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Desktop.Controls.Primitives;
using AtomUI.Desktop.Controls.Primitives.DesignTokens;
using AtomUI.Theme;
using AtomUI.Theme.Configuration;
using AtomUI.Theme.Schema;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Diagnostics;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Shouldly;
using SkiaSharp;
using Xunit;

namespace AtomUI.Desktop.Controls.Rendering.Tests.Effect.Families;

/// <summary>
/// Scope isolation of the data-entry remainder (theme-effect-tests.md, family gate): a <c>Controls[X]</c> override reaches X
/// only, the root Token reaches X, the Token-owner transparent parts (FormItem, FormItemDecorator, SegmentedItem,
/// UploadTrigger, UploadDefaultDropArea, UploadDropZone, CandidateList, CandidateListItem) follow their host, and registered
/// controls nested in a member (form inputs, the Mentions TextArea, upload actions and progress bars) keep their identity.
/// </summary>
public partial class DataEntryFamilyEffectTests
{
    private const string IsolationColor = "#ff00b96b";

    private static ThemeConfig IsolationConfig(
        (ControlTokenIdentity Identity, string Token, string Value)? control = null,
        (string Token, string Value)? root = null) =>
        FeedbackFamilyEffectTests.IsolationConfig(control, root);

    private static void WithIsolationWindow(Control content, Action<ThemeConfigProvider, Func<SKBitmap>> body) =>
        FeedbackFamilyEffectTests.WithIsolationWindow(content, body);

    private static double ChangedRatioOf(SKBitmap before, SKBitmap after, Control control) =>
        FeedbackFamilyEffectTests.ChangedRatioOf(before, after, control);

    private static void AssertReachesOnly(
        ThemeConfigProvider provider,
        Func<SKBitmap> capture,
        (ControlTokenIdentity Identity, string Token, Control Control)[] members)
    {
        foreach (var (identity, token, target) in members)
        {
            provider.Config = IsolationConfig();
            using var before = capture();
            provider.Config = IsolationConfig(control: (identity, token, IsolationColor));
            using var after = capture();
            foreach (var (_, _, control) in members)
            {
                var ratio = ChangedRatioOf(before, after, control);
                if (ReferenceEquals(control, target))
                {
                    ratio.ShouldBeGreaterThan(0d, $"Controls[{identity}].{token} did not reach {control.GetType().Name}.");
                }
                else
                {
                    ratio.ShouldBe(0d, $"Controls[{identity}].{token} leaked into {control.GetType().Name}.");
                }
            }

            provider.Config = IsolationConfig(root: (token, IsolationColor));
            using var byRoot = capture();
            ChangedRatioOf(before, byRoot, target).ShouldBeGreaterThan(0d, $"Root {token} did not reach {target.GetType().Name}.");
        }
    }

    [Fact]
    public void Controls_X_Colours_Change_Only_X_For_Data_Entry_Members()
    {
        var form         = CreateForm();
        var descriptions = CreateDescriptions(bordered: true);
        var upload       = CreateUpload();
        var otp          = CreateOtp();
        var segmented    = CreateSegmented();
        var left  = new StackPanel { Spacing = 12, Children = { form, descriptions } };
        var right = new StackPanel { Spacing = 12, Children = { upload, otp, segmented } };
        var root  = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 16, Children = { left, right } };
        form.Width         = 360;
        descriptions.Width = 360;
        upload.Width       = 300;
        WithIsolationWindow(root, (provider, capture) =>
        {
            AssertReachesOnly(provider, capture,
            [
                (FormTokens.Identity, "ColorTextDescription", form),
                (DescriptionsTokens.Identity, "ColorSplit", descriptions),
                (UploadTokens.Identity, "ColorFillSecondary", upload),
                (OtpLineEditTokens.Identity, "ColorBorder", otp),
                (SegmentedTokens.Identity, "ColorTextDisabled", segmented)
            ]);
        });
    }

    [Fact]
    public void Root_Global_Colour_Reaches_Two_Independent_Members()
    {
        var form = CreateForm();
        var upload = CreateUpload(statuses: [FileUploadStatus.Failed]);
        WithIsolationWindow(new StackPanel { Children = { form, upload } }, (provider, capture) =>
        {
            using var before = capture();
            provider.Config = IsolationConfig(control: (FormTokens.Identity, "ColorError", IsolationColor));
            using var byForm = capture();
            ChangedRatioOf(before, byForm, form).ShouldBeGreaterThan(0d);
            ChangedRatioOf(before, byForm, upload).ShouldBe(0d);
            provider.Config = IsolationConfig(root: ("ColorError", IsolationColor));
            using var byRoot = capture();
            ChangedRatioOf(before, byRoot, form).ShouldBeGreaterThan(0d);
            ChangedRatioOf(before, byRoot, upload).ShouldBeGreaterThan(0d);
        });
    }

    [Fact]
    public void Form_Parts_Follow_The_Form_And_Its_Inputs_Keep_Their_Identity()
    {
        var form = CreateForm();
        WithIsolationWindow(form, (provider, capture) =>
        {
            var item      = FormItemAt(form, 1);
            var decorator = Descendant<FormItemDecorator>(form);
            var input     = Descendant<LineEdit>(item);
            var label     = LabelOf(form, 1);
            ThemeScope.GetTokenOwner(item).ShouldBe(FormTokens.Identity);
            ThemeScope.GetTokenOwner(decorator).ShouldBe(FormTokens.Identity);
            ThemeScope.GetTokenOwner(label).ShouldBe(FormTokens.Identity);
            ThemeScope.GetTokenOwner(input).ShouldBe(LineEditTokens.Identity);
            using var before = capture();

            // FormItem and FormItemDecorator are transparent: their own identities reach nothing.
            provider.Config = IsolationConfig(control: (FormItemTokens.Identity, "ColorTextDescription", IsolationColor));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, form).ShouldBe(0d, "Controls[FormItem] reached a transparent form item.");
            provider.Config = IsolationConfig(control: (FormItemDecoratorTokens.Identity, "SpacingXS", "40"));
            using var byDecorator = capture();
            ChangedRatioOf(before, byDecorator, form).ShouldBe(0d, "Controls[FormItemDecorator] reached a transparent decorator.");
            provider.Config = IsolationConfig(control: (FormTokens.Identity, "SpacingXS", "40"));
            using var byFormSpacing = capture();
            ChangedRatioOf(before, byFormSpacing, decorator).ShouldBeGreaterThan(0d, "Controls[Form] did not reach the decorator.");

            // The help text follows the Form; the line edit keeps its identity.
            provider.Config = IsolationConfig(control: (FormTokens.Identity, "ColorBorder", IsolationColor));
            using var byFormBorder = capture();
            ChangedRatioOf(before, byFormBorder, input).ShouldBe(0d, "Controls[Form].ColorBorder reached the nested LineEdit.");
            provider.Config = IsolationConfig(control: (LineEditTokens.Identity, "ColorBorder", IsolationColor));
            using var byLineEdit = capture();
            ChangedRatioOf(before, byLineEdit, input).ShouldBeGreaterThan(0d, "Controls[LineEdit] did not reach the form input.");
        });
    }

    [Fact]
    public void Form_Submit_And_Reset_Buttons_Use_Their_Own_Terminal_Tokens()
    {
        // SubmitButton / ResetButton are independent terminals (AbstractStandardButtonToken): their shared ButtonTheme
        // reads the terminal's own effective globals, so Controls[Button] no longer styles them.
        var submit = new SubmitButton();
        var reset  = new ResetButton();
        var root   = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, Children = { submit, reset } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            ThemeScope.GetTokenOwner(submit).ShouldBe(SubmitButtonTokens.Identity);
            ThemeScope.GetTokenOwner(reset).ShouldBe(ResetButtonTokens.Identity);
            using var before = capture();
            provider.Config = IsolationConfig(control: (ButtonTokens.Identity, "ColorPrimary", IsolationColor));
            using var byButton = capture();
            ChangedRatioOf(before, byButton, submit).ShouldBe(0d, "Controls[Button] leaked into SubmitButton.");
            provider.Config = IsolationConfig(control: (SubmitButtonTokens.Identity, "ColorPrimary", IsolationColor));
            using var bySubmit = capture();
            ChangedRatioOf(before, bySubmit, submit).ShouldBeGreaterThan(0d, "Controls[SubmitButton] did not reach SubmitButton.");
            ChangedRatioOf(before, bySubmit, reset).ShouldBe(0d, "Controls[SubmitButton] leaked into ResetButton.");
            provider.Config = IsolationConfig(control: (ButtonTokens.Identity, "ColorBorder", IsolationColor));
            using var byButtonBorder = capture();
            ChangedRatioOf(before, byButtonBorder, reset).ShouldBe(0d, "Controls[Button] leaked into ResetButton.");
            provider.Config = IsolationConfig(control: (ResetButtonTokens.Identity, "ColorBorder", IsolationColor));
            using var byReset = capture();
            ChangedRatioOf(before, byReset, reset).ShouldBeGreaterThan(0d, "Controls[ResetButton] did not reach ResetButton.");
        });
    }

    [Fact]
    public void Segmented_Items_Follow_The_Segmented()
    {
        var segmented = CreateSegmented();
        WithIsolationWindow(segmented, (provider, capture) =>
        {
            var disabled = SegmentedItemAt(segmented, 3);
            ThemeScope.GetTokenOwner(disabled).ShouldBe(SegmentedTokens.Identity);
            using var before = capture();
            provider.Config = IsolationConfig(control: (SegmentedItemTokens.Identity, "ColorTextDisabled", IsolationColor));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, segmented).ShouldBe(0d, "Controls[SegmentedItem] reached a transparent item.");
            provider.Config = IsolationConfig(control: (SegmentedTokens.Identity, "ColorTextDisabled", IsolationColor));
            using var bySegmented = capture();
            ChangedRatioOf(before, bySegmented, disabled).ShouldBeGreaterThan(0d, "Controls[Segmented] did not reach the item.");
        });
    }

    [Fact]
    public void Upload_Parts_Follow_The_Upload_And_Nested_Controls_Keep_Their_Identity()
    {
        var textList = CreateUpload(statuses: [FileUploadStatus.Success, FileUploadStatus.Uploading]);
        var dragger  = CreateUpload(dragger: true);
        var root = new StackPanel { Spacing = 12, Children = { textList, dragger } };
        WithIsolationWindow(root, (provider, capture) =>
        {
            var trigger   = Descendant<UploadTrigger>(textList);
            var header    = TextHeaderAt(textList, 0);
            var progress  = textList.GetVisualDescendants().OfType<ProgressBar>().First(static bar => bar.IsEffectivelyVisible);
            var deleteBtn = Descendant<IconButton>(header);
            var zone      = Descendant<UploadDropZone>(dragger);
            var area      = (UploadDefaultDropArea)DropArea(dragger);
            ThemeScope.GetTokenOwner(trigger).ShouldBe(UploadTokens.Identity);
            ThemeScope.GetTokenOwner(header).ShouldBe(UploadTokens.Identity);
            ThemeScope.GetTokenOwner(zone).ShouldBe(UploadTokens.Identity);
            ThemeScope.GetTokenOwner(area).ShouldBe(UploadTokens.Identity);
            ThemeScope.GetTokenOwner(progress).ShouldBe(ProgressBarTokens.Identity);
            ThemeScope.GetTokenOwner(deleteBtn).ShouldBe(IconButtonTokens.Identity);
            using var before = capture();

            // The transparent drag area: its own identity reaches nothing, the Upload reaches its icon.
            provider.Config = IsolationConfig(control: (UploadDefaultDropAreaTokens.Identity, "ColorPrimary", IsolationColor));
            using var byArea = capture();
            ChangedRatioOf(before, byArea, area).ShouldBe(0d, "Controls[UploadDefaultDropArea] reached a transparent drop area.");
            provider.Config = IsolationConfig(control: (UploadTriggerTokens.Identity, "ColorTextSecondary", IsolationColor));
            using var byTrigger = capture();
            ChangedRatioOf(before, byTrigger, trigger).ShouldBe(0d, "Controls[UploadTrigger] reached a transparent trigger.");
            provider.Config = IsolationConfig(control: (UploadTokens.Identity, "ColorPrimary", IsolationColor));
            using var byUpload = capture();
            ChangedRatioOf(before, byUpload, area).ShouldBeGreaterThan(0d, "Controls[Upload] did not reach the drop area.");
            ChangedRatioOf(before, byUpload, progress).ShouldBeGreaterThan(0d, "Controls[Upload] did not reach the progress line.");
            provider.Config = IsolationConfig(control: (UploadTokens.Identity, "ColorTextSecondary", IsolationColor));
            using var byUploadText = capture();
            ChangedRatioOf(before, byUploadText, trigger).ShouldBeGreaterThan(0d, "Controls[Upload] did not reach the trigger.");

            // The progress bar keeps its identity for its own reads (its groove and stroke are set by the Upload theme).
            provider.Config = IsolationConfig(control: (ProgressBarTokens.Identity, "ColorPrimary", IsolationColor));
            using var byProgressBar = capture();
            ChangedRatioOf(before, byProgressBar, header).ShouldBe(0d, "Controls[ProgressBar] reached the upload item header.");
        });
    }

    [Fact]
    public void Upload_Item_Motion_Follows_The_Upload_Tokens()
    {
        // The text list items sit in the list's registered ScrollViewer but are logical children of the list, so their
        // TokenTransitions take the Upload's durations.
        var upload = CreateUpload();
        WithProvider(upload, provider =>
        {
            var header = TextHeaderAt(upload, 0);
            WaitFor(() => header.Transitions?.OfType<TransitionBase>().Any() == true);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(UploadTokens.Identity, "MotionDurationMid", "00:00:00.4000000");
            WaitFor(() => header.Transitions!.OfType<TransitionBase>().All(static t => t.Duration == TimeSpan.FromMilliseconds(400)));
            header.Transitions!.OfType<TransitionBase>()
                  .ShouldAllBe(static transition => transition.Duration == TimeSpan.FromMilliseconds(400));
        });
    }

    [Fact]
    public void Segmented_Item_Motion_Follows_The_Segmented_Tokens()
    {
        var segmented = CreateSegmented();
        WithProvider(segmented, provider =>
        {
            var item = SegmentedItemAt(segmented, 0);
            WaitFor(() => item.Transitions?.OfType<TransitionBase>().Any() == true);
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(SegmentedTokens.Identity, "MotionDurationMid", "00:00:00.4000000");
            WaitFor(() => item.Transitions!.OfType<TransitionBase>().All(static t => t.Duration == TimeSpan.FromMilliseconds(400)));
            item.Transitions!.OfType<TransitionBase>()
                .ShouldAllBe(static transition => transition.Duration == TimeSpan.FromMilliseconds(400));
        });
    }

    [Fact]
    public void Otp_Cells_And_Clear_Button_Follow_The_OtpLineEdit()
    {
        var otp = CreateOtp(clear: true);
        WithIsolationWindow(otp, (provider, capture) =>
        {
            var cell  = OtpCellAt(otp, 0);
            var clear = Descendant<Control>(otp, "PART_ClearButton");
            ThemeScope.GetTokenOwner(cell).ShouldBe(OtpLineEditTokens.Identity);
            ThemeScope.GetTokenOwner(clear).ShouldBe(OtpLineEditTokens.Identity);
            using var before = capture();
            provider.Config = IsolationConfig(control: (OtpLineEditTokens.Identity, "ColorTextQuaternary", IsolationColor));
            using var byOtp = capture();
            ChangedRatioOf(before, byOtp, clear).ShouldBeGreaterThan(0d, "Controls[OtpLineEdit] did not reach the clear button.");
            provider.Config = IsolationConfig(control: (LineEditTokens.Identity, "ColorBorder", IsolationColor));
            using var byLineEdit = capture();
            ChangedRatioOf(before, byLineEdit, otp).ShouldBe(0d, "Controls[LineEdit] reached the OTP cells.");
        });
    }

    [Fact]
    public void Root_CellWidth_Uses_The_Theme_And_Survives_Size_And_Token_Changes()
    {
        var otp = CreateOtp();
        otp.CellWidth = 58;
        WithProvider(otp, provider =>
        {
            var cell = OtpCellAt(otp, 0);
            cell.Width.ShouldBe(58);
            cell.GetDiagnostic(Avalonia.Layout.Layoutable.WidthProperty).Priority.ShouldNotBe(BindingPriority.LocalValue);
            otp.SizeType = CustomizableSizeType.Large;
            provider.Config = FeedbackFamilyEffectTests.WithControlToken(OtpLineEditTokens.Identity, "CellWidthLG", "70");
            WaitFor(() => cell.Width == 58);
            cell.Width.ShouldBe(58);
            otp.CellWidth = null;
            WaitFor(() => cell.Width == 70);
            cell.Width.ShouldBe(70);
        });
    }

    [Fact]
    public void Root_BorderBrush_Is_Projected_Onto_Every_Otp_Cell_In_Every_State()
    {
        // The root (or CellBorderBrush) border brush wins over the cell state brushes through the input-family HostBorderBrush
        // projection, not through a local BorderBrush value on the cell (ATOMUIGEN047).
        var otp     = CreateOtp();
        var magenta = Avalonia.Media.Color.Parse("#ffff00ff");
        var lime    = Avalonia.Media.Color.Parse("#ff00ff00");
        otp.BorderBrush = new Avalonia.Media.SolidColorBrush(magenta);
        WithIsolationWindow(otp, (_, _) =>
        {
            var cell = (OtpLineEditCell)OtpCellAt(otp, 0);
            static Avalonia.Media.Color? ColorOf(Avalonia.Media.IBrush? brush) => (brush as Avalonia.Media.ISolidColorBrush)?.Color;
            WaitFor(() => ColorOf(cell.BorderBrush) == magenta);
            ColorOf(cell.BorderBrush).ShouldBe(magenta);
            cell.GetDiagnostic(Avalonia.Controls.Primitives.TemplatedControl.BorderBrushProperty).Priority
                .ShouldNotBe(BindingPriority.LocalValue, "The root brush must not be written as a local BorderBrush value.");
            ColorOf(cell.HostBorderBrush).ShouldBe(magenta);

            // The projection also wins over the hover and pressed state brushes.
            Pseudo(cell, ":pointerover");
            Pseudo(cell, ":pressed");
            WaitFor(() => ColorOf(cell.BorderBrush) == magenta);
            ColorOf(cell.BorderBrush).ShouldBe(magenta);

            otp.CellBorderBrush = new Avalonia.Media.SolidColorBrush(lime);
            WaitFor(() => ColorOf(cell.BorderBrush) == lime);
            ColorOf(cell.BorderBrush).ShouldBe(lime);

            otp.ClearValue(OtpLineEdit.CellBorderBrushProperty);
            otp.ClearValue(Avalonia.Controls.Primitives.TemplatedControl.BorderBrushProperty);
            WaitFor(() => cell.HostBorderBrush is null);
            cell.HostBorderBrush.ShouldBeNull();
            ColorOf(cell.BorderBrush).ShouldNotBe(magenta);
        });
    }

    [Fact]
    public void Mentions_Candidate_List_And_Internal_TextArea_Follow_The_Mentions()
    {
        var host     = CreateMentionsHost();
        var mentions = MentionsOf(host);
        WithIsolationWindow(host, (provider, capture) =>
        {
            // The input is a nested TextArea: Controls[TextArea] styles it, Controls[Mentions] does not. Checked before the
            // popup opens, because the popup opens over the input at the trigger line.
            var textArea = Descendant<TextArea>(mentions);
            ThemeScope.GetTokenOwner(textArea).ShouldBe(MentionsTokens.Identity);
            using var closed = capture();
            provider.Config = IsolationConfig(control: (MentionsTokens.Identity, "ColorBorder", IsolationColor));
            using var byMentionsBorder = capture();
            ChangedRatioOf(closed, byMentionsBorder, textArea).ShouldBeGreaterThan(0d, "Controls[Mentions].ColorBorder did not reach its internal input.");
            provider.Config = IsolationConfig(control: (TextAreaTokens.Identity, "ColorBorder", IsolationColor));
            using var byTextArea = capture();
            ChangedRatioOf(closed, byTextArea, textArea).ShouldBe(0d, "Controls[TextArea] leaked into the Mentions input.");
            provider.Config = IsolationConfig();

            OpenMentions(mentions);
            var list  = TopLevel.GetTopLevel(mentions)!.GetVisualDescendants().OfType<CandidateList>().First();
            var item  = list.GetVisualDescendants().OfType<CandidateListItem>().First();
            var frame = MentionsPopupFrame(mentions);
            ThemeScope.GetTokenOwner(list).ShouldBe(MentionsTokens.Identity);
            ThemeScope.GetTokenOwner(item).ShouldBe(MentionsTokens.Identity);
            using var before = capture();

            // CandidateList and CandidateListItem are transparent: their own identities reach nothing; the Mentions does.
            provider.Config = IsolationConfig(control: (CandidateListTokens.Identity, "ControlHeight", "48"));
            using var byList = capture();
            ChangedRatioOf(before, byList, frame).ShouldBe(0d, "Controls[CandidateList] reached the transparent list.");
            provider.Config = IsolationConfig(control: (CandidateListItemTokens.Identity, "ControlHeight", "48"));
            using var byItem = capture();
            ChangedRatioOf(before, byItem, frame).ShouldBe(0d, "Controls[CandidateListItem] reached a transparent item.");
            provider.Config = IsolationConfig(control: (MentionsTokens.Identity, "ControlHeight", "48"));
            using var byMentions = capture();
            ChangedRatioOf(before, byMentions, frame).ShouldBeGreaterThan(0d, "Controls[Mentions] did not reach the candidate items.");

            // Borrowing (Task 10.10d): the candidate items read the ListBox Own Tokens.
            provider.Config = IsolationConfig();
            list.SelectedIndex = 0;
            using var selected = capture();
            provider.Config = IsolationConfig(control: (ListBoxTokens.Identity, "ItemSelectedBgColor", IsolationColor));
            using var byListBox = capture();
            ChangedRatioOf(selected, byListBox, item).ShouldBe(0d, "Controls[ListBox].ItemSelectedBgColor leaked into the candidate.");
            list.IsShowSelectedIndicator = true;
            provider.Config = IsolationConfig();
            using var indicatorBaseline = capture();
            var indicator = Descendant<IconTemplatePresenter>(item, "SelectedIndicator");
            var margin = indicator.Margin;
            provider.Config = IsolationConfig(control: (ListBoxTokens.Identity, "SelectedIndicatorMargin", "24,0,0,0"));
            using var sourceMargin = capture();
            indicator.Margin.ShouldBe(margin, "ListBox indicator Own leaked into its transparent candidate consumer.");
            provider.Config = IsolationConfig(control: (MentionsTokens.Identity, "MarginXXS", "24"));
            using var hostMargin = capture();
            indicator.Margin.ShouldBe(new Avalonia.Thickness(24, 0, 0, 0));
        });
    }

    [Fact]
    public void AutoComplete_Candidate_List_Follows_The_AutoComplete()
    {
        // CandidateList is Token-owner transparent for every host: the AutoComplete candidates take the AutoComplete owner.
        var autoComplete = new AutoComplete
        {
            Width             = 240,
            IsPopupPinnedOpen = true,
            OptionsSource =
            [
                new AutoCompleteOption { Header = "Alpha", Content = "Alpha" },
                new AutoCompleteOption { Header = "Alpine", Content = "Alpine" }
            ]
        };
        var host = FeedbackFamilyEffectTests.OverlayLayerHost(autoComplete, 420, 360);
        WithIsolationWindow(host, (provider, capture) =>
        {
            var input = autoComplete.GetVisualDescendants().OfType<AbstractTextInput>().First();
            input.Text       = "Al";
            input.CaretIndex = 2;
            var item = FeedbackFamilyEffectTests.WaitForVisual(
                () => TopLevel.GetTopLevel(autoComplete)!.GetVisualDescendants().OfType<CandidateListItem>().FirstOrDefault(),
                "The AutoComplete candidate popup");
            ThemeScope.GetTokenOwner(item).ShouldBe(AutoCompleteTokens.Identity);
            ThemeScope.GetTokenOwner(item.GetVisualAncestors().OfType<CandidateList>().First()).ShouldBe(AutoCompleteTokens.Identity);
            using var before = capture();
            provider.Config = IsolationConfig(control: (AutoCompleteTokens.Identity, "ControlHeight", "48"));
            using var after = capture();
            ChangedRatioOf(before, after, item).ShouldBeGreaterThan(0d,
                "Controls[AutoComplete].ControlHeight must visibly reach its actual candidate popup.");
        });
    }
}
