using AtomUI.Controls;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public partial class Select
{
    private CancellationTokenSource? _optionsLoadCTS;
    private bool _asyncOptionsLoaded;
    
    private void LoadOptionsAsync()
    {
        if (TryLoadOptionsAsync(OptionsAsyncLoadContext))
        {
            return;
        }
        var loadingEventArgs = new SelectOptionsLoadingEventArgs(OptionsAsyncLoadContext);
        NotifyOptionsLoading(loadingEventArgs);
        if (!loadingEventArgs.Cancel)
        {
            OptionsLoadComplete(OptionsAsyncLoadContext);
        }
    }
    
    protected virtual void NotifyOptionsLoading(SelectOptionsLoadingEventArgs e)
    {
        IsLoading = true;
        OptionsLoading?.Invoke(this, e);
    }
    
    protected virtual void NotifyOptionsLoaded(SelectOptionsLoadedEventArgs e)
    {
        IsLoading = false;
        OptionsLoaded?.Invoke(this, e);
    }
    
    private bool TryLoadOptionsAsync(object? context)
    {
        _optionsLoadCTS?.Cancel(false);
        _optionsLoadCTS?.Dispose();
        _optionsLoadCTS = null;
    
        if (OptionsLoader == null)
        {
            return false;
        }
    
        _optionsLoadCTS = new CancellationTokenSource();
        var task = LoadOptionAsync(context, _optionsLoadCTS.Token);
        if (task.Status == TaskStatus.Created)
        {
            task.Start();
        }
    
        return true;
    }
    
    private async Task LoadOptionAsync(object? context, CancellationToken cancellationToken)
    {
        try
        {
            if (OptionsLoader == null)
            {
                return;
            }

            var result     = await OptionsLoader.LoadAsync(context, cancellationToken);
            var resultList = result.Data;

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    SetCurrentValue(OptionsSourceProperty, resultList);
                    OptionsLoadComplete(context, result);
                    _asyncOptionsLoaded = true;
                }
            });
        }
        catch (TaskCanceledException e)
        {
            OptionsLoaded?.Invoke(this, new SelectOptionsLoadedEventArgs(context, new SelectOptionsLoadResult()
            {
                UserFriendlyMessage = e.Message,
                StatusCode          = RpcStatusCode.Cancelled
            }));
        }
        finally
        {
            _optionsLoadCTS?.Dispose();
            _optionsLoadCTS = null;
        }
    }
    
    private void OptionsLoadComplete(object? context, SelectOptionsLoadResult? loadResult = null)
    {
        // Fire the Populated event containing the read-only view data.
        var optionsLoaded = new SelectOptionsLoadedEventArgs(context, loadResult);
        NotifyOptionsLoaded(optionsLoaded);
    
        bool isDropDownOpen = loadResult?.Data?.Any() == true;
        if (isDropDownOpen != IsDropDownOpen)
        {
            IgnorePropertyChange = true;
            SetCurrentValue(IsDropDownOpenProperty, isDropDownOpen);
        }
        if (IsDropDownOpen)
        {
            OpeningDropDown(false);
        }
        else
        {
            ClosingDropDown(true);
        }
    }
}