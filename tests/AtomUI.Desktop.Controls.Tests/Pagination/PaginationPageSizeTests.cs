using System.ComponentModel;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUIComboBox = AtomUI.Desktop.Controls.ComboBox;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Pagination;

public class PaginationPageSizeTests
{
    static PaginationPageSizeTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Pagination_Custom_SizeSelector_Follows_PageNavigation_By_Default()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            IsShowSizeChanger  = true,
            SizeChangerTemplate = CreateSizeChangerTemplate(),
            IsMotionEnabled    = false
        };

        ShowInWindow(pagination, () =>
        {
            var root = pagination.GetVisualDescendants()
                                .OfType<StackPanel>()
                                .Single(control => control.Name == "PART_RootLayout");
            var childNames = root.Children.OfType<Control>().Select(control => control.Name).ToArray();

            Array.IndexOf(childNames, "PART_Nav")
                .ShouldBeLessThan(Array.IndexOf(childNames, "PART_SizeChangerPresenter"));
        });
    }

    [Fact]
    public void Pagination_PageSize_Clamp_Emits_Single_State_Notification()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total       = 100,
            CurrentPage = 10,
            PageSize    = 10
        };
        var notificationCount = 0;
        pagination.CurrentPageChanged += (_, _) => notificationCount++;

        pagination.PageSize = 100;

        pagination.CurrentPage.ShouldBe(1);
        pagination.PageCount.ShouldBe(1);
        notificationCount.ShouldBe(1);
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_API_Uses_Null_Default_And_TwoWay_Context()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination();

        pagination.SizeChangerTemplate.ShouldBeNull();
        PaginationSizeChangerContext.PageSizeProperty
                                    .GetMetadata(typeof(PaginationSizeChangerContext))
                                    .DefaultBindingMode
                                    .ShouldBe(BindingMode.TwoWay);
        PaginationSizeChangerContext.SizeTypeProperty.IsReadOnly.ShouldBeTrue();
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_Materializes_Context_And_Updates_Page_State()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total                = 500,
            CurrentPage          = 50,
            PageSize             = 10,
            SizeType             = CustomizableSizeType.Small,
            IsShowSizeChanger    = true,
            SizeChangerTemplate  = CreateSizeChangerTemplate(),
            IsMotionEnabled      = false
        };

        ShowInWindow(pagination, () =>
        {
            var customRoot = pagination.GetVisualDescendants()
                                       .OfType<Border>()
                                       .Single(control => control.Name == "CustomPageSizeChanger");
            var context = customRoot.Tag.ShouldBeOfType<PaginationSizeChangerContext>();

            context.PageSize.ShouldBe(10);
            context.SizeType.ShouldBe(CustomizableSizeType.Small);
            pagination.GetVisualDescendants().OfType<AtomUIComboBox>().ShouldBeEmpty();

            context.PageSize = 100;
            Dispatcher.UIThread.RunJobs();

            pagination.PageSize.ShouldBe(100);
            pagination.PageCount.ShouldBe(5);
            pagination.CurrentPage.ShouldBe(5);

            pagination.PageSize = 25;
            Dispatcher.UIThread.RunJobs();
            context.PageSize.ShouldBe(25);
        });
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_Switches_Content_And_Survives_Template_Reapply()
    {
        var observedContexts = new List<PaginationSizeChangerContext>();
        var template = CreateSizeChangerTemplate(observedContexts);
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total               = 100,
            IsShowSizeChanger   = true,
            SizeChangerTemplate = template,
            IsEnabled           = false,
            IsMotionEnabled     = false
        };

        ShowInWindow(pagination, () =>
        {
            var firstRoot = FindCustomSizeChanger(pagination);
            var context = firstRoot.Tag.ShouldBeOfType<PaginationSizeChangerContext>();
            firstRoot.IsEffectivelyEnabled.ShouldBeFalse();

            pagination.SizeChangerTemplate = null;
            Dispatcher.UIThread.RunJobs();
            firstRoot.IsAttachedToVisualTree().ShouldBeFalse();
            pagination.GetVisualDescendants().OfType<AtomUIComboBox>().Count().ShouldBe(1);

            pagination.SizeChangerTemplate = template;
            Dispatcher.UIThread.RunJobs();
            FindCustomSizeChanger(pagination).Tag.ShouldBeSameAs(context);
            pagination.GetVisualDescendants().OfType<AtomUIComboBox>().ShouldBeEmpty();

            pagination.IsShowSizeChanger = false;
            Dispatcher.UIThread.RunJobs();
            pagination.GetVisualDescendants()
                      .OfType<Border>()
                      .ShouldNotContain(control => control.Name == "CustomPageSizeChanger");

            pagination.IsShowSizeChanger = true;
            Dispatcher.UIThread.RunJobs();
            FindCustomSizeChanger(pagination).Tag.ShouldBeSameAs(context);

            pagination.ClearValue(TemplatedControl.TemplateProperty);
            pagination.ApplyTemplate();
            Dispatcher.UIThread.RunJobs();

            FindCustomSizeChanger(pagination).Tag.ShouldBeSameAs(context);
            pagination.GetVisualDescendants()
                      .OfType<Border>()
                      .Count(control => control.Name == "CustomPageSizeChanger")
                      .ShouldBe(1);
            observedContexts.ShouldAllBe(candidate => ReferenceEquals(candidate, context));
        });
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_Replays_Template_Changes_Made_While_Detached()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total               = 100,
            IsShowSizeChanger   = true,
            SizeChangerTemplate = CreateSizeChangerTemplate(),
            IsMotionEnabled     = false
        };

        ShowInWindow(pagination, window =>
        {
            FindCustomSizeChanger(pagination).ShouldNotBeNull();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();
            pagination.SizeChangerTemplate = null;

            window.Content = pagination;
            Dispatcher.UIThread.RunJobs();

            pagination.GetVisualDescendants().OfType<AtomUIComboBox>().Count().ShouldBe(1);
            pagination.GetVisualDescendants()
                      .OfType<Border>()
                      .ShouldNotContain(control => control.Name == "CustomPageSizeChanger");
        });
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_Preserves_External_TwoWay_PageSize_Binding()
    {
        var source = new PageSizeBindingSource { PageSize = 10 };
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total               = 500,
            IsShowSizeChanger   = true,
            SizeChangerTemplate = CreateSizeChangerTemplate(),
            IsMotionEnabled     = false
        };
        pagination.Bind(
            AbstractPagination.PageSizeProperty,
            new Binding(nameof(PageSizeBindingSource.PageSize))
            {
                Source = source,
                Mode   = BindingMode.TwoWay
            });

        ShowInWindow(pagination, () =>
        {
            var context = FindCustomSizeChanger(pagination).Tag.ShouldBeOfType<PaginationSizeChangerContext>();

            context.PageSize = 25;
            Dispatcher.UIThread.RunJobs();
            source.PageSize.ShouldBe(25);
            pagination.PageSize.ShouldBe(25);

            source.PageSize = 40;
            Dispatcher.UIThread.RunJobs();
            pagination.PageSize.ShouldBe(40);
            context.PageSize.ShouldBe(40);
        });
    }

    [Fact]
    public void Pagination_Custom_Size_Changer_Projects_Zero_As_Default_Until_Positive_Request()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total               = 100,
            PageSize            = 0,
            IsShowSizeChanger   = true,
            SizeChangerTemplate = CreateSizeChangerTemplate(),
            IsMotionEnabled     = false
        };

        ShowInWindow(pagination, () =>
        {
            var context = FindCustomSizeChanger(pagination).Tag.ShouldBeOfType<PaginationSizeChangerContext>();
            pagination.PageSize.ShouldBe(0);
            context.PageSize.ShouldBe(AbstractPagination.DefaultPageSize);

            context.PageSize = AbstractPagination.DefaultPageSize;
            Dispatcher.UIThread.RunJobs();

            pagination.PageSize.ShouldBe(AbstractPagination.DefaultPageSize);
            context.PageSize.ShouldBe(AbstractPagination.DefaultPageSize);
        });
    }

    [Fact]
    public void SimplePagination_Allows_Custom_PageSize()
    {
        var pagination = new SimplePagination
        {
            Total = 10
        };

        var exception = Record.Exception(() => pagination.PageSize = 3);

        exception.ShouldBeNull();
        pagination.PageSize.ShouldBe(3);
        pagination.PageCount.ShouldBe(4);
    }

    [Fact]
    public void Pagination_Allows_Custom_PageSize()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total = 10
        };

        var exception = Record.Exception(() => pagination.PageSize = 3);

        exception.ShouldBeNull();
        pagination.PageSize.ShouldBe(3);
        pagination.PageCount.ShouldBe(4);
    }

    [Fact]
    public void Pagination_Rejects_Negative_PageSize()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination();

        Should.Throw<ArgumentException>(() => pagination.PageSize = -1);
    }

    [Fact]
    public void Pagination_SizeChanger_Includes_And_Selects_Custom_PageSize()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total             = 10,
            PageSize          = 3,
            IsShowSizeChanger = true,
            IsMotionEnabled   = false
        };

        ShowInWindow(pagination, () =>
        {
            var sizeChanger = pagination.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .Single();
            var pageSizes = sizeChanger.Items
                                       .Select(x => GetPageSize(x!))
                                       .ToArray();

            pageSizes.ShouldBe([3, 10, 20, 50, 100]);
            sizeChanger.SelectedIndex.ShouldBe(0);
            GetPageSize(sizeChanger.SelectedItem!).ShouldBe(3);
        });
    }

    [Fact]
    public void Pagination_SizeChanger_Uses_Configured_PageSizeOptions()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total             = 100,
            PageSize          = 5,
            PageSizeOptions   = [5, 15, 30],
            IsShowSizeChanger = true,
            IsMotionEnabled   = false
        };

        ShowInWindow(pagination, () =>
        {
            var sizeChanger = pagination.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .Single();
            var pageSizes = sizeChanger.Items
                                       .Select(x => GetPageSize(x!))
                                       .ToArray();

            pageSizes.ShouldBe([5, 15, 30]);
            GetPageSize(sizeChanger.SelectedItem!).ShouldBe(5);
        });
    }

    [Fact]
    public void Pagination_SizeChanger_Inserts_Current_PageSize_When_Not_In_Configured_Options()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total             = 100,
            PageSize          = 3,
            PageSizeOptions   = [5, 10],
            IsShowSizeChanger = true,
            IsMotionEnabled   = false
        };

        ShowInWindow(pagination, () =>
        {
            var sizeChanger = pagination.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .Single();
            var pageSizes = sizeChanger.Items
                                       .Select(x => GetPageSize(x!))
                                       .ToArray();

            pageSizes.ShouldBe([3, 5, 10]);
            GetPageSize(sizeChanger.SelectedItem!).ShouldBe(3);
        });
    }

    [Fact]
    public void Pagination_SizeChanger_Rebuilds_When_PageSizeOptions_Changes()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total             = 100,
            PageSize          = 6,
            PageSizeOptions   = [6, 12],
            IsShowSizeChanger = true,
            IsMotionEnabled   = false
        };

        ShowInWindow(pagination, () =>
        {
            var sizeChanger = pagination.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .Single();

            pagination.PageSizeOptions = [3, 9];
            Dispatcher.UIThread.RunJobs();

            var pageSizes = sizeChanger.Items
                                       .Select(x => GetPageSize(x!))
                                       .ToArray();

            pageSizes.ShouldBe([6, 3, 9]);
            GetPageSize(sizeChanger.SelectedItem!).ShouldBe(6);
        });
    }

    [Fact]
    public void Pagination_SizeChanger_Does_Not_Rebuild_Items_During_PageSize_Selection()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination
        {
            Total             = 100,
            PageSize          = 5,
            PageSizeOptions   = [5, 10, 20],
            IsShowSizeChanger = true,
            IsMotionEnabled   = false
        };

        ShowInWindow(pagination, () =>
        {
            var sizeChanger = pagination.GetVisualDescendants()
                                        .OfType<AtomUIComboBox>()
                                        .Single();

            var exception = Record.Exception(() =>
            {
                sizeChanger.SelectedIndex = 1;
                Dispatcher.UIThread.RunJobs();
            });

            exception.ShouldBeNull();
            pagination.PageSize.ShouldBe(10);
            GetPageSize(sizeChanger.SelectedItem!).ShouldBe(10);
        });
    }

    [Fact]
    public void Pagination_Rejects_Invalid_PageSizeOptions()
    {
        var pagination = new AtomUI.Desktop.Controls.Pagination();

        Should.Throw<ArgumentException>(() => pagination.PageSizeOptions = [0, 10]);
        Should.Throw<ArgumentException>(() => pagination.PageSizeOptions = [10, -20]);
    }

    [Fact]
    public void SimplePagination_Template_Assigns_Next_Item_Type()
    {
        var pagination = new SimplePagination
        {
            Total           = 100,
            CurrentPage     = 2,
            IsMotionEnabled = false
        };

        ShowInWindow(pagination, () =>
        {
            var previousItem = pagination.GetVisualDescendants()
                                         .OfType<PaginationNavItem>()
                                         .Single(item => item.Name == "PART_PreviousNavItem");
            var nextItem = pagination.GetVisualDescendants()
                                     .OfType<PaginationNavItem>()
                                     .Single(item => item.Name == "PART_NextNavItem");

            previousItem.PaginationItemType.ShouldBe(PaginationItemType.Previous);
            nextItem.PaginationItemType.ShouldBe(PaginationItemType.Next);
        });
    }

    [Fact]
    public void SimplePagination_Editable_Jump_Uses_Default_PageSize_When_PageSize_Is_Zero()
    {
        var pagination = new SimplePagination
        {
            Total           = 100,
            CurrentPage     = 1,
            PageSize        = 0,
            IsReadOnly      = false,
            IsMotionEnabled = false
        };

        ShowInWindow(pagination, () =>
        {
            var quickJumper = pagination.GetVisualDescendants()
                                        .OfType<QuickJumpEdit>()
                                        .Single(item => item.Name == "PART_QuickJumper");

            quickJumper.Text = "3";

            var exception = Record.Exception(() =>
            {
                quickJumper.RaiseEvent(new KeyEventArgs
                {
                    RoutedEvent  = InputElement.KeyUpEvent,
                    Source       = quickJumper,
                    Key          = Key.Enter,
                    PhysicalKey  = PhysicalKey.Enter,
                    KeyModifiers = KeyModifiers.None
                });
                Dispatcher.UIThread.RunJobs();
            });

            exception.ShouldBeNull();
            pagination.CurrentPage.ShouldBe(3);
        });
    }

    private static int GetPageSize(object item)
    {
        var pageSizeProperty = item.GetType()
                                   .GetProperty(
                                       "PageSize",
                                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        pageSizeProperty.ShouldNotBeNull($"Expected {item.GetType().Name} to expose a PageSize property.");
        return (int)pageSizeProperty.GetValue(item)!;
    }

    private static FuncDataTemplate<PaginationSizeChangerContext> CreateSizeChangerTemplate(
        IList<PaginationSizeChangerContext>? observedContexts = null)
    {
        return new FuncDataTemplate<PaginationSizeChangerContext>((context, _) =>
        {
            var nonNullContext = context.ShouldNotBeNull();
            observedContexts?.Add(nonNullContext);
            return new Border
            {
                Name = "CustomPageSizeChanger",
                Tag  = nonNullContext
            };
        });
    }

    private static Border FindCustomSizeChanger(AtomUI.Desktop.Controls.Pagination pagination)
    {
        return pagination.GetVisualDescendants()
                         .OfType<Border>()
                         .Single(control => control.Name == "CustomPageSizeChanger");
    }

    private sealed class PageSizeBindingSource : INotifyPropertyChanged
    {
        private int _pageSize;

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize == value)
                {
                    return;
                }

                _pageSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PageSize)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        ShowInWindow(content, _ => assertion());
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 220,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

}
