# 架构基础

本目录保存对整个 AtomUI 解决方案生效的基础架构边界。

- [项目依赖关系](dependency-graph.md)：解决方案项目、主要项目引用和内部可见性。
- [运行平台策略](runtime-platforms.md)：Desktop、Browser、Native 和 Mobile 的能力边界。
- [启动与注册链路](startup-and-registration.md)：AppBuilder 默认配置、Builder、Provider 和生成池的注册顺序；启动编排见
  [启动系统](../systems/startup/overview.md)。
- [构建与打包](build-and-packaging.md)：Target Framework、版本、Analyzer、生成输出和 NuGet 包边界。
- [AOT 与裁剪架构](aot-and-trimming.md)：TypeMap 架构、本地实现与验证状态及发布门槛。
- [TypeMap 注册管线](aot-typemap-registration.md)：普通生成器、包引导、条件映射与启动收集。
- [控件与资源注册契约](control-registration-contracts.md)：逐控件片段、Token owner、语义描述、主题导出与资源顺序。
- [Browser TypeMap 链接](aot-browser-linking.md)：浏览器精细裁剪后端、工具链边界与实际运行验收。

具体跨模块业务系统进入 `architecture/systems/`；单个项目的源码组织进入 `modules/`。
