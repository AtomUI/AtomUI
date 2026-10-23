# OtpLineEdit Token 设计

本文档定义 `OtpLineEditToken` 在 OtpLineEdit 一次性验证码输入控件中的专属语义、分类、使用范围和兼容边界。共享输入表面分层见 [输入控件共享架构设计](../input-control-architecture-design.md)，控件 Token 的通用分层、命名、计算、Theme Variables 边界和预设色规则见 [AtomUI 控件 Token 设计规范](../../../../engineering/development/control-token-guidelines.md)。OtpLineEdit 整体架构见 [OtpLineEdit 桌面版架构设计](overview.md)，内部实现原理见 [OtpLineEdit 桌面版实现原理](implementation.md)，设计和契约变化记录见 [OtpLineEdit Changelog](changelog.md)。

## 1. 定位

OtpLineEdit 使用 `OtpLineEditToken` 表达 OTP 分格输入的专属布局与光标时间线。它定义 cell 宽度、cell 间距及光标周期/缓动，不承载验证码文本、separator 内容、active cell、mask、placeholder、Form 状态、validation error、focus、hover、pressed 或 disabled 等运行时状态。

输入表面的颜色、边框、背景、focus shadow、disabled、error和warning统一复用输入表面合同，消费OtpLineEdit身份的Effective Global。OtpLineEditToken不复制这些输入体系Token。

## 2. Token 分类

### 2.1 Cell 宽度 Token

- `OtpLineEdit.CellWidth`
- `OtpLineEdit.CellWidthLG`
- `OtpLineEdit.CellWidthSM`

这些Token控制单个验证码cell的默认宽度。高度由SizeType及OtpLineEdit有效Global ControlHeight三档控制，宽度独立定义以保证短码位数视觉稳定。

### 2.2 Cell 间距 Token

- `OtpLineEdit.CellGap`
该Token控制相邻cell的默认水平间距。SizeType选择对应宽度、高度和字号；separator左margin取有效Global MarginXXS.Left，默认4，与CellGap职责不同。

### 2.3 输入光标时间线

| Token | 类型／默认值 | 实际职责 |
| --- | --- | --- |
| `CaretBlinkDuration` | TimeSpan／1s | 完整的1→0→1周期；非正值停止闪烁，保留活动光标的静态可见反馈，不夹成最小时长。 |
| `CaretBlinkEasing` | Easing／LinearEasing | 原生Animation在完整周期上的缓动，沿既有easing codec；可变Spline/Spring参数按实例复制。 |

默认仍为1s线性往返。`IsMotionEnabled`控制装饰Transition，保留既有独立输入反馈，不关闭光标周期；需要静态光标时显式配置非正周期。隐藏时由原生动画暂停并保持相位；失焦、detach和模板退役时移除本实例的动画Style。周期/缓动有效值改变时替换该Style，等价参数、字体或颜色更新不重启时间线。活动、禁用及只读仍是控件状态，不写入Token。

## 3. 控件专项模型中的 Token 使用

OtpLineEditToken 使用路径：

```text
OtpLineEdit Effective Global
   ↓
OtpLineEditToken
   ↓
OtpLineEditTheme / OtpLineEditCellTheme
   ↓
cell width + cell gap + caret blink
```

SizeType=Large/Middle/Small选择对应cell宽度，字号使用OtpLineEdit有效Global FontSize/SM/LG。Custom默认归入Middle。CellWidth、CellBorderBrush、根BorderBrush及专用Cell Style按照各自原生属性优先级生效；根几何和字体按实际组合职责分别验证，不能以根属性存在代替cell消费证据。

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
| 修改光标时间线 | 验证实际周期/曲线、默认1s、非正值、清除恢复、隐藏暂停相位、只读/禁用/焦点及释放；静态截图不能替代时间线观测。 |
| 删除或重命名 Token | 默认不允许；如获授权，需同步 AXAML 引用、生成文件、Token 类型、生成数据和 token.md和控件文档。 |
| 文档改动 | 运行 `git diff --check`，检查文档链接存在。 |

## 默认求值协议

本Own定义使用DependencyDefaultsV2、Revision2，默认工厂以有效Global或声明常量为输入，各字段没有Own之间的依赖；显式Own覆盖优先。原四字段名称、类型、公共资源编号0–3及既有单位/范围保持，光标周期与缓动的公共编号追加为4/5，不因slot排序改变旧ID含义。Global变化时重新计算未显式覆盖的默认值，清除Own后恢复对应公式。

根操作区spacing、内部文本selection/placeholder及cell状态值同样使用OtpLineEdit typed Global。EnableMotion进入公开root，经既有CellInfo传递到所有cell；光标时间线使用独立Own。动态更新已有六cell，清除后恢复预设并保留文本与原生覆盖优先级。

## 单边几何消费

光标宽度使用有效Global LineWidth；Underlined边框使用有效Global BorderThickness.Bottom，其余边为零；separator使用有效Global MarginXXS.Left，其余边为零。零值和不对称Thickness均按原值投射，不强制最小1。默认分别为1、1、4。BorderThickness属于alias，control算法禁用时独立显式覆盖，不把LineWidth单字段覆盖误称为已重新计算alias。专用Style遵循[部件契约](semantic-part.md)，不新增重复Own字段。
