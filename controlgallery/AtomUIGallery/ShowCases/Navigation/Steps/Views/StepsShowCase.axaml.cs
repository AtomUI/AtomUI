using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Avalonia;
using Avalonia.Interactivity;
using AtomUISteps = AtomUI.Desktop.Controls.Steps;
using StepsCurrentChangeRequestedEventArgs = AtomUI.Desktop.Controls.StepsCurrentChangeRequestedEventArgs;

namespace AtomUIGallery.ShowCases.Steps;

public partial class StepsShowCase : GalleryReactiveUserControl<StepsViewModel>
{
    public const string LanguageId = nameof(StepsShowCase);

    public static readonly StyledProperty<double[]> DashedArrayProperty =
        AvaloniaProperty.Register<StepsShowCase, double[]>(nameof(DashedArray));

    public double[] DashedArray
    {
        get => GetValue(DashedArrayProperty);
        set => SetValue(DashedArrayProperty, value);
    }

    public StepsShowCase()
    {
        InitializeComponent();
        DashedArray = [4d, 3d];

        this.WhenActivated(disposables =>
        {
            ResetInteractiveState();

            var languageManager = GalleryLocalization.GetLanguageManager();
            if (languageManager != null)
            {
                EventHandler<LanguageChangedEventArgs> handler = (_, _) => RefreshInteractiveText();
                languageManager.LanguageChanged += handler;
                Disposable.Create(() => languageManager.LanguageChanged -= handler)
                          .DisposeWith(disposables);
            }
        });
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        ResetInteractiveState();
    }

    public void HandleNextButtonClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is StepsViewModel viewModel)
        {
            viewModel.MoveToNextInteractiveStep();
        }
    }

    public void HandlePreviousButtonClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is StepsViewModel viewModel)
        {
            viewModel.MoveToPreviousInteractiveStep();
        }
    }

    public void HandleCurrentChangeRequested(object? sender, StepsCurrentChangeRequestedEventArgs args)
    {
        if (DataContext is StepsViewModel viewModel)
        {
            viewModel.Current = args.Current;
        }
    }

    public void HandleLocalCurrentChangeRequested(object? sender, StepsCurrentChangeRequestedEventArgs args)
    {
        if (sender is AtomUISteps steps)
        {
            steps.Current = args.Current;
        }
    }

    private void ResetInteractiveState()
    {
        if (DataContext is StepsViewModel viewModel)
        {
            viewModel.ResetInteractiveStep();
        }
    }

    private void RefreshInteractiveText()
    {
        if (DataContext is StepsViewModel viewModel)
        {
            viewModel.RefreshInteractiveText();
        }
    }
}
