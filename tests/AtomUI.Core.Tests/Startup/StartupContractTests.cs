using AtomUI.Theme;
using Avalonia;
using Avalonia.Headless;
using Shouldly;
using Xunit;

namespace AtomUI.Core.Tests.Startup;

// 契约：AtomUI 内核在 Application.Initialize() 之前提交——Prism 在 Initialize 中创建并显示 Shell，App.axaml 也会引用
// AtomUI 资源，提交一旦推迟到 Initialize 之后，它们拿不到主题与本地化。仓库内其他测试应用都手写挂接，这里同时覆盖
// 生成器为 partial 应用生成的 RegisterServices 挂接。
public class StartupContractTests
{
    // headless 会话的 dispatch 完成回调内联执行：必须阻塞式 dispatch，否则会话线程会在 Dispose 中等待自身。
    [Fact]
    public async Task Generated_hook_commits_before_Initialize_and_reaches_Ready()
    {
        var session = HeadlessUnitTestSession.StartNew(typeof(HookedEntry), AvaloniaTestIsolationLevel.PerTest);
        try
        {
            await session.Dispatch(() =>
            {
                var application = (TestApplication)Application.Current!;
                application.ThemeManagerSeenInInitialize.ShouldBeTrue();
                application.GetStartup().State.ShouldBe(StartupState.Ready);
                application.GetStartup().Completion.IsCompletedSuccessfully.ShouldBeTrue();
            }, TestContext.Current.CancellationToken);
        }
        finally
        {
            await Task.Run(session.Dispose, TestContext.Current.CancellationToken);
        }
    }

    public static class HookedEntry
    {
        public static AppBuilder BuildAvaloniaApp() =>
            AppBuilder.Configure<TestApplication>()
                      .UseHeadless(new AvaloniaHeadlessPlatformOptions())
                      .UseAtomUI(builder => builder.WithInitialTheme(IThemeManager.DEFAULT_THEME_ID));
    }
}
