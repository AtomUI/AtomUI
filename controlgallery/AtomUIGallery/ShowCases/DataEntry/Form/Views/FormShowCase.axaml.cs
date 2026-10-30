using System.Reactive.Disposables;
using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace AtomUIGallery.ShowCases.Form;

public partial class FormShowCase : GalleryReactiveUserControl<FormViewModel>
{
    private static int s_formGid = 3;

    private WindowMessageManager? _messageManager;

    public FormShowCase()
    {
        this.WhenActivated(disposables =>
        {
            if (DataContext is FormViewModel viewModel)
            {
                RefreshLocalizedOptionData(viewModel);
                var languageManager = Application.Current is { } application
                    ? global::AtomUI.ApplicationExtensions.GetLanguageManager(application)
                    : null;
                if (languageManager != null)
                {
                    EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshLocalizedOptionData(viewModel);
                    languageManager.LanguageChanged += handler;
                    disposables.Add(Disposable.Create(() => languageManager.LanguageChanged -= handler));
                }
            }
        });

        InitializeComponent();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _messageManager?.Dispose();
        _messageManager = null;
    }

    private void HandleBasicFormAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomUIForm basicForm)
        {
            basicForm.InitialValues = CreateBasicFormInitialValues();
        }
    }

    private void HandleSemanticDemoFormAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        // 对齐上游 Semantic DOM 示例:进入语义示例时让 password 项进入
        // 两条 error 消息的校验失败状态,username 项保持仅有 help 文案。
        if (sender is AtomUIForm semanticDemoForm)
        {
            semanticDemoForm.Validate();
        }
    }

    private static FormValues CreateBasicFormInitialValues()
    {
        var values = new FormValues();
        values.Add("remember", true);
        return values;
    }

    private void HandleLayoutCaseFormAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomUIForm layoutCaseForm)
        {
            layoutCaseForm.PropertyChanged -= HandleLayoutCaseFormPropertyChanged;
            layoutCaseForm.PropertyChanged += HandleLayoutCaseFormPropertyChanged;
            UpdateLayoutCaseFormBounds(layoutCaseForm, layoutCaseForm.FormLayout);
        }
    }

    private void HandleLayoutCaseFormDetached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomUIForm layoutCaseForm)
        {
            layoutCaseForm.PropertyChanged -= HandleLayoutCaseFormPropertyChanged;
        }
    }

    private void HandleLayoutCaseFormPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (sender is AtomUIForm layoutCaseForm &&
            args.Property == AtomUIForm.FormLayoutProperty &&
            args.NewValue is FormLayout layout)
        {
            UpdateLayoutCaseFormBounds(layoutCaseForm, layout);
        }
    }

    private static void UpdateLayoutCaseFormBounds(AtomUIForm layoutCaseForm, FormLayout layout)
    {
        if (layout == FormLayout.Inline)
        {
            layoutCaseForm.MinWidth            = 0;
            layoutCaseForm.HorizontalAlignment = HorizontalAlignment.Stretch;
        }
        else
        {
            layoutCaseForm.MinWidth            = 600;
            layoutCaseForm.HorizontalAlignment = HorizontalAlignment.Left;
        }
    }

    private void HandleFormLayoutOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm &&
            args.CheckedOption.Tag is FormLayout formLayout)
        {
            vm.FormLayout = formLayout;
        }
    }

    private void HandleFormStyleVariantChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (sender is AtomUISegmented segmented &&
            segmented.SelectedItem is SegmentedItem segmentedItem &&
            segmentedItem.Tag is InputControlStyleVariant styleVariant &&
            DataContext is FormViewModel vm)
        {
            vm.FormStyleVariant = styleVariant;
        }
    }

    private void HandleFormRequiredMarkChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm &&
            args.CheckedOption.Tag is FormRequiredMark requiredMark)
        {
            vm.FormRequiredMark = requiredMark;
        }
    }

    private void HandleFormSizeTypeChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (DataContext is FormViewModel vm &&
            args.CheckedOption.Tag is CustomizableSizeType sizeType)
        {
            vm.FormSizeType = sizeType;
        }
    }

    private void HandleFillClicked(object? sender, RoutedEventArgs args)
    {
        if (sender is not Control source ||
            !TryFindTemplateControl<AtomUIForm>(source, "NoBlockRuleForm", out var noBlockRuleForm))
        {
            return;
        }

        var formValues = new FormValues();
        formValues.Add("url", "https://taobao.com/");
        noBlockRuleForm.SetFormValues(formValues);
    }

    private void HandleNoBlockFormSubmitted(object? sender, FormSubmittedEventArgs args)
    {
        GetMessageManager()?.Show(new AtomUIMessage(
            type: MessageType.Success,
            content: FormShowCaseLanguage.Get(FormShowCaseLangResourceKind.P3SubmitSuccessMessage,
                "Submit success!")
        ));
    }

    private void HandleNoBlockFormValidated(object? sender, FormValidatedEventArgs args)
    {
        if (args.Result == FormValidateResult.Error)
        {
            GetMessageManager()?.Show(new AtomUIMessage(
                type: MessageType.Error,
                content: FormShowCaseLanguage.Get(FormShowCaseLangResourceKind.P3SubmitFailedMessage,
                    "Submit failed!")
            ));
        }
    }

    private WindowMessageManager? GetMessageManager()
    {
        if (_messageManager is not null)
        {
            return _messageManager;
        }

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
        {
            return null;
        }

        _messageManager = new WindowMessageManager(topLevel)
        {
            MaxItems = 10
        };
        return _messageManager;
    }

    private void HandleAddFormItem(object? sender, RoutedEventArgs args)
    {
        if (sender is not Control source ||
            !TryFindTemplateControl<AtomUIForm>(source, "DynamicForm", out var dynamicForm))
        {
            return;
        }

        var formItem    = CreatePassengerFormItem();
        var insertIndex = 0;
        for (var i = 0; i < dynamicForm.Items.Count; ++i)
        {
            var item = dynamicForm.Items[i];
            if (item is FormActionsItem)
            {
                insertIndex = i;
                break;
            }
        }

        insertIndex = Math.Max(0, insertIndex);
        dynamicForm.Items.Insert(insertIndex, formItem);
    }

    private void HandleAddFormItemAtHead(object? sender, RoutedEventArgs args)
    {
        if (sender is Control source &&
            TryFindTemplateControl<AtomUIForm>(source, "DynamicForm", out var dynamicForm))
        {
            dynamicForm.Items.Insert(0, CreatePassengerFormItem());
        }
    }

    private static FormItem CreatePassengerFormItem()
    {
        var id = s_formGid++;
        return new LocalizedPassengerFormItem(id)
        {
            FieldName = $"Passengers_{id}",
            Content   = new AtomUILineEdit(),
            Validators = new List<IFormValidator>()
            {
                new LocalizedPassengerNameValidator()
            }
        };
    }

    private void HandleFormSliderItemAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is AtomUI.Desktop.Controls.Slider formSliderItem)
        {
            formSliderItem.Marks = CreateSliderMarks();
        }
    }

    private static List<SliderMark> CreateSliderMarks()
    {
        return new List<SliderMark>
        {
            new("A", 0),
            new("B", 20),
            new("C", 40),
            new("D", 60),
            new("E", 80),
            new("F", 100)
        };
    }

    private static bool TryFindTemplateControl<T>(Control source, string name, out T control)
        where T : Control
    {
        var current = source;
        while (current is not null)
        {
            if (current is T directControl &&
                current.Name == name)
            {
                control = directControl;
                return true;
            }

            var descendantControl = current.GetVisualDescendants()
                                           .OfType<T>()
                                           .FirstOrDefault(candidate => candidate.Name == name);
            if (descendantControl is not null)
            {
                control = descendantControl;
                return true;
            }

            current = current.Parent as Control;
        }

        control = null!;
        return false;
    }

    private static void RefreshLocalizedOptionData(FormViewModel viewModel)
    {
        viewModel.GenderOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderMale, "Male", "male"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderFemale, "Female", "female"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderOther, "Other", "other")
        ];
        viewModel.PresetGenderOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderMale2, "Male", "male"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderFemale2, "Female", "female"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderOther2, "Other", "other")
        ];
        viewModel.CountryOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderChina, "China", "china"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderUSA, "USA", "usa")
        ];
        viewModel.ColorOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderRed, "Red", "red"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderGreen, "Green", "green"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderBlue, "Blue", "blue")
        ];
        viewModel.DemoSelectOptions = [SelectOption(FormShowCaseLangResourceKind.P2HeaderDemo, "Demo", "demo")];
        viewModel.RequiredStyleSelectOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderBbb, "Bbb", "bbb"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderAaa, "Aaa", "aaa")
        ];
        viewModel.ValidationSelectOptions =
        [
            SelectOption(FormShowCaseLangResourceKind.P2HeaderOptionN1, "Option 1", "1"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderOptionN2, "Option 2", "2"),
            SelectOption(FormShowCaseLangResourceKind.P2HeaderOptionN3, "Option 3", "3")
        ];
        viewModel.PresetCascaderOptions = BuildAddressCascaderOptions();
        viewModel.DemoCascaderOptions =
        [
            CascaderOption(FormShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang", "zhejiang",
            [
                CascaderOption(FormShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou", "hangzhou")
            ])
        ];
        viewModel.ValidationCascaderOptions =
        [
            CascaderOption(FormShowCaseLangResourceKind.P2HeaderXx, "xx", "xx")
        ];
        viewModel.DemoTreeNodes =
        [
            TreeNode(FormShowCaseLangResourceKind.P2HeaderLight, "Light", "light",
            [
                TreeNode(FormShowCaseLangResourceKind.P2HeaderBamboo, "Bamboo", "bamboo")
            ])
        ];
        viewModel.ValidationTreeNodes =
        [
            TreeNode(FormShowCaseLangResourceKind.P2HeaderXx, "xx", "xx")
        ];
    }

    private static List<ICascaderOption> BuildAddressCascaderOptions()
    {
        var hangzhouChildren = new List<ICascaderOption>
        {
            CascaderOption(FormShowCaseLangResourceKind.P2HeaderWestLake, "West Lake", "xihu")
        };

        return
        [
            CascaderOption(FormShowCaseLangResourceKind.P2HeaderZhejiang, "Zhejiang", "zhejiang",
            [
                CascaderOption(FormShowCaseLangResourceKind.P2HeaderHangzhou, "Hangzhou", "hangzhou", hangzhouChildren)
            ]),
            CascaderOption(FormShowCaseLangResourceKind.P2HeaderJiangsu, "Jiangsu", "jiangsu",
            [
                CascaderOption(FormShowCaseLangResourceKind.P2HeaderNanjing, "Nanjing", "nanjing",
                [
                    CascaderOption(FormShowCaseLangResourceKind.P2HeaderZhongHuaMen, "Zhong Hua Men", "zhonghuamen")
                ])
            ])
        ];
    }

    private static SelectOption SelectOption(FormShowCaseLangResourceKind header, string fallback, string content)
    {
        return new SelectOption
        {
            Header  = FormShowCaseLanguage.Get(header, fallback),
            Content = content
        };
    }

    private static CascaderOption CascaderOption(
        FormShowCaseLangResourceKind header,
        string fallback,
        string value,
        IList<ICascaderOption>? children = null)
    {
        return new CascaderOption
        {
            Header = FormShowCaseLanguage.Get(header, fallback),
            Value  = value,
            Children = children ?? []
        };
    }

    private static TreeItemNode TreeNode(
        FormShowCaseLangResourceKind header,
        string fallback,
        string value,
        IList<ITreeItemNode>? children = null)
    {
        return new TreeItemNode
        {
            Header   = FormShowCaseLanguage.Get(header, fallback),
            Value    = value,
            Children = children ?? []
        };
    }

    private sealed class LocalizedPassengerNameValidator : FormStringNotEmptyValidator
    {
        public LocalizedPassengerNameValidator()
        {
            RefreshMessage();
        }

        protected override Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
        {
            RefreshMessage();
            return base.ValidateCoreAsync(fieldName, value, cancellationToken);
        }

        private void RefreshMessage()
        {
            Message = FormShowCaseLanguage.Get(
                FormShowCaseLangResourceKind.P2MessagePleaseInputPassengerSNameOrDeleteThisField,
                "Please input passenger's name or delete this field!");
        }
    }

    private sealed class LocalizedPassengerFormItem(int id) : FormItem
    {
        private IDisposable? _labelBinding;

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _labelBinding ??= GalleryLocalization.CreateBinding(
                this,
                LabelTextProperty,
                FormShowCaseLangResourceKind.P3DynamicPassengerLabelFormat,
                BindingPriority.LocalValue,
                value =>
                {
                    var format = value as string ?? "passengers_{0}";
                    var culture = GalleryLocalization.GetFormattingCulture();
                    return string.Format(culture, format, id);
                });
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _labelBinding?.Dispose();
            _labelBinding = null;
            base.OnDetachedFromVisualTree(e);
        }
    }
}

internal static class FormShowCaseLanguage
{
    public static string Get(FormShowCaseLangResourceKind resourceKind, string fallback)
    {
        return GalleryLocalization.Get(resourceKind, fallback);
    }

    public static string Format(FormShowCaseLangResourceKind resourceKind, string fallback, params object?[] args)
    {
        return GalleryLocalization.Format(resourceKind, fallback, args);
    }
}

public class NoteFormItem : FormItem
{
    protected override void NotifyFormItemChanged(IFormItem formItem)
    {
        if (formItem.FieldName == "gender")
        {
            if (formItem.Content is AtomUISelect select && select.Mode == SelectMode.Single)
            {
                if (formItem.GetItemValue() is ISelectOption selectOption)
                {
                    SetItemValue(FormShowCaseLanguage.Format(FormShowCaseLangResourceKind.P3GenderNoteFormat,
                        "Hi, {0}!",
                        selectOption.Content));
                }
            }
        }
    }
}

public class CustomizeGenderFormItem : FormItem
{
    protected override void NotifyFormItemChanged(IFormItem formItem)
    {
        if (formItem.FieldName == "gender")
        {
            if (formItem.Content is AtomUISelect select && select.Mode == SelectMode.Single)
            {
                var option = formItem.GetItemValue() as ISelectOption;
                if (option?.Content?.ToString() == "other")
                {
                    IsVisible = true;
                }
                else
                {
                    IsVisible = false;
                }
            }
        }
    }
}

public class PriceValidator : AbstractFormValidator
{
    protected override async Task<bool> ValidateCoreAsync(string fieldName, object? value, CancellationToken cancellationToken)
    {
        var price = value as PriceInfo;
        return await Task.FromResult(price != null && price.Value > 0);
    }
}
