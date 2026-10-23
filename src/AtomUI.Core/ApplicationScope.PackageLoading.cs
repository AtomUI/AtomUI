using AtomUI.Localization;
using AtomUI.Registration;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Threading;

namespace AtomUI;

internal sealed partial class ApplicationScope
{
    public void LoadPackages(Action<IAtomUIBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        Dispatcher.UIThread.VerifyAccess();
        ThrowIfDisposed();
        if (_loadingPackages || !_registrationCompleted || Volatile.Read(ref _initialized) == 0)
        {
            throw new InvalidOperationException("Package loading requires an initialized, idle application scope.");
        }
        ThemeManager.BeginPackageLoading();
        var languageEntered = false;
        _loadingPackages = true;
        AtomUIBuilder? builder = null;
        ThemeManager.PackageUpdate? themes = null;
        LanguageManager.PackageUpdate? languages = null;
        var services = new List<IAtomUIOwnedService>();
        var attached = 0;
        var rolledBack = false;

        void Rollback(ICollection<Exception> failures)
        {
            if (rolledBack)
            {
                return;
            }
            rolledBack = true;
            TryCleanup(() => languages?.Rollback(), failures);
            TryCleanup(() => themes?.Rollback(), failures);
            if (builder is not null)
            {
                foreach (var package in builder.PackageDeclarations.Reverse())
                {
                    TryCleanup(package.Release, failures);
                }
            }
            for (var index = attached - 1; index >= 0; index--)
            {
                var service = services[index];
                TryCleanup(() => service.Detach(_application!), failures);
            }
            for (var index = services.Count - 1; index >= 0; index--)
            {
                TryCleanup(services[index].Dispose, failures);
            }
        }

        try
        {
            LanguageManager.BeginPackageLoading();
            languageEntered = true;
            builder = new AtomUIBuilder(_application!.GetType(), ThemeManager.EnabledPackageIds, _ownedServiceIds);
            configure(builder);
            builder.ControlRegistrationState.ThrowIfUnavailable();
            if (builder.PackageDeclarations.Count == 0 && builder.ThemeManagerBuilder.ControlPackages.Count == 0 &&
                builder.OwnedServiceRegistrations.Count == 0 && builder.LocalizationBuilder.ModuleData.Count == 0 &&
                builder.ThemeManagerBuilder.StyleAssets.Count == 0 &&
                builder.LocalizationBuilder.Catalogs.Count == 0 && builder.LocalizationBuilder.TranslationBundles.Count == 0)
            {
                return;
            }
            languages = LanguageManager.PreparePackages(builder.LocalizationBuilder);
            themes = ThemeManager.PreparePackages(builder.ThemeManagerBuilder);
            foreach (var registration in builder.OwnedServiceRegistrations)
            {
                services.Add(registration.Factory(_application) ??
                    throw new InvalidOperationException($"Owned service '{registration.Id}' returned null."));
            }
            var ownership = new PackageOwnership(builder.PackageDeclarations.ToArray(), services.ToArray());
            _additionalOwnership.EnsureCapacity(_additionalOwnership.Count + 1);
            _ownedServiceIds.EnsureCapacity(_ownedServiceIds.Count + builder.OwnedServiceRegistrations.Count);
            var publication = new PackagePublication();
            themes.RunResourceUpdate(() =>
            {
                try
                {
                    languages.Stage(publication);
                    themes.Stage(publication, languages.RunCommittedRead);
                    foreach (var package in builder.PackageDeclarations)
                    {
                        package.PrepareContext(_application, ThemeManager);
                    }
                    for (var index = 0; index < services.Count; index++)
                    {
                        // Attach may acquire ownership before throwing; the attempted service must detach too.
                        attached = index + 1;
                        services[index].Attach(_application);
                    }
                    foreach (var package in builder.PackageDeclarations)
                    {
                        package.Commit(new ControlPackageContext(_application, ThemeManager));
                    }
                    themes.EnsureCurrent();
                }
                catch (Exception error)
                {
                    var failures = new List<Exception> { error };
                    Rollback(failures);
                    ThrowFailures("Package append and rollback failed.", failures);
                }
            });
            _additionalOwnership.Add(ownership);
            _ownedServiceIds.UnionWith(builder.OwnedServiceRegistrations.Select(registration => registration.Id));
            // Every fallible hook/attach/resource finalization has finished. The staged read
            // views share this fence, so background readers switch generations together.
            publication.Complete();
            themes.Commit();
            languages.Commit();
            themes.CompleteCommit();
            languages.CompleteCommit();
            themes.Publish();
        }
        catch (Exception error)
        {
            var failures = new List<Exception> { error };
            if (themes is not null)
            {
                TryCleanup(() => themes.RunResourceUpdate(() => Rollback(failures)), failures);
            }
            else
            {
                Rollback(failures);
            }
            ThrowFailures("Package append and rollback failed.", failures);
        }
        finally
        {
            builder?.ControlRegistrationState.Close();
            builder?.LocalizationBuilder.Close();
            if (languageEntered)
            {
                LanguageManager.EndPackageLoading();
            }
            ThemeManager.EndPackageLoading();
            _loadingPackages = false;
        }
    }

    private void ReleaseAdditionalOwnership(Application application, ICollection<Exception> failures)
    {
        for (var batch = _additionalOwnership.Count - 1; batch >= 0; batch--)
        {
            var ownership = _additionalOwnership[batch];
            for (var index = ownership.Packages.Count - 1; index >= 0; index--)
            {
                TryCleanup(ownership.Packages[index].Release, failures);
            }
            for (var index = ownership.Services.Count - 1; index >= 0; index--)
            {
                var service = ownership.Services[index];
                TryCleanup(() => service.Detach(application), failures);
            }
            for (var index = ownership.Services.Count - 1; index >= 0; index--)
            {
                TryCleanup(ownership.Services[index].Dispose, failures);
            }
        }
        _additionalOwnership.Clear();
    }

    private sealed record PackageOwnership(IReadOnlyList<EnabledControlPackage> Packages,
        IReadOnlyList<IAtomUIOwnedService> Services);
}
