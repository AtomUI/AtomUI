# CT14 N3-P：Pagination 默认函数迁移

**Goal:** 按既定 V2 方案迁移14项默认，保留现有值、资源与消费者；两项输入接线作为独立获批步骤取证。

**Spec:** [容器 N3](container-consumers.md)与[Own默认求值](own-defaults.md)。新行为仅按独立[输入短设计](pagination-input-wiring-design.md)取得批准后实施。

**Architecture / Tech Stack:** 现有 PaginationToken 接入 DependencyDefaultsV2／Revision1／IControlTokenDefaults；Avalonia、C#、NET10测试与NET8兼容。SimplePagination 仍以独立 Global identity 显式复用 Pagination Own；不新增 Own scope。

## 固定范围

14项都是常量／有效Global，只含 ColorBgContainer、ControlHeight／SM／LG、ColorTextDisabled、ControlItemBgActiveDisabled、UniformlyMarginXS／XXS。纯机制阶段原四项未消费字段保留默认及配置往返，不因机制迁移自动标为 Active 已交付、自动接线或移除；随后独立批准并接通Mini宽度与输入背景，剩余ItemLinkBg／InputOutlineOffset兼容保留。没有 Own→Own 边。

源代码只改 PaginationToken.cs 的默认声明，字段顺序、类型、ID／key／枚举数值／slot、公式与显式下游保持。L0 机制不改变模板、绑定、导航、输入或状态；Small默认50→44与明确输入背景生效属于另一个L2步骤，批准前不实施。

## 执行步骤

- [x] 原Pagination精确filter基线；临时验证V2合同先RED，明暗14默认、原始／投射类型、key／slot、Global／Control Global、14Own、0／清除兼容分别记录。
- [x] 接入14个静态默认函数，同一合同GREEN，默认／资源逐项对照；纯机制阶段不改变待批接线反例。
- [x] 对实际产品Window／真实明暗、普通与简洁及既有尺寸／状态作同历史A/A、1DIP与新旧／恢复核对，当前Mac managed／NativeAOT分开验证资源与持有状态。
- [x] 临时源、外部工程与媒体清理；只跑原Pagination精确filter、NET8、完整Token文件成员审计。正式文档／CSV／总表与作者自审同步，具体Git提交另审。

全程使用当前93ef4ba8d所在checkout／feature/custom-theme，单agent，不创建分支／worktree或发布NuGet。完整Pagination、其他Global／字体、输入及连续动效、N4、平台与原生最终像素不由本片关闭。

本机实现、57项清理后模块及NET8通过；用户已批准独立接线设计，并于2026-10-26批准[结果表](pagination-input-wiring-results.md)所示视觉修复及N01–N10精确限定差异。四份临时C#、419文件原生工程及159媒体文件已清理；本机切片出口完成，不扩大到完整家族或Git授权。
