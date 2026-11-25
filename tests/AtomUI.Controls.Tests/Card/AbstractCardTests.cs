using System.Collections.Specialized;
using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Card;

public class AbstractCardTests
{
    static AbstractCardTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestCard : AbstractCard
    {
        public int CreatedCount;
        public int AttachedCount;
        public int DetachedCount;
        public readonly List<NotifyCollectionChangedAction> ActionChanges = new();

        public TestCard(bool withLoadingHost = true)
        {
            Theme = new ControlTheme(typeof(TestCard))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestCard>((_, scope) =>
                    {
                        var host = new Panel { Name = withLoadingHost ? LoadingHostPart : "Other" };
                        host.Children.Add(new ContentPresenter
                        {
                            Name      = "Presenter",
                            [!ContentPresenter.ContentProperty] = new Avalonia.Data.Binding(nameof(Content)) { Source = this },
                            [!Visual.IsVisibleProperty]         = new Avalonia.Data.Binding(nameof(IsContentVisible)) { Source = this }
                        });
                        return host.RegisterInNameScope(scope);
                    }))
                }
            };
        }

        public bool ContentVisible => IsContentVisible;

        public bool ActionsPanelVisible => IsActionsPanelVisible;

        public Control? Indicator => LoadingIndicator;

        protected override Control CreateLoadingIndicator()
        {
            CreatedCount++;
            return new Border { Name = "Indicator" };
        }

        protected override void AttachLoadingIndicator(Control indicator)
        {
            AttachedCount++;
            base.AttachLoadingIndicator(indicator);
        }

        protected override void DetachLoadingIndicator(Control indicator)
        {
            DetachedCount++;
            base.DetachLoadingIndicator(indicator);
        }

        protected override void OnActionsChanged(NotifyCollectionChangedEventArgs e)
        {
            ActionChanges.Add(e.Action);
            base.OnActionsChanged(e);
        }

        public void ReapplyTemplate()
        {
            var theme = Theme;
            Theme = null;
            Theme = theme;
            ApplyTemplate();
        }
    }

    private static Window Host(Control content)
    {
        var window = new Window { Width = 400, Height = 200, Content = content };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        return window;
    }

    private static Border[] Indicators(Control card)
    {
        return card.GetVisualDescendants().OfType<Border>().Where(b => b.Name == "Indicator").ToArray();
    }

    [Fact]
    public void Defaults()
    {
        var card = new TestCard();

        card.IsLoading.ShouldBeFalse();
        card.ContentVisible.ShouldBeTrue();
        card.Indicator.ShouldBeNull();
        card.Actions.ShouldBeEmpty();
        card.ActionsPanelVisible.ShouldBeFalse();
    }

    [Fact]
    public void Indicator_exists_only_while_loading()
    {
        var card   = new TestCard();
        var window = Host(card);

        card.Indicator.ShouldBeNull();
        Indicators(card).ShouldBeEmpty();
        card.CreatedCount.ShouldBe(0);

        card.IsLoading = true;
        Dispatcher.UIThread.RunJobs();

        var indicator = card.Indicator.ShouldNotBeNull();
        Indicators(card).ShouldHaveSingleItem().ShouldBeSameAs(indicator);
        card.CreatedCount.ShouldBe(1);
        card.AttachedCount.ShouldBe(1);

        card.IsLoading = false;
        Dispatcher.UIThread.RunJobs();

        card.Indicator.ShouldBeNull();
        Indicators(card).ShouldBeEmpty();
        card.DetachedCount.ShouldBe(1);
        indicator.GetVisualParent().ShouldBeNull();

        window.Close();
    }

    [Fact]
    public void Loading_before_template_creates_indicator_on_apply()
    {
        var card = new TestCard { IsLoading = true };
        card.CreatedCount.ShouldBe(0);

        var window = Host(card);

        card.CreatedCount.ShouldBe(1);
        Indicators(card).ShouldHaveSingleItem();
        window.Close();
    }

    [Fact]
    public void Loading_toggle_restores_content()
    {
        var content = new TextBlock { Text = "Body" };
        var card    = new TestCard { Content = content };
        var window  = Host(card);

        var presenter = card.GetVisualDescendants().OfType<ContentPresenter>().Single();
        card.ContentVisible.ShouldBeTrue();
        presenter.IsVisible.ShouldBeTrue();
        content.IsEffectivelyVisible.ShouldBeTrue();

        card.IsLoading = true;
        Dispatcher.UIThread.RunJobs();
        card.ContentVisible.ShouldBeFalse();
        presenter.IsVisible.ShouldBeFalse();

        card.IsLoading = false;
        Dispatcher.UIThread.RunJobs();
        card.ContentVisible.ShouldBeTrue();
        presenter.IsVisible.ShouldBeTrue();
        content.IsEffectivelyVisible.ShouldBeTrue();

        window.Close();
    }

    [Fact]
    public void Rapid_toggle_leaves_no_residual_indicator()
    {
        var card   = new TestCard();
        var window = Host(card);

        for (var i = 0; i < 5; i++)
        {
            card.IsLoading = true;
            card.IsLoading = false;
        }
        Dispatcher.UIThread.RunJobs();
        Indicators(card).ShouldBeEmpty();
        card.Indicator.ShouldBeNull();

        card.IsLoading = true;
        Dispatcher.UIThread.RunJobs();
        Indicators(card).ShouldHaveSingleItem();
        card.CreatedCount.ShouldBe(card.DetachedCount + 1);

        window.Close();
    }

    [Fact]
    public void Template_reapply_drops_the_old_indicator_and_creates_one_while_loading()
    {
        var card   = new TestCard { IsLoading = true };
        var window = Host(card);
        var first  = card.Indicator.ShouldNotBeNull();

        card.ReapplyTemplate();
        Dispatcher.UIThread.RunJobs();

        var second = card.Indicator.ShouldNotBeNull();
        second.ShouldNotBeSameAs(first);
        first.GetVisualParent().ShouldBeNull();
        Indicators(card).ShouldHaveSingleItem().ShouldBeSameAs(second);

        card.IsLoading = false;
        card.ReapplyTemplate();
        Dispatcher.UIThread.RunJobs();
        card.Indicator.ShouldBeNull();
        Indicators(card).ShouldBeEmpty();

        window.Close();
    }

    [Fact]
    public void Missing_loading_host_creates_nothing()
    {
        var card   = new TestCard(withLoadingHost: false) { IsLoading = true };
        var window = Host(card);

        card.CreatedCount.ShouldBe(0);
        card.Indicator.ShouldBeNull();
        window.Close();
    }

    [Fact]
    public void Actions_sync_and_visibility()
    {
        var card = new TestCard();
        var a    = new Border();
        var b    = new Border();

        card.Actions.Add(a);
        card.ActionsPanelVisible.ShouldBeTrue();
        card.Actions.Add(b);
        card.ActionsPanelVisible.ShouldBeTrue();
        card.Actions.Move(0, 1);
        card.Actions.Remove(a);
        card.ActionsPanelVisible.ShouldBeTrue();
        card.Actions.Remove(b);
        card.ActionsPanelVisible.ShouldBeFalse();

        card.ActionChanges.ShouldBe(
        [
            NotifyCollectionChangedAction.Add,
            NotifyCollectionChangedAction.Add,
            NotifyCollectionChangedAction.Move,
            NotifyCollectionChangedAction.Remove,
            NotifyCollectionChangedAction.Remove
        ]);
    }

    [Fact]
    public void Headerless_follows_header_and_extra()
    {
        var card   = new TestCard();
        var window = Host(card);

        card.Classes.Contains(":headerless").ShouldBeTrue();

        card.Header = "Title";
        card.Classes.Contains(":headerless").ShouldBeFalse();
        card.Header = null;
        card.Classes.Contains(":headerless").ShouldBeTrue();

        card.HeaderTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock());
        card.Classes.Contains(":headerless").ShouldBeFalse();
        card.HeaderTemplate = null;
        card.Classes.Contains(":headerless").ShouldBeTrue();

        card.Extra = "More";
        card.Classes.Contains(":headerless").ShouldBeFalse();
        card.Extra = null;
        card.Classes.Contains(":headerless").ShouldBeTrue();

        card.ExtraTemplate = new FuncDataTemplate<object>((_, _) => new TextBlock());
        card.Classes.Contains(":headerless").ShouldBeFalse();
        card.ExtraTemplate = null;
        card.Classes.Contains(":headerless").ShouldBeTrue();

        window.Close();
    }
}
