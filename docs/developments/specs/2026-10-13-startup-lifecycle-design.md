# 统一启动生命周期契约（已批准）

2026-10-13：用户明确选择“按统一契约重构”，允许破坏性启动 API 迁移。本方案替代兼容增量 WaitForStartupAsync。

## 公共契约

```csharp
public enum AtomUIStartupState
{
    Configuring,
    WaitingForHost,
    Initializing,
    Ready,
    Failed,
    Canceled
}

public interface IAtomUIStartup
{
    AtomUIStartupState State { get; }
    Task Completion { get; }
    event EventHandler<AtomUIStartupProgressEventArgs>? ProgressChanged;
}

// ApplicationExtensions：未 UseAtomUI 时抛明确错误。
public static IAtomUIStartup GetStartup(this Application application);

public sealed class AtomUIStartupProgressEventArgs : EventArgs
{
    public AtomUIStartupState State { get; }
    public string Detail { get; }
}
```

Completion 是框架就绪结果。成功为 RanToCompletion，失败保留原始异常，取消为 Canceled；每个 Application
只有一个结果，迟到观察者不会丢失。调用者使用 `Completion.WaitAsync(cancellationToken)` 只取消自身等待。
ProgressChanged 只提供可选 UI 线程进度，逐订阅者隔离异常，不能作为读取终态的唯一依据。

## 内部所有权

ApplicationScope 持有一个 internal AtomUIStartup 实现；状态转换、Completion 和通知都由该对象控制。
删除 scope 的重复 `_stageTwoState` 和无结果 completion，以及 Builder 的独立 StartupEvents 实例。
单阶段走 Configuring→Ready；分阶段走 Configuring→WaitingForHost→Initializing→Ready；
允许进入 Failed/Canceled 终态，终态不可覆盖。启动服务释放不把已 Ready 改成 Canceled。

成功、失败和取消先关闭注册输入/释放等待者，再发布终态结果。Stage 2 的主题/语言提交继续复用事务执行门，
Startup 不接管主题和语言状态。初始化线程、资源优先级、包粒度、net8/net10 与 AOT 不变。

## 迁移与删除

- 删除 `IAtomUIStartupEvents`、`AtomUIStartupEvents`、`StartupStage`、`StartupStageEventArgs` 及两个 GetStartupEvents 扩展。
- Deferred 作为声明入口继续存在，仍执行阶段能力校验；其参数类型变更不属于本方案。
- Gallery 构造加载态后 await `Application.GetStartup().Completion.WaitAsync(windowCancellation)`。
  成功排队构建内容，失败显示错误，窗口退出取消等待。启动已完成后再建窗口也立即获得同一结果。
- 同步、分期、等待期间关闭、运行中关闭、失败、晚窗口与调用者取消都做临时契约验证；旧测试迁移到结果契约。
- 更新正式启动架构、Gallery 接入文档；清楚记录公开 API 破坏性变更。

## 验证与成本

原始晚订阅问题用现有事件复现；新接口先建立会失败的目标契约，再迁移实现和消费者。独立等待者取消不得取消
应用启动；一次失败不能被任务吞掉或改报成功。状态与 Task 结果必须一致，所有终态释放等待对象。
不为单阶段引入后台工作或调度；对象分配成本应纳入当前启动测量，不能仅以 API 更整洁替代验证。
