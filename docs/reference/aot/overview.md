# AOT Reference

> 以下契约对应本地已实现的 TypeMap 注册体系；产品验证、发布状态与支持边界以
> [AOT 与裁剪架构](../../architecture/foundations/aot-and-trimming.md)为准。

本目录定义普通生成器、Core 注册代码与发布后端共同使用的生成 ABI。它面向框架和工具维护者；
应用与第三方控件作者通过普通包入口使用，不手写这些元数据。

| 文档 | 所有权 |
| --- | --- |
| [TypeMap 生成 ABI](typemap-contract.md) | 包 marker、映射 accessor、片段代理、身份和失败规则 |
| [条件注册生成 ABI](conditional-registration-contract.md) | net8 候选记录、规范 JSON/摘要、受控 collector 与输入快照；发布接线仍在验收 |

系统结构见 [TypeMap 注册体系](../../architecture/foundations/aot-typemap-registration.md)，类型与资源语义见
[控件注册契约](../../architecture/foundations/control-registration-contracts.md)，Browser 编译后端见
[Browser 链接架构](../../architecture/foundations/aot-browser-linking.md)，net8 工具与发布边界见
[.NET 8 条件后端](../../architecture/foundations/aot-net8-conditional-backends.md)。
