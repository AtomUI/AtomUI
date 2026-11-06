using System.Reactive;
using ReactiveUI;

namespace AtomUI.Toolkits.GalleryBase.Infrastructure;

// Keep Gallery's public System.Reactive.Unit command signatures when ReactiveUI
// uses RxVoid for its Action/Task convenience overloads. Explicit generic
// factories retain the original argument, result, errors and can-execute stream.
internal static class GalleryCommands
{
    internal static ReactiveCommand<Unit, Unit> Create(Action execute, IObservable<bool>? canExecute = null)
    {
        return ReactiveCommand.Create<Unit, Unit>(_ =>
        {
            execute();
            return Unit.Default;
        }, canExecute);
    }

    internal static ReactiveCommand<T, Unit> Create<T>(Action<T> execute, IObservable<bool>? canExecute = null)
    {
        return ReactiveCommand.Create<T, Unit>(argument =>
        {
            execute(argument);
            return Unit.Default;
        }, canExecute);
    }

    internal static ReactiveCommand<Unit, Unit> CreateFromTask(Func<Task> execute)
    {
        return ReactiveCommand.CreateFromTask<Unit, Unit>(async _ =>
        {
            await execute();
            return Unit.Default;
        });
    }

    internal static ReactiveCommand<T, Unit> CreateFromTask<T>(Func<T, Task> execute)
    {
        return ReactiveCommand.CreateFromTask<T, Unit>(async argument =>
        {
            await execute(argument);
            return Unit.Default;
        });
    }
}
