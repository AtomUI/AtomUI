# CT14 Pagination 小号输入与背景短设计（设计已批准）

沿用现有 CT14 计划，当前基线 93ef4ba8d。产品源码与模板尚未修改；本片独立于已执行的第十八批 Git 授权，不创建分支／worktree。

用户已明确回复“批准推荐的两项接线与两项兼容保留”；实施记录见[接线与视觉结果](pagination-input-wiring-results.md)。本设计正文保留实施前条件，不表示视觉／Git批准。

## 已查明的当前行为

Pagination 有 14 个 Own，全部默认只依赖 Global 或常量。10 项有现行消费者，4 项此前为 NeedsDecision。SimplePagination 有独立 Global identity，没有 Own，现有模板显式复用 Pagination Own 的尺寸／间距资源；本设计保留该边界。

本地参考按规定顺序找到 `/Users/chinboy/Projects/ReferenceProjects/ant-design`，commit `211c6ed29b0d2abd18485e293681f8a4e415240b`，未用 GitHub 替换本地版本。参考实现 small 的普通跳转输入及 simple 输入消费 mini 宽度，simple 输入消费 itemInputBg；itemLinkBg 在 bordered 导航分支使用，inputOutlineOffset 是焦点 box-shadow 的横向标量偏移。参考实现只用于澄清意图，AtomUI 证据如下。

临时驱动用公开 ThemeConfigProvider 配置、实际明暗主题事务和产品 Window，保留 Total500／PageSize10／CurrentPage6、Small、普通快速跳转开启／size changer 关闭、Simple.IsReadOnly=false。原模块 57 项通过。原版两个临时用例分别记录三个拟修复合同失败：Mini 默认资源44、常规资源50，但普通输入 Width50／Bounds50、简洁输入 MinWidth50／Bounds50；Mini Own90 和 ItemInputBg Purple 已实际进入资源表，两输入仍50，简洁绘制背景仍 Light.White／Dark.#141414。清除后恢复原值、页码不变，输入节点引用与可见性保持。

背景以真实 `AddOnDecoratedBoxContentFrame` 的 PixelAlignedBorder 观测；初期把 LineEdit.Background、Border／TemplatedControl 当作绘制目标的失败只是探针准备问题，不算合同反例。完整树与边界已保存，产品没有回写目标节点。

## 方案与取舍

1. 只迁移14项 V2，保留四项未消费：默认画面不变，但小号宽度和明确输入背景仍无效。
2. **推荐**：机械迁移与两项接线分开验证，接通既有 Small 和 simple 编辑输入；另两项保留兼容，不新增 bordered 或焦点偏移 API。修复范围明确，代价是已有小号输入的默认尺寸变化及明确背景配置开始生效。
3. 把四项全部接上：需要新增 bordered 导航能力，并为 Thickness 定义新的桌面焦点偏移语义，超出本片，不采用。

## 推荐方案的具体边界

- 14 项默认按既定 Global-only 方案接入 V2，保留字段、默认公式、类型、identity、资源 ID／枚举数值／slot、明确值和撤回。
- 普通 QuickJumperBar 的 Small 输入仍设置 Width，但改读 PaginationMiniQuickJumperInputWidth；Simple 的 Small 编辑输入仍设置 MinWidth，改读同一资源。当前默认分别从50改44 DIP；Simple 仍可随内容变宽。Large／Middle／Custom 保留常规宽度；不改字号、页码、页大小、跳转事件或输入校验。
- ItemInputBg 仅接 Simple 编辑输入现有 Outlined／enabled 绘制帧；默认仍来自有效 Pagination Global.ColorBgContainer，明确 Own 背景生效，清除恢复。disabled 颜色、焦点边框／阴影、用户其他变体保留。通过 AXAML ControlTheme 的现有模板链声明，不在 code-behind 查节点回写样式。
- ItemLinkBg 归为当前默认主题未消费的兼容保留项：AtomUI 没有 bordered 导航入口，保持资源、默认和配置往返，不新增该变体。
- InputOutlineOffset 同样兼容保留：当前是 Thickness，不能直接当作参考实现的 box-shadow 横向数值；保留现有 InputActiveShadow 路线，不发明四边偏移或反射接口。
- 保留现有 root／item／info 专用 Semantic Style 及应用本地尺寸的适用优先级，不新增签名、Token、Part、伪类或 C# 绑定。若现有尺寸／Style 优先级不能保持，先修订方案。

## 执行与验收

设计等级 L2：小号输入默认宽度及明确背景配置是可观察变化，需要批准后才修改产品模板。先复用上述原版反例，验证两项接线及合法0／透明／撤回、普通与简洁、现有尺寸／禁用／只读、输入／焦点／跳转及 Style 覆盖；机制迁移的旧新默认14值、key／slot 与真实消费者独立核对。

实际明暗同历史做自然 A/A、1 DIP、新旧／恢复比较，所示预期视觉差异另交审查；Mac managed／NativeAOT、临时清理后的 Pagination 精确模块／NET8／成员与文档审查分别记录。设计批准不表示视觉出口、Git 提交或完整 CT14／CT25 验收。
