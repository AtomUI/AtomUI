using System.Globalization;
using System.Reactive;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.LineEdit;

public class LineEditViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "LineEdit";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private string? _otpLineEditBoundValue = "123456";
    private string? _otpLineEditBoundValueSummary;

    public string? OtpLineEditBoundValue
    {
        get => _otpLineEditBoundValue;
        set
        {
            this.RaiseAndSetIfChanged(ref _otpLineEditBoundValue, value);
            UpdateOtpLineEditBoundValueSummary();
        }
    }

    public string? OtpLineEditBoundValueSummary
    {
        get => _otpLineEditBoundValueSummary;
        set => this.RaiseAndSetIfChanged(ref _otpLineEditBoundValueSummary, value);
    }

    public ReactiveCommand<Unit, Unit> SetOtpLineEditBoundValueCommand { get; }
    public ReactiveCommand<Unit, Unit> ClearOtpLineEditBoundValueCommand { get; }

    public Func<string, string> OtpLineEditUppercaseFormatter { get; } = value => value.ToUpperInvariant();

    public LineEditViewModel(IScreen screen)
    {
        HostScreen                         = screen;
        SetOtpLineEditBoundValueCommand   = ReactiveCommand.Create(HandleSetOtpLineEditBoundValue);
        ClearOtpLineEditBoundValueCommand = ReactiveCommand.Create(HandleClearOtpLineEditBoundValue);
        UpdateOtpLineEditBoundValueSummary();
    }

    private void HandleSetOtpLineEditBoundValue()
    {
        OtpLineEditBoundValue = "654321";
    }

    private void HandleClearOtpLineEditBoundValue()
    {
        OtpLineEditBoundValue = null;
    }

    private void UpdateOtpLineEditBoundValueSummary()
    {
        var displayValue = string.IsNullOrEmpty(OtpLineEditBoundValue)
            ? Lang(LineEditShowCaseLangResourceKind.OtpLineEditEmptyValueText)
            : OtpLineEditBoundValue;
        OtpLineEditBoundValueSummary = GalleryLocalization.Format(
            LineEditShowCaseLangResourceKind.OtpLineEditCurrentValueFormat,
            "Current value: {0}",
            displayValue);
    }

    private static string Lang(LineEditShowCaseLangResourceKind kind)
    {
        return Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(kind) ?? kind.ToString()
            : kind.ToString();
    }
}

public sealed class OtpLineEditSeparatorBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int cellIndex && cellIndex % 2 != 0
            ? Brushes.Red
            : Brushes.Blue;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
