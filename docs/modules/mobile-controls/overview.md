# AtomUI.Mobile.Controls 模块概览

AtomUI Mobile 位于独立仓库 [AtomUI/AtomUI.Mobile](https://github.com/AtomUI/AtomUI.Mobile)，以 NuGet 方式消费本仓库的 `AtomUI.Controls` 等包。包职责、上游集成、启动打包与平台 adapter由该仓库的
`docs/modules/mobile-controls/overview.md` 维护，本仓库不再保存 Mobile 预实现架构或能力清单的副本。

本仓库只维护 Mobile 依赖的上游契约：`AtomUI.Core`、`AtomUI.Controls.Shared`、`AtomUI.Controls` 与 `AtomUI.Native` 对
`AtomUI.Mobile.Controls` 开放内部成员，见 [项目依赖关系](../../architecture/foundations/dependency-graph.md)。
