using Avalonia.Input;
using Avalonia.Headless;
using AtomUI.Controls.Commons;
using System.ComponentModel;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Rate;

public class RateBindingTests
{
    static RateBindingTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Value_Is_TwoWay_And_DataValidation_Enabled()
    {
        var metadata = Desktop.Controls.Rate.ValueProperty.GetMetadata(typeof(Desktop.Controls.Rate));

        metadata.DefaultBindingMode.ShouldBe(BindingMode.TwoWay);
        metadata.EnableDataValidation.ShouldBe(true);
    }

    [Fact]
    public void Value_DefaultBindingMode_Updates_ViewModel()
    {
        var viewModel = new RateBindingViewModel
        {
            Value = 2.0
        };
        var rate = new Desktop.Controls.Rate();
        rate.Bind(
            Desktop.Controls.Rate.ValueProperty,
            new Binding(nameof(RateBindingViewModel.Value))
            {
                Source = viewModel
            });

        ShowInWindow(rate, () =>
        {
            rate.Value.ShouldBe(2.0);

            rate.Value = 4.0;
            Dispatcher.UIThread.RunJobs();

            viewModel.Value.ShouldBe(4.0);

            var owner = TopLevel.GetTopLevel(rate).ShouldBeOfType<AvaloniaWindow>();
            var star = rate.GetVisualDescendants().OfType<RateItem>().First();
            var point = star.TranslatePoint(new Point(star.Bounds.Width / 2, star.Bounds.Height / 2), owner).ShouldNotBeNull();
            var other = new AvaloniaWindow { Width = 360, Height = 240, Content = new Border { Background = Brushes.Blue } };
            other.Show();
            try
            {
                other.MouseMove(point);
                other.MouseDown(point, MouseButton.Left);
                other.MouseUp(point, MouseButton.Left);
                rate.Value.ShouldBe(4.0);
                viewModel.Value.ShouldBe(4.0, "foreign-window input must not write back a rating");

                owner.MouseMove(point);
                owner.MouseDown(point, MouseButton.Left);
                owner.MouseUp(point, MouseButton.Left);
                rate.Value.ShouldBe(1.0);
                viewModel.Value.ShouldBe(1.0);
            }
            finally
            {
                other.Close();
            }
        });
    }

    [Fact]
    public void Template_Frame_Binds_To_BorderThickness()
    {
        var rate = new Desktop.Controls.Rate
        {
            BorderBrush     = Brushes.Red,
            BorderThickness = new Thickness(2)
        };

        ShowInWindow(rate, () =>
        {
            var frame = rate.GetVisualDescendants()
                            .OfType<PixelAlignedBorder>()
                            .Single(item => item.Name == "Frame");

            frame.BorderThickness.ShouldBe(new Thickness(2));
        });
    }

    private static void ShowInWindow(Control content, Action assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 360,
            Height  = 240,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion();
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class RateBindingViewModel : INotifyPropertyChanged
    {
        private double _value;

        public double Value
        {
            get => _value;
            set
            {
                if (Math.Abs(_value - value) < double.Epsilon)
                {
                    return;
                }

                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
