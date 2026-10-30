# OtpLineEdit Token 设计

本文档定义 `OtpLineEditToken` 在 OtpLineEdit 一次性验证码输入控件中的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。OtpLineEdit 整体架构见 [OtpLineEdit 桌面版架构设计](overview.md)，内部实现原理见 [OtpLineEdit 桌面版实现原理](implementation.md)，设计和契约变化记录见 [OtpLineEdit Changelog](changelog.md)。

## 1. 定位

OtpLineEdit 使用 `OtpLineEditToken` 表达 OTP 分格输入的专属布局语义。它只定义 cell 宽度和 cell 间距，不承载验证码文本、separator 内容、active cell、mask、placeholder、Form 状态、validation error、focus、hover、pressed 或 disabled 等运行时状态。

输入表面的颜色、边框、背景、focus shadow、disabled、error 和 warning 语义统一复用 `InputControlFrameTheme` 和 SharedToken。OtpLineEditToken 不复制这些已有输入体系 Token。

## 2. Token 分类

### 2.1 Cell 宽度 Token

- `OtpLineEdit.CellWidth`
- `OtpLineEdit.CellWidthLG`
- `OtpLineEdit.CellWidthSM`

这些 Token 控制单个验证码 cell 的默认宽度。cell 高度由输入家族 `SizeType` 和 SharedToken 控制，宽度单独定义是为了保证短码输入的位数视觉稳定。

### 2.2 Cell 间距 Token

- `OtpLineEdit.CellGap`
这些 Token 控制相邻 cell 之间的默认水平间距。当前模板使用 `CellGap`；`SizeType` 只改变 cell 宽度和字号，separator 的 `Margin="4,0"` 属于模板固定布局，不进入 Token。

## 3. 控件专项模型中的 Token 使用

OtpLineEditToken 使用路径：

```text
SharedToken
   ↓
OtpLineEditToken
   ↓
OtpLineEditTheme / OtpLineEditCellTheme
   ↓
cell width + cell gap
```

`SizeType=Large/Middle/Small` 选择对应 cell 宽度 Token；字号由 SharedToken 的输入字号分支提供。`SizeType=Custom` 默认归入 Middle 分支；用户显式设置 `Width`、`Height`、`FontSize` 或主题资源时，由 Avalonia 属性优先级控制最终视觉。

mask 字符使用与 cell 文本相同的字体和字号，不定义独立 mask Token。placeholder 使用输入家族占位文本颜色，不定义独立 placeholder Token。

## 4. 控件家族影响

OtpLineEditToken 影响以下控件或主题：

- `OtpLineEditTheme.axaml`
- `OtpLineEditCellTheme.axaml`
- Gallery OtpLineEdit 示例
- OtpLineEdit token.md 语义说明

OtpLineEditToken 不影响 LineEdit、SearchEdit、TextArea 或其他输入控件的尺寸和主题。其他控件需要 OTP 风格分格输入时，应直接使用 OtpLineEdit，而不是复制 Token 或 cell 主题。

## 5. 兼容性要求

OtpLineEditToken 属于 OtpLineEdit 的主题契约。即使 Token 类型是 internal，生成的 TokenKind、AXAML resource 使用点、Token 类型、生成数据和 token.md 都会形成稳定依赖。

Token 变更要求：

- 不擅自重命名、删除或迁移既有 Token。
- 不把运行时状态、实例值、active cell、Form 状态或 validation error 写成 Token。
- 不在 OtpLineEditToken 中复制 `InputControlFrame` 的边框、背景、focus shadow、error、warning 或 disabled 状态 Token。
- 不在 OtpLineEditToken 中表达 `Length`、当前文本长度、是否完成、separator 内容或模板固定外边距。
- `SizeType=Custom` 不新增专属 Token；默认复用 Middle 分支，由用户显式属性完成自定义。
- 需要破坏性变更时，必须先说明影响范围并获得授权。

## 6. 验证策略

| 改动类型 | 验证要求 |
| --- | --- |
| 修改 cell 宽度 Token | 验证 Large/Middle/Small/Custom 下的 cell 宽度、字符居中、focus ring 和 error 状态。 |
| 修改 cell 间距 Token | 验证相邻 cell、清除按钮、Form feedback 和整体布局不重叠。 |
| 修改 separator 模板布局 | 验证静态 separator、模板 separator 和不同 SizeType 的对齐。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |
