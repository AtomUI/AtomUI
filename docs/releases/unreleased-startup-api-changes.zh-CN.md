# 待发布启动 API 迁移

本文记录工作树中已经批准、尚未发布的启动 API 变更，不分配发布版本，也不修改包版本。
后续发布必须明确说明以下源码及二进制兼容性破坏。

English version: [Pending startup API migration](unreleased-startup-api-changes.md).

## 移除的 API

移除 `IAtomUIStartupEvents`、`AtomUIStartupEvents`、`StartupStage`、`StartupStageEventArgs`，以及
`Application.GetStartupEvents()` 和 `IAtomUIBuilder.GetStartupEvents()`。删除配置期间的事件订阅并重新编译消费者。
`Deferred(Action)` 继续作为声明入口，保留原有阶段能力校验。

## 等待共享启动结果

`UseAtomUI` 后通过 `Application.GetStartup()` 获取该应用的 `IAtomUIStartup`：

```csharp
var startup = application.GetStartup();
try
{
    await startup.Completion.WaitAsync(windowCancellation);
    BuildContent();
}
catch (OperationCanceledException) when (windowCancellation.IsCancellationRequested)
{
    // 当前窗口停止等待，其他观察者和应用启动继续。
}
catch (Exception exception)
{
    ShowStartupError(exception);
}
```

`State` 包含 `Configuring`、`WaitingForHost`、`Initializing`、`Ready`、`Failed`、`Canceled`。
同步启动在 `UseAtomUI` 返回时已经 Ready；分阶段模式在宿主就绪、框架初始化与终态清理完成后结束。
晚观察者读取相同结果；失败任务保留原始异常，应用启动取消得到 canceled 任务。Ready 后释放应用服务不会改写终态。
`UseAtomUI` 同步阶段抛出的错误仍同步传播。

将 `StageTwoCompleted`/`StageTwoFailed` 处理改为任务的成功/异常路径。`ProgressChanged` 只用于可选 UI 线程进度；
不重放过去事件，订阅者异常不改变启动结果。调用者的 `WaitAsync` 取消只结束自身等待。
尚未注册 AtomUI 时调用 `GetStartup()` 会抛明确错误。

框架就绪不代表业务内容已绘制。Gallery 单独调度 Shell 构建，关窗时取消自身等待与排队操作。
完整语义见[启动架构](../architecture/foundations/startup-and-registration.md)。
