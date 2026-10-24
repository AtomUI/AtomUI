using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using Avalonia.Media;
using Avalonia.LogicalTree;

namespace AtomUI.Theme.Resources;

// The real inheritance parent is protected and may differ from the logical parent.
// Product controls expose only this read-only bridge; ControlTheme owns final values.
internal interface IThemeInheritanceSource
{
    AvaloniaObject? ThemeInheritanceParent { get; }
}

internal static class ControlTypographyBindings
{
    public static readonly BindingBase InheritedFontFamily = new CompiledBindingExtension(
        new CompiledBindingPathBuilder().Self().Property(
            new ClrPropertyInfo("InheritedFontFamilySource", value => ObserveInherited((Control)value, TextElement.FontFamilyProperty),
                null, typeof(IObservable<FontFamily>)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .StreamObservable<FontFamily>().Build());

    public static readonly BindingBase InheritedFontSize = new CompiledBindingExtension(
        new CompiledBindingPathBuilder().Self().Property(
            new ClrPropertyInfo("InheritedFontSizeSource", value => ObserveInherited((Control)value, TextElement.FontSizeProperty),
                null, typeof(IObservable<double>)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .StreamObservable<double>().Build());

    public static readonly BindingBase InheritedForeground = new CompiledBindingExtension(
        new CompiledBindingPathBuilder().Self().Property(
            new ClrPropertyInfo("InheritedForegroundSource", value => ObserveInherited((Control)value, TextElement.ForegroundProperty),
                null, typeof(IObservable<IBrush?>)), PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
            .StreamObservable<IBrush?>().Build());

    public static readonly IMultiValueConverter FontFamilyConverter = new OptionalFontFamilyConverter();
    public static readonly IMultiValueConverter FontSizeConverter = new OptionalFontSizeConverter();
    public static readonly IMultiValueConverter PresetFontSizeConverter = new SizeTypeFontSizeConverter();
    public static readonly IMultiValueConverter ForegroundConverter = new OptionalForegroundConverter();

    private static IObservable<T> ObserveInherited<T>(Control owner, StyledProperty<T> property) => Observable.Create<T>(observer =>
    {
        if (owner is not IThemeInheritanceSource source)
        {
            observer.OnError(new InvalidOperationException("This theme requires an actual inheritance-parent source."));
            return Disposable.Empty;
        }
        IDisposable? parentSubscription = null;
        void StopParent()
        {
            parentSubscription?.Dispose();
            parentSubscription = null;
        }
        void Refresh()
        {
            StopParent();
            var parent = source.ThemeInheritanceParent;
            if (parent is null)
            {
                observer.OnNext(property.GetDefaultValue(owner));
            }
            else
            {
                parentSubscription = parent.GetObservable(property).Subscribe(observer.OnNext, observer.OnError);
            }
        }
        void Attached(object? sender, LogicalTreeAttachmentEventArgs args) => Refresh();
        void Detached(object? sender, LogicalTreeAttachmentEventArgs args) => StopParent();
        owner.AttachedToLogicalTree += Attached;
        owner.DetachedFromLogicalTree += Detached;
        var parentChanges = owner.GetObservable(StyledElement.ParentProperty).Subscribe(_ => Refresh());
        return Disposable.Create(() =>
        {
            StopParent();
            parentChanges.Dispose();
            owner.AttachedToLogicalTree -= Attached;
            owner.DetachedFromLogicalTree -= Detached;
        });
    });

    private sealed class OptionalFontSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            object inherited = values.Count > 0 && values[0] is double size ? size : AvaloniaProperty.UnsetValue;
            // Owners without size presets supply inheritance, presence and value.
            if (values.Count == 3)
            {
                return values[1] is double && values[2] is double scalar ? scalar : inherited;
            }
            var hasCustomFlag = values.Count != 8;
            if ((values.Count != 8 && values.Count != 9 && values.Count != 12) || (hasCustomFlag && values[2] is not false) ||
                values[1] is not CustomizableSizeType sizeType)
            {
                return inherited;
            }
            // A public owner can expose Own+Global triples or Global-only pairs.
            var stride = values.Count == 12 ? 3 : 2;
            var group = sizeType switch
            {
                CustomizableSizeType.Small => 1,
                CustomizableSizeType.Large => 2,
                _ => 0
            };
            var offset = (hasCustomFlag ? 3 : 2) + group * stride;
            var isExplicit = values[offset] is double || (stride == 3 && values[offset + 1] is double);
            // Some composite inputs historically rendered the middle preset even when
            // their root inherited a different size. Keep that default, while the
            // IsCustomFontSize opt-out above still returns actual inheritance.
            var fallback = parameter is "PresetFallback" && values[(hasCustomFlag ? 3 : 2) + stride - 1] is double preset
                ? preset : inherited;
            return isExplicit && values[offset + stride - 1] is double configured ? configured : fallback;
        }
    }

    private sealed class SizeTypeFontSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 4 || values[0] is not CustomizableSizeType sizeType)
            {
                return AvaloniaProperty.UnsetValue;
            }
            var offset = sizeType switch
            {
                CustomizableSizeType.Small => 2,
                CustomizableSizeType.Large => 3,
                _ => 1
            };
            return values[offset] is double size ? size : AvaloniaProperty.UnsetValue;
        }
    }

    private sealed class OptionalFontFamilyConverter : IMultiValueConverter
    {
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var inherited = values.Count > 0 && values[0] is FontFamily family ? family : FontFamily.Default;
            return values.Count == 3 && values[1] is FontFamily && values[2] is FontFamily configured
                ? configured : inherited;
        }
    }

    private sealed class OptionalForegroundConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var inherited = values.Count > 0 ? values[0] : AvaloniaProperty.UnsetValue;
            return values.Count == 3 && values[1] is IBrush && values[2] is IBrush configured
                ? configured : inherited;
        }
    }
}
