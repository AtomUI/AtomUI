# Reactive 依赖迁移

当前依赖为 ReactiveUI.Avalonia 12.1.6、ReactiveUI 26.0.1、
ReactiveUI.Binding 9.1.0、ReactiveUI.Primitives 9.0.0、Splat 21.0.0。
升级处理原生发布中属性观察器、表达式重写和路由解析的裁剪/AOT诊断；没有新增警告屏蔽。
源码版本和验证边界见[评估记录](../../superpowers/audits/2026-11-06-windows-servicing/reactive-evaluation.md)。

## 外部源码影响

`IViewFor`、`IViewLocator`、`DefaultViewLocator` 和 `ViewLocator` 现在位于
`ReactiveUI.Binding` 命名空间与程序集。原来完整限定 `ReactiveUI.IViewFor` 的代码需要迁移；
包的全局 using 不能消除完整限定名称的变化。`AvaloniaScheduler` 位于
`ReactiveUI.Primitives.Concurrency`。不要依赖旧包传递提供 DynamicData。

GalleryBase 已公开的 `ReactiveCommand<System.Reactive.Unit, System.Reactive.Unit>`
等签名保持原有 Unit 类型。内部 `GalleryCommands` 使用显式泛型工厂，避免新默认 RxVoid
改变签名；保留 canExecute、任务异常及取消传播。该内部文件由 Gallery 工程链接使用，
没有扩展产品公共 API。

## 路由和生命周期

Gallery 的每个 `RoutedViewHost` 使用内部 `GalleryRouteViewLocator`。默认 contract 的已注册
ViewModel 精确类型始终调用产品注册的页面工厂；命名 contract 和非路由类型委托给配置的全局
locator。没有替换全局定位器，也没有禁用源生成器。

新依赖的生成映射先于 `DefaultViewLocator.Map` 执行。Card 的共用 ViewModel 曾因此命中高级
示例而覆盖显式工厂；实际 Windows Gallery 上的失败和修复后全路由运行证据证明了此适配的必要性。
宿主负责设置 ViewModel，页面激活/退役继续由 RoutedViewHost 与现有 GalleryReactiveUserControl 管理。

Windows x64 托管和原生验证分别执行。聚焦检查不等于全量依赖兼容验收；其他平台、架构和
全量回归必须有各自的执行证据。
