using System.Text;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;

namespace AtomUI.Performance;

internal static partial class Program
{
    internal const int DefaultCount = 60;

    [STAThread]
    public static int Main(string[] args)
    {
        var options = PerfOptions.Parse(args);
        SetupAvalonia();

        AddOnDecoratedBoxPerfProbe.IsEnabled = true;
        if (options.MeasureColorPickerInteractions)
        {
            return RunColorPickerInteractionBenchmarks(options.Count, options.MarkdownOutputPath);
        }
        if (options.MeasureSwitchInteractions)
        {
            return RunSwitchInteractionBenchmarks(options.Count, options.MarkdownOutputPath);
        }
        if (options.MeasureSelectInteractions)
        {
            return RunSelectInteractionBenchmarks(options.Count, options.MarkdownOutputPath);
        }

        if (options.VerifyAdornerLayerStates ||
            options.VerifyAccessories ||
            options.VerifyEffectiveBrushes ||
            options.VerifyAddonStates ||
            options.VerifyAntDesignMetadata ||
            options.VerifyIconHiddenSlots ||
            options.VerifyIconProviderCache ||
            options.VerifyButtonStates ||
            options.VerifySpaceStates ||
            options.VerifySelectStates ||
            options.VerifyAutoCompleteStates ||
            options.VerifyAvatarStates ||
            options.VerifyBadgeStates ||
            options.VerifyBreadcrumbStates ||
            options.VerifyButtonSpinnerStates ||
            options.VerifyCalendarStates ||
            options.VerifyCardStates ||
            options.VerifyCarouselStates ||
            options.VerifyCascaderStates ||
            options.VerifyCheckBoxStates ||
            options.VerifyColorPickerStates ||
            options.VerifyCollapseStates ||
            options.VerifyComboBoxStates ||
            options.VerifyDataGridStates ||
            options.VerifyDatePickerStates ||
            options.VerifyTimePickerStates ||
            options.VerifyDescriptionsStates ||
            options.VerifyDialogStates ||
            options.VerifyDropdownButtonStates ||
            options.VerifyDrawerStates ||
            options.VerifyExpanderStates ||
            options.VerifyEmptyStates ||
            options.VerifyFloatButtonStates ||
            options.VerifyFlexPanelStates ||
            options.VerifyFlyoutStates ||
            options.VerifyFormStates ||
            options.VerifyGridStates ||
            options.VerifyGroupBoxStates ||
            options.VerifyImagePreviewerStates ||
            options.VerifyInfoFlyoutStates ||
            options.VerifyInputStates ||
            options.VerifyListBoxStates ||
            options.VerifyListViewStates ||
            options.VerifyMarqueeLabelStates ||
            options.VerifyMentionsStates ||
            options.VerifyMenuStates ||
            options.VerifyNavMenuStates ||
            options.VerifyMessageStates ||
            options.VerifyMessageBoxStates ||
            options.VerifyNotificationStates ||
            options.VerifyNumericUpDownStates ||
            options.VerifyOptionButtonGroupStates ||
            options.VerifyPaginationStates ||
            options.VerifyPopupConfirmStates ||
            options.VerifyProgressBarStates ||
            options.VerifyRadioButtonStates ||
            options.VerifyQRCodeStates ||
            options.VerifyRateStates ||
            options.VerifyResultStates ||
            options.VerifyScrollViewerStates ||
            options.VerifySkeletonStates ||
            options.VerifySegmentedStates ||
            options.VerifySpinStates ||
            options.VerifySplitButtonStates ||
            options.VerifySwitchStates ||
            options.VerifyTabControlStates ||
            options.VerifyStepsStates ||
            options.VerifyStatisticStates ||
            options.VerifyTimelineStates ||
            options.VerifySplitViewStates ||
            options.VerifySplitterStates ||
            options.VerifySliderStates ||
            options.VerifyTreeViewStates ||
            options.VerifyTransferStates ||
            options.VerifyWatermarkStates ||
            options.VerifyWindowTitleBarStates ||
            options.VerifyTextBlockStates)
        {
            var verified = true;
            if (options.VerifyAdornerLayerStates)
            {
                verified &= RunAdornerLayerStateVerification();
            }
            if (options.VerifyAccessories)
            {
                Console.WriteLine("--verify-accessories is currently disabled (Avalonia 12 accessory architecture rewrite pending).");
            }
            if (options.VerifyEffectiveBrushes)
            {
                Console.WriteLine("--verify-effective-brushes is currently disabled (AddOnDecoratedBox InnerBox*Brush API removed in Avalonia 12 migration).");
            }
            if (options.VerifyAddonStates)
            {
                verified &= RunAddonStateVerification();
            }
            if (options.VerifyAntDesignMetadata)
            {
                verified &= RunAntDesignMetadataVerification();
            }
            if (options.VerifyIconHiddenSlots)
            {
                verified &= RunIconHiddenSlotsVerification();
            }
            if (options.VerifyIconProviderCache)
            {
                verified &= RunIconProviderCacheVerification();
            }
            if (options.VerifyButtonStates)
            {
                verified &= RunButtonStateVerification();
            }
            if (options.VerifySpaceStates)
            {
                verified &= RunSpaceStateVerification();
            }
            if (options.VerifySelectStates)
            {
                verified &= RunSelectStateVerification();
            }
            if (options.VerifyAutoCompleteStates)
            {
                verified &= RunAutoCompleteStateVerification();
            }
            if (options.VerifyAvatarStates)
            {
                verified &= RunAvatarStateVerification();
            }
            if (options.VerifyBadgeStates)
            {
                verified &= RunBadgeStateVerification();
            }
            if (options.VerifyBreadcrumbStates)
            {
                verified &= RunBreadcrumbStateVerification();
            }
            if (options.VerifyButtonSpinnerStates)
            {
                verified &= RunButtonSpinnerStateVerification();
            }
            if (options.VerifyCalendarStates)
            {
                verified &= RunCalendarStateVerification();
            }
            if (options.VerifyCardStates)
            {
                verified &= RunCardStateVerification();
            }
            if (options.VerifyCarouselStates)
            {
                verified &= RunCarouselStateVerification();
            }
            if (options.VerifyCascaderStates)
            {
                verified &= RunCascaderStateVerification();
            }
            if (options.VerifyCheckBoxStates)
            {
                verified &= RunCheckBoxStateVerification();
            }
            if (options.VerifyColorPickerStates)
            {
                verified &= RunColorPickerStateVerification();
            }
            if (options.VerifyCollapseStates)
            {
                verified &= RunCollapseStateVerification();
            }
            if (options.VerifyComboBoxStates)
            {
                verified &= RunComboBoxStateVerification();
            }
            if (options.VerifyDataGridStates)
            {
                verified &= RunDataGridStateVerification();
            }
            if (options.VerifyDatePickerStates)
            {
                verified &= RunDatePickerStateVerification();
            }
            if (options.VerifyTimePickerStates)
            {
                verified &= RunTimePickerStateVerification();
            }
            if (options.VerifyDescriptionsStates)
            {
                verified &= RunDescriptionsStateVerification();
            }
            if (options.VerifyDialogStates)
            {
                verified &= RunDialogStateVerification();
            }
            if (options.VerifyDropdownButtonStates)
            {
                verified &= RunDropdownButtonStateVerification();
            }
            if (options.VerifyDrawerStates)
            {
                verified &= RunDrawerStateVerification();
            }
            if (options.VerifyExpanderStates)
            {
                verified &= RunExpanderStateVerification();
            }
            if (options.VerifyEmptyStates)
            {
                verified &= RunEmptyStateVerification();
            }
            if (options.VerifyFloatButtonStates)
            {
                verified &= RunFloatButtonStateVerification();
            }
            if (options.VerifyFlexPanelStates)
            {
                verified &= RunFlexPanelStateVerification();
            }
            if (options.VerifyFlyoutStates)
            {
                verified &= RunFlyoutStateVerification();
            }
            if (options.VerifyFormStates)
            {
                verified &= RunFormStateVerification();
            }
            if (options.VerifyGridStates)
            {
                verified &= RunGridStateVerification();
            }
            if (options.VerifyGroupBoxStates)
            {
                verified &= RunGroupBoxStateVerification();
            }
            if (options.VerifyImagePreviewerStates)
            {
                verified &= RunImagePreviewerStateVerification();
            }
            if (options.VerifyInfoFlyoutStates)
            {
                verified &= RunInfoFlyoutStateVerification();
            }
            if (options.VerifyInputStates)
            {
                verified &= RunInputStateVerification();
            }
            if (options.VerifyListBoxStates)
            {
                verified &= RunListBoxStateVerification();
            }
            if (options.VerifyListViewStates)
            {
                verified &= RunListViewStateVerification();
            }
            if (options.VerifyMarqueeLabelStates)
            {
                verified &= RunMarqueeLabelStateVerification();
            }
            if (options.VerifyMentionsStates)
            {
                verified &= RunMentionsStateVerification();
            }
            if (options.VerifyMenuStates)
            {
                verified &= RunMenuStateVerification();
            }
            if (options.VerifyNavMenuStates)
            {
                verified &= RunNavMenuStateVerification();
            }
            if (options.VerifyMessageStates)
            {
                verified &= RunMessageStateVerification();
            }
            if (options.VerifyMessageBoxStates)
            {
                verified &= RunMessageBoxStateVerification();
            }
            if (options.VerifyNotificationStates)
            {
                verified &= RunNotificationStateVerification();
            }
            if (options.VerifyNumericUpDownStates)
            {
                verified &= RunNumericUpDownStateVerification();
            }
            if (options.VerifyOptionButtonGroupStates)
            {
                verified &= RunOptionButtonGroupStateVerification();
            }
            if (options.VerifyPaginationStates)
            {
                verified &= RunPaginationStateVerification();
            }
            if (options.VerifyPopupConfirmStates)
            {
                verified &= RunPopupConfirmStateVerification();
            }
            if (options.VerifyProgressBarStates)
            {
                verified &= RunProgressBarStateVerification();
            }
            if (options.VerifyRadioButtonStates)
            {
                verified &= RunRadioButtonStateVerification();
            }
            if (options.VerifyQRCodeStates)
            {
                verified &= RunQRCodeStateVerification();
            }
            if (options.VerifyRateStates)
            {
                verified &= RunRateStateVerification();
            }
            if (options.VerifyResultStates)
            {
                verified &= RunResultStateVerification();
            }
            if (options.VerifyScrollViewerStates)
            {
                verified &= RunScrollViewerStateVerification();
            }
            if (options.VerifySkeletonStates)
            {
                verified &= RunSkeletonStateVerification();
            }
            if (options.VerifySegmentedStates)
            {
                verified &= RunSegmentedStateVerification();
            }
            if (options.VerifySpinStates)
            {
                verified &= RunSpinStateVerification();
            }
            if (options.VerifySplitButtonStates)
            {
                verified &= RunSplitButtonStateVerification();
            }
            if (options.VerifySwitchStates)
            {
                verified &= RunSwitchStateVerification();
            }
            if (options.VerifyTabControlStates)
            {
                verified &= RunTabControlStateVerification();
            }
            if (options.VerifyStepsStates)
            {
                verified &= RunStepsStateVerification();
            }
            if (options.VerifyStatisticStates)
            {
                verified &= RunStatisticStateVerification();
            }
            if (options.VerifyTimelineStates)
            {
                verified &= RunTimelineStateVerification();
            }
            if (options.VerifySplitViewStates)
            {
                verified &= RunSplitViewStateVerification();
            }
            if (options.VerifySplitterStates)
            {
                verified &= RunSplitterStateVerification();
            }
            if (options.VerifySliderStates)
            {
                verified &= RunSliderStateVerification();
            }
            if (options.VerifyTreeViewStates)
            {
                verified &= RunTreeViewStateVerification();
            }
            if (options.VerifyTransferStates)
            {
                verified &= RunTransferStateVerification();
            }
            if (options.VerifyWatermarkStates)
            {
                verified &= RunWatermarkStateVerification();
            }
            if (options.VerifyWindowTitleBarStates)
            {
                verified &= RunWindowTitleBarStateVerification();
            }
            if (options.VerifyTextBlockStates)
            {
                verified &= RunTextBlockStateVerification();
            }
            return verified ? 0 : 1;
        }

        var scenarios = CreateScenarios(options.Suite);
        foreach (var scenario in scenarios)
        {
            RunWarmup(scenario, Math.Min(5, options.Count));
        }

        var results = scenarios
            .Select(scenario => MeasureScenario(scenario, options.Count))
            .ToList();

        Console.WriteLine(RenderTable(results));

        if (!string.IsNullOrWhiteSpace(options.MarkdownOutputPath))
        {
            var markdown = RenderMarkdown(results, options);
            var fullPath = Path.GetFullPath(options.MarkdownOutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, markdown, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            Console.WriteLine();
            Console.WriteLine($"Wrote markdown baseline: {fullPath}");
        }

        return 0;
    }

    private static void SetupAvalonia()
    {
        // OnLastWindowClose (the default) lets the first verification-window dispose begin
        // application shutdown; from Avalonia 12.1.2 the dispatcher then silently drops all
        // later posts, which kills multi-window verifications like the DataGrid range suite.
        AppBuilder.Configure<PerfApplication>()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                  .UseAtomUI(builder =>
                  {
                      builder.Theme.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID);
                      builder.UseDesktopControls();
                      builder.UseDesktopColorPicker();
                      builder.UseDesktopDataGrid();
                  })
                  .SetupWithLifetime(new ClassicDesktopStyleApplicationLifetime
                  {
                      ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown
                  });
    }

    private static IReadOnlyList<PerfScenario> CreateScenarios(string suite)
    {
        return suite.ToLowerInvariant() switch
        {
            "adornerlayer" => CreateAdornerLayerScenarios(),
            "icon" => CreateIconScenarios(),
            "avatar" => CreateAvatarScenarios(),
            "badge" => CreateBadgeScenarios(),
            "breadcrumb" => CreateBreadcrumbScenarios(),
            "button" => CreateButtonScenarios(),
            "buttonspinner" => CreateButtonSpinnerScenarios(),
            "calendar" => CreateCalendarScenarios(),
            "card" => CreateCardScenarios(),
            "carousel" => CreateCarouselScenarios(),
            "cascader" => CreateCascaderScenarios(),
            "checkbox" => CreateCheckBoxScenarios(),
            "colorpicker" => CreateColorPickerScenarios(),
            "collapse" => CreateCollapseScenarios(),
            "combobox" => CreateComboBoxScenarios(),
            "datagrid" => CreateDataGridScenarios(),
            "datepicker" => CreateDatePickerScenarios(),
            "descriptions" => CreateDescriptionsScenarios(),
            "dialog" => CreateDialogScenarios(),
            "dropdownbutton" => CreateDropdownButtonScenarios(),
            "drawer" => CreateDrawerScenarios(),
            "expander" => CreateExpanderScenarios(),
            "empty" => CreateEmptyScenarios(),
            "flexpanel" => CreateFlexPanelScenarios(),
            "flyouts" => CreateFlyoutScenarios(),
            "floatbutton" => CreateFloatButtonScenarios(),
            "form" => CreateFormScenarios(),
            "grid" => CreateGridScenarios(),
            "groupbox" => CreateGroupBoxScenarios(),
            "imagepreviewer" => CreateImagePreviewerScenarios(),
            "infoflyout" => CreateInfoFlyoutScenarios(),
            "input" => CreateInputScenarios(),
            "listbox" => CreateListBoxScenarios(),
            "listview" => CreateListViewScenarios(),
            "marqueelabel" => CreateMarqueeLabelScenarios(),
            "mentions" => CreateMentionsScenarios(),
            "menu" => CreateMenuScenarios(),
            "navmenu" => CreateNavMenuScenarios(),
            "message" => CreateMessageScenarios(),
            "messagebox" => CreateMessageBoxScenarios(),
            "notification" => CreateNotificationScenarios(),
            "numericupdown" => CreateNumericUpDownScenarios(),
            "optionbuttongroup" => CreateOptionButtonGroupScenarios(),
            "pagination" => CreatePaginationScenarios(),
            "popupconfirm" => CreatePopupConfirmScenarios(),
            "progressbar" => CreateProgressBarScenarios(),
            "radiobutton" => CreateRadioButtonScenarios(),
            "qrcode" => CreateQRCodeScenarios(),
            "rate" => CreateRateScenarios(),
            "result" => CreateResultScenarios(),
            "scrollviewer" => CreateScrollViewerScenarios(),
            "skeleton" => CreateSkeletonScenarios(),
            "segmented" => CreateSegmentedScenarios(),
            "splitbutton" => CreateSplitButtonScenarios(),
            "slider" => CreateSliderScenarios(),
            "spin" => CreateSpinScenarios(),
            "switch" => CreateSwitchScenarios(),
            "tabcontrol" => CreateTabControlScenarios(),
            "steps" => CreateStepsScenarios(),
            "statistic" => CreateStatisticScenarios(),
            "timeline" => CreateTimelineScenarios(),
            "watermark" => CreateWatermarkScenarios(),
            "windowtitlebar" => CreateWindowTitleBarScenarios(),
            "splitview" => CreateSplitViewScenarios(),
            "splitter" => CreateSplitterScenarios(),
            "space" => CreateSpaceScenarios(),
            "select" => CreateSelectScenarios(),
            "autocomplete" => CreateAutoCompleteScenarios(),
            "textblock" => CreateTextBlockScenarios(),
            "popup" => CreatePopupScenarios(),
            "tag" => CreateTagScenarios(),
            "separator" => CreateSeparatorScenarios(),
            _ => CreateAddOnScenarios()
        };
    }
}
