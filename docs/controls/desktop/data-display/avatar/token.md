# Avatar Token 设计

本文档定义 Avatar 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Avatar 整体架构见 [Avatar 桌面版架构设计](overview.md)，内部实现原理见 [Avatar 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Avatar Changelog](changelog.md)。

## 1. 定位

Avatar Token 只表达组件级视觉变量，例如尺寸、间距、颜色、圆角、阴影、图标尺寸和弹层边界。Token 不承载运行时选择、展开、加载、错误、上传任务、过滤条件或业务状态。

当前 Token scope：

- `AvatarToken`，scope id 为 `Avatar`，源码位于 `src/AtomUI.Controls/Avatar/AvatarToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 控件高度、宽度、图标尺寸、内容最小尺寸。 | `ContainerSize`、`ContainerSizeLG`、`ContainerSizeSM`、`TextFontSize`、`TextFontSizeLG`、`TextFontSizeSM` |
| 间距与布局 | 单头像组间距和重叠距离。 | `GroupSpace`、`GroupOverlapping` |
| 颜色与状态视觉 | 文本、背景、边框、hover、selected、active、disabled 视觉。 | `GroupBorderColor`、`AvatarBg`、`AvatarColor` |
| 结构与装饰 | 圆角、阴影、指示器、弹层和装饰线相关变量。 | 按源码 Token 语义维护 |

未出现在上表中的 Token 仍按源码中的组件语义维护，不按代码顺序机械分类。

## 3. 控件专项模型中的 Token 使用

Avatar 的控件专项模型通过 Theme 消费 Token：

- C# 控件负责状态归一和伪类同步。
- AXAML/ControlTheme 负责把 Token 映射到背景、前景、边框、padding、尺寸和动效。
- Token 默认值通过 DependencyDefaultsV2/revision1 从 owner Global 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

## 4. 控件家族影响

调整 Avatar Token 时必须评估以下范围：

- `Avatar`
- `AvatarGroup`
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。
- `AvatarToken` 由 `AtomUI.Controls` 拥有；Desktop 的 `AvatarGroupTheme` 只能消费它，不能在 Desktop 再定义同名 Token。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |


## 7. CT15 字段消费者裁定

11 Own 的名称、type、key、slot与四算法默认公式保持，Math.Round仍采用原默认取整。ContainerSize/SM/LG进入三档真实宽高；TextFontSize/SM/LG保持现有 Icon 尺寸角色，不据字段名强行改变默认文字。GroupSpace/GroupOverlapping用于overflow排列和重叠；GroupBorderColor进入单头像边框；AvatarBg/AvatarColor用于原内容与fold颜色。

文字通过原生 Font 属性、继承及明确 Avatar Global FontFamily/FontSize 定制，仍由Gap约束的DownOnly Viewbox呈现。Square圆角来自Avatar Global，Circle来自实际宽度。AvatarGroup的边框厚度来自其自身Global；其余上述Own仍以Avatar identity解析。合法0、清空、三档/Custom、动态配置和实际值应以专项验证为准，不将默认迁移等同家族出口。


| Own | V2 默认输入 | 真实消费 |
| --- | --- | --- |
| ContainerSize / LG / SM | ControlHeight / LG / SM | Middle/Large/Small box宽高 |
| TextFontSize | Round((FontSizeLG+FontSizeXL)/2) | Middle EffectiveIconSize |
| TextFontSizeLG / SM | FontSizeHeading3 / FontSize | Large/Small EffectiveIconSize |
| GroupSpace | UniformlyMarginXXS | overflow StackPanel.Spacing |
| GroupOverlapping | UniformlyMarginXS | Group测量与排列重叠 |
| GroupBorderColor | ColorBorderBg | Avatar.BorderBrush |
| AvatarBg | ColorTextPlaceholder | Text/Icon背景与fold背景 |
| AvatarColor | ColorTextLightSolid | Avatar.Foreground与fold前景 |

明确字号与方形圆角可配置在 Avatar identity 下：

```csharp
new ThemeConfigBuilder().WithControl(AvatarTokens.Identity,
    new ControlThemeConfigBuilder()
        .WithToken("FontFamily", "Menlo")
        .WithToken("FontSize", "23")
        .WithToken("BorderRadius", "9")
        .Build()).Build();
```

原生入口仍可声明在 root Style 或实例上：

```xml
<atom:Avatar Text="Original complete text" Shape="Square"
             FontSize="17" FontFamily="Menlo" Gap="7" Size="48" />
```

不要用取得Frame/Icon/Image节点后赋值来代替公开入口；不要把TextFontSize三Own误写成自然文字字号。Group的Global使用其注册identity（`new ControlTokenIdentity("AtomUI", "AvatarGroup")`），不会通过Avatar identity替Group消费边框厚度。


CT15当前仅[限定表](../../../../superpowers/plans/2026-10-22-custom-theme/avatar-consumers-visual-review.md)48对有意变化已获批准，其他关闭/往返微差及完整Avatar/CT15保留，不授权提交。
