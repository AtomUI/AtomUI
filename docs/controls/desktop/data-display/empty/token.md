# Empty Token 设计

本文档定义 Empty 相关控件 Token 的专属语义、分类、使用范围和兼容边界。控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。Empty 整体架构见 [Empty 桌面版架构设计](overview.md)，内部实现原理见 [Empty 桌面版实现原理](implementation.md)，设计和契约变化记录见 [Empty Changelog](changelog.md)。

## 1. 定位

Empty Token 只表达预设图片高度、描述间距和 Footer 间距。内置图形颜色直接使用 SharedToken 输入，不由 EmptyToken
另行定义。Token 不承载图片来源、图片生成结果、资源就绪状态或控件挂树状态。

当前 Token scope：

- `EmptyToken`，scope id 为 `Empty`，源码位于 `src/AtomUI.Desktop.Controls/Empty/EmptyToken.cs`。

## 2. Token 分类

Token 按控件语义分类维护：

| 分类 | 语义 | 代表 Token |
| --- | --- | --- |
| 尺寸与密度 | 三档 SizeType 的 image 高度。 | `EmptyImgHeight`、`EmptyImgHeightSM`、`EmptyImgHeightMD` |
| 间距与布局 | image、description 与 footer 之间的稳定间距。 | `DescriptionMargin`、`DescriptionMarginSM`、`FooterMargin` |

## 3. 控件专项模型中的 Token 使用

Empty 的控件专项模型通过 Theme 消费 Token：

- AXAML/ControlTheme 负责把 EmptyToken 映射到 image 高度、description margin 和 footer margin。
- EmptyToken 默认值从 SharedToken 派生，不直接读取控件实例状态。
- Token 类型、生成数据和 token.md 应显式维护，不依赖运行时反射扫描。

预设图形的颜色绑定由 `EmptyTheme.axaml` 声明：

| 内部颜色输入 | SharedToken |
| --- | --- |
| `BorderColor` | `ColorFill` |
| `BorderColorSecondary` | `ColorBorderSecondary` |
| `ShadowColor` | `ColorFillTertiary` |
| `ContentColor` | `ColorFillQuaternary` |
| `BgColor` | `ColorBgElevated` |

`AbstractEmpty` 消费这些输入进行颜色叠加并生成 SVG。资源暂缺、恢复和重新挂树的结果提交规则由
[图片计算与结果提交](implementation.md#81-图片计算与结果提交)统一定义；不通过新增 Token 或固定颜色保存图片状态。

## 4. 控件家族影响

调整 Empty Token 时必须评估以下范围：

- `Empty`
- `Empty.Footer` 及其 Semantic footer 区域。
- 对应 Gallery ShowCase 的示例和源码片段。
- Light/Dark 主题、Browser/Desktop 主题和 Compact/Form/Popup 集成场景。

## 5. 兼容性要求

- 不删除或重命名已生成的 TokenKind、TokenResource key 和 AXAML 引用。
- 不把实例状态、交互状态或 `EffectiveXxx` 状态写成 Token。
- 不在 Token 中展开颜色、variant 和状态的组合矩阵；组合关系应由 Theme selector 表达。
- Token 默认值变更必须同步评估 Gallery 示例和截图可观察外观。
- 如需引入新 Token，必须同步 Token 类型、生成资源、主题引用和本文档。

## 6. 验证策略

CT15 的 `EmptyToken` 使用 DependencyDefaultsV2 revision 1，六个 Own 字段的名称、类型、资源 key、slot 及四算法默认保持。`EmptyImgHeight` / `EmptyImgHeightSM` / `EmptyImgHeightMD` 分别使用当前 Empty Global 的 `ControlHeightLG × 2.5 / 0.875 / 1.85`；两个描述 margin 与 Footer margin 保持原公式。高度 0 只折叠图片占用，不替换图片来源。

预设 Default/Simple 的五个画刷输入与描述的 `ColorTextDescription` 从 Empty owner 的 Global 解析。图片颜色仍走原生成器和背景混色公式；自定义 `ImageSource` / `ImagePath` 保留调用方 SVG 内容、命名色与固有比例，不接受预设插画 palette 的重着色。Root 字体按原生值、继承与明确 Empty Global 的既有可选字体规则解析；描述与 Footer 继续使用各自原角色。专用 `EmptyImageStyle`、`EmptyDescriptionStyle`、`EmptyFooterStyle` 覆盖实际目标，清除后回到当前 Token 或继承值。

限定验证与尚未覆盖的边界见 [CT15 Empty 结果](../../../../superpowers/plans/2026-10-22-custom-theme/empty-consumers-results.md)。局部分包或默认迁移不构成完整 CT15 通过。

| 改动类型 | 验证要求 |
| --- | --- |
| Token 文档 | `git diff --check`，检查相对链接存在。 |
| Token 默认值 | 运行对应控件测试，走查 Light/Dark 和 Browser 主题。 |
| Token 名称或数量 | 检查 generated TokenResource key、AXAML 引用和 token.md。 |
| 主题映射 | 走查 hover、pressed、selected、disabled、loading 等状态视觉。 |
