using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Desktop.Controls;

namespace AtomUIGallery.ShowCases.Mentions;

public partial class MentionsShowCase : GalleryReactiveUserControl<MentionsViewModel>
{
    public const string LanguageId = nameof(MentionsShowCase);

    public MentionsShowCase()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            if (DataContext is MentionsViewModel viewModel)
            {
                InitBasicMentionOptions(viewModel);
                InitSemanticPreviewOptions(viewModel);
                InitStyleClassOptions(viewModel);
                viewModel.MentionTriggers          = ["@", "#"];
                viewModel.MentionOptionAsyncLoader = new MentionOptionsAsyncLoader();

                Disposable.Create(() =>
                {
                    viewModel.BasicMentionOptions      = null;
                    viewModel.SemanticPreviewOptions   = null;
                    viewModel.StyleClassOptions        = null;
                    viewModel.MentionTriggers          = null;
                    viewModel.MentionOptionAsyncLoader = null;
                }).DisposeWith(disposables);
            }
        });
    }

    private void InitSemanticPreviewOptions(MentionsViewModel viewModel)
    {
        viewModel.SemanticPreviewOptions =
        [
            new MentionOption { Header = "afc163", Value = "afc163" },
            new MentionOption { Header = "zombieJ", Value = "zombieJ" },
            new MentionOption { Header = "yesmeck", Value = "yesmeck" }
        ];
    }

    private static void InitStyleClassOptions(MentionsViewModel viewModel)
    {
        viewModel.StyleClassOptions =
        [
            new MentionOption { Header = "afc163", Value = "afc163" },
            new MentionOption { Header = "zombieJ", Value = "zombieJ" }
        ];
    }

    private void InitBasicMentionOptions(MentionsViewModel viewModel)
    {
        viewModel.BasicMentionOptions =
        [
            new MentionOption()
            {
                Header = "afc163",
                Value  = "afc163"
            },
            new MentionOption()
            {
                Header = "zombieJ",
                Value  = "zombieJ"
            },
            new MentionOption()
            {
                Header = "yesmeck",
                Value  = "yesmeck"
            }
        ];
    }

    private void HandleCandidateTriggered(object? sender, MentionCandidateTriggeredEventArgs e)
    {
        if (sender is AtomUIMentions mentions)
        {
            if (e.TriggerChar == "@")
            {
                mentions.OptionsSource =
                [
                    new MentionOption()
                    {
                        Header = "afc163",
                        Value  = "afc163"
                    },
                    new MentionOption()
                    {
                        Header = "zombieJ",
                        Value  = "zombieJ"
                    },
                    new MentionOption()
                    {
                        Header = "yesmeck",
                        Value  = "yesmeck"
                    }
                ];
            }
            else if (e.TriggerChar == "#")
            {
                mentions.OptionsSource =
                [
                    new MentionOption()
                    {
                        Header = "1.0",
                        Value  = "1.0"
                    },
                    new MentionOption()
                    {
                        Header = "2.0",
                        Value  = "2.0"
                    },
                    new MentionOption()
                    {
                        Header = "3.0",
                        Value  = "3.0"
                    }
                ];
            }
        }
    }
}
