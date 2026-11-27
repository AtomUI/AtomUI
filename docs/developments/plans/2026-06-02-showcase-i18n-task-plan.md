# AtomUI Gallery ShowCase 多语言任务计划

本文档记录 AtomUI Gallery ShowCase 多语言完善工作的代码分析结果和实施计划。当前计划基于 `ShowCases/<Category>/<ShowCase>/Views|ViewModels|Localization` 的最新组织规范。

## 当前进度

| 项 | 结果 |
| --- | ---: |
| ShowCase 总数 | 71 |
| 已有 ShowCase 本地多语言 | 69 个，P1 标题/描述迁移全部完成；`Icon`、`Palette` 无 `ShowCaseItem` 标题/描述，P1 无需新增 Catalog |
| 仍有硬编码 `ShowCaseItem.Title` / `Description` 的 ShowCase | 0 个 |
| 待迁移 `ShowCaseItem.Title` | 0 处 |
| 待迁移 `ShowCaseItem.Description` | 0 处 |
| P2 主要可见演示文案 | 已完成；排除注释、示例数据、URL、金额、日期、选择值语义后，实际 UI 硬编码待迁移项为 0 |
| P3 复杂 ShowCase | 已完成；`Form`、`DataGrid`、`Space` 父级场景 Tab 已迁入本地资源，`Form` 运行时文案和局部辅助控件模板已接入当前语言 |
| P4 共享资源收敛 | 已完成；废弃的 `ShowCaseScenarioLang` 共享场景资源已删除 |
| P5 回归验证 | 已完成；资源一致性、关键残留扫描、Debug build 和短启动 smoke 均通过 |
| P6 POCO 选项动态语言切换 | 已完成；控件层已修复 `Cascader`、`CascaderView`、`Select`、`TreeSelect`、`TreeView`、`ListView`、`ListBox` 的数据源刷新/容器复用状态恢复；示例层内联本地化 POCO 选项迁移为 0 残留 |

当前所有 `ShowCaseItem.Title` / `Description` 均已按 ShowCase 本地 Catalog enum 和 `en-US`、`zh-CN`、`zh-TW` XLIFF 组织；`Icon`、`Palette` 没有 `ShowCaseItem` 标题和描述，P1 不需要迁移。P2 已完成控件内部按钮文字、占位符、校验消息、标题、提示、开关文案等主要可见演示文案迁移。`SelectOption.Content`、`DescriptionItem.Content`、URL、金额、日期、用户名等示例数据继续保留原值，避免破坏选择值、过滤值和演示数据语义。

## 分类工作量

| 分类 | ShowCase 数 | 已有本地多语言 | 硬编码标题/描述迁移量 | 优先级 |
| --- | ---: | ---: | ---: | --- |
| DataEntry | 18 | 18 | 0 / 0 | 已完成 P1 |
| DataDisplay | 22 | 22 | 0 / 0 | 已完成 P1 |
| Navigation | 8 | 8 | 0 / 0 | 已完成 P1 |
| Feedback | 11 | 11 | 0 / 0 | 已完成 P1 |
| Layout | 4 | 4 | 0 / 0 | 已完成 P1 |
| General | 8 | 6 | 0 / 0 | 已完成 P1，`Icon` / `Palette` 无 P1 迁移项 |

## 实施计划

| 阶段 | 范围 | 任务 | 产出 | 验证 |
| --- | --- | --- | --- | --- |
| P0 | 多语言契约 | 统一 `LanguageId`、资源类命名、资源 key 命名、XAML 引用方式 | 可复用迁移模板 | 选择 1-2 个 ShowCase 验证资源生成正常 |
| P1 | ShowCase 标题和描述 | 迁移所有 `ShowCaseItem.Title` / `Description` | 每个 ShowCase 拥有自己的 Catalog enum 和三种内置语言 XLIFF | 确认不再存在硬编码 `ShowCaseItem` 标题和描述 |
| P2 | 主要可见演示文案 | 迁移明显展示给用户的 `Content`、`Header`、`HeaderTitle`、`Watermark`、按钮文字、表单标签、校验消息、Tooltip、开关文案等 | 已完成；Gallery 主体演示文案可随语言切换 | `dotnet build` 通过；硬编码扫描 actionable=0 |
| P3 | 复杂 ShowCase | 处理 `Form`、`DataGrid`、`Space` 的子场景页面和局部辅助控件 | 已完成；子场景文案跟随对应 ShowCase 就近维护 | `dotnet build` 通过；P3 共享场景资源引用扫描为 0 |
| P4 | 共享资源收敛 | 删除已无实际引用的 `ShowCaseScenarioLang` 共享资源 | 已完成；避免共享资源持续膨胀，场景 Tab 文案统一就近维护 | `dotnet build` 通过；`ShowCaseScenario` 相关扫描为 0 |
| P5 | 回归验证 | Gallery Debug build、资源一致性扫描、关键残留扫描、短启动 smoke、语言切换路径核查 | 已完成；形成可提交的稳定批次 | `dotnet build` 通过；资源扫描和短启动 smoke 通过 |
| P6 | POCO 选项动态语言切换正确性 | 处理 `SelectOption`、`CascaderOption`、`TreeItemNode`、`AutoCompleteOption` 这类非 Avalonia 对象上的本地化静态化问题 | 控件层保持数据源刷新后的选择/默认/展开状态；示例层迁移内联选项到 ViewModel 数据源 | `dotnet build` 通过；语言切换后选项 Header、已选项、Tag、默认路径同步刷新 |

## 建议批次

| 批次 | 内容 | 目的 |
| --- | --- | --- |
| Batch 1 | `Button`、`AutoComplete`、`DataGrid` | 覆盖简单 ShowCase、输入类 ShowCase、复杂 tab ShowCase，先验证迁移模式 |
| Batch 2 | `DataEntry` 全部 | 文案量最大，优先完成高收益区域 |
| Batch 3 | `DataDisplay` 全部 | ShowCase 数量最多，和 `DataGrid` 场景相关 |
| Batch 4 | `Navigation` 和 `Feedback` | 已完成中等规模迁移 |
| Batch 5 | `Layout` 和 `General` 剩余项 | 已完成收尾，保留并复用 `AboutUs` 已有模式 |

## P2 执行记录

| 项 | 结果 |
| --- | --- |
| 本轮补齐范围 | `Form` 标签和校验消息、`Alert` / `Spin` 消息、`Result` 头部文案、`Tooltip` / `InfoFlyout` 提示和方位按钮、`LineEdit` 搜索按钮、`ToggleSwitch` / `Transfer` / `TreeSelect` 开关文案、`Drawer` / `Steps` / `List` / `Badge` / `Card` 等按钮和说明文案 |
| 本轮修正范围 | 修正脚本误把 `Avatar`、`Tag`、`Alert`、`HyperLinkTextBlock` 文本迁移为 `Content` 属性的问题，改回各控件真实文本属性 |
| 保留边界 | `SelectOption.Content`、`DescriptionItem.Content`、`AutoCompleteOption.Content`、URL、金额、日期、人名、选择值和 mock 数据不纳入 P2 翻译 |
| 验证结果 | `AtomUIGallery.Desktop` Debug build 通过；排除保留边界后的硬编码扫描结果为 `actionable=0` |

## P3 执行记录

| 项 | 结果 |
| --- | --- |
| 本轮补齐范围 | `Form`、`DataGrid`、`Space` 父级场景 Tab 由共享 `ShowCaseScenarioLangResource` 迁移到各自 ShowCase 本地资源 |
| Form 运行时文案 | 动态乘客标签、动态字段校验消息、提交成功/失败消息、性别联动提示改为从当前语言资源读取；动态乘客标签使用资源绑定，已创建字段也能随语言资源刷新 |
| Form 局部辅助控件 | `Captcha` 获取验证码按钮、`PriceInput` 货币下拉 Header 接入 `FormShowCaseLangResource`；`SelectOption.Content` 和 `DefaultValues` 继续保留原值语义 |
| 保留边界 | `DataGrid` 行数据、筛选样本、动态新增行内容继续作为 mock/value 数据保留，避免影响过滤、排序和示例逻辑 |
| 验证结果 | `AtomUIGallery.Desktop` Debug build 通过，0 warning / 0 error；`Form`、`DataGrid`、`Space` 中 `ShowCaseScenarioLangResource` 引用扫描为 0 |

## P4 执行记录

| 项 | 结果 |
| --- | --- |
| 本轮清理范围 | 删除 `ShowCaseScenario.cs` 和 `ShowCases/Localization/ShowCaseScenarioLang` 的 Catalog 与 XLIFF |
| 生成资源收敛 | 生成的 Catalog 模块注册入口不再包含 `ShowCaseScenarioLang`；对应 enum、XLIFF 和扩展类均已删除 |
| 维护边界 | 场景 Tab 文案继续由各自 ShowCase 的本地 `Localization` 维护，不再新增公共场景名资源 |
| 验证结果 | `AtomUIGallery.Desktop` Debug build 通过，0 warning / 0 error；`ShowCaseScenarioLangResource`、`ShowCaseScenarioLang`、`ShowCaseScenario` 扫描结果为 0 |

## P5 执行记录

| 项 | 结果 |
| --- | --- |
| 资源一致性 | 检查 71 组 ShowCase Catalog/XLIFF，`en-US` / `zh-CN` / `zh-TW` unit mismatch 为 0，空资源值为 0 |
| XAML 资源引用 | 扫描 ShowCase XAML 中的 `*LangResource` 引用，无法匹配生成资源枚举的引用为 0 |
| 标题描述回归 | 精确扫描非 Localization 代码中的 `ShowCaseItem.Title` / `Description` 硬编码，结果为 0 |
| P3/P4 关键残留 | `ShowCaseScenarioLangResource`、`ShowCaseScenarioLang`、`ShowCaseScenario` 残留为 0；`Form` / `DataGrid` / `Space` 关键共享资源引用为 0 |
| 语言切换路径 | `WorkspaceWindow` 通过 `ILanguageManager.ChangeLanguage(LanguageTags.*)` 切换；XAML 使用 Catalog 生成的 `*LangResource` 扩展，非视觉数据由 `ILocalizer` 在 `LanguageChanged` 后重建 |
| 构建和启动 | `AtomUIGallery.Desktop` Debug build 通过，0 warning / 0 error；最终构建产物短启动 8 秒 smoke 通过，无启动崩溃 |

## P5 补充修正记录

| 项 | 结果 |
| --- | --- |
| Button 资源值漏扫 | 修正 `Button` ShowCase 中已接入资源但中文资源值仍为英文的按钮文案，包括 `Primary Button`、`Default Button`、`Text Button`、`Link Button`、`Click me!`、Danger/Ghost/Disabled 系列按钮内容 |
| Button 尺寸标签 | 将英文资源 `Expand direction:` 修正为更符合场景的 `Button size:`，中文资源为 `按钮尺寸：` |
| SplitButton 内联内容漏扫 | 修正 `SplitButton` ShowCase 中直接写在控件内容里的 `Hover me`、`Large`、`Middle`、`Small`、`Default`、`Primary`、`Click Me`，统一改为本地资源绑定 |
| SplitButton 资源补齐 | 新增 `P2ContentHoverMe`、`P2ContentLarge`、`P2ContentMiddle`、`P2ContentSmall`、`P2ContentDefault`、`P2ContentPrimary`、`P2ContentClickMe` 中英文资源 |
| Separator 资源值漏扫 | 修正 `Separator` ShowCase 中已接入资源但中文资源值仍为英文的 `Left Text with 0 orientationMargin`、`Right Text with 50px orientationMargin` 和 Lorem 示例段落 |
| CustomizeTheme 资源值漏扫 | 修正 `CustomizeTheme` ShowCase 中已接入资源但中文资源值仍为英文的 `Primary Button`、`Default Button`、`Text Button`、`Link Button` |
| DropdownButton 内联内容漏扫 | 修正 `DropdownButton` ShowCase 中直接写在按钮内容里的 `Hover me`、`Edit File`、`BottomLeft`、`Bottom`、`BottomRight`、`TopLeft`、`Top`、`TopRight`，统一改为本地资源绑定；补齐中文资源值中的 `Paste` / `Paste from History` 翻译 |
| Menu 资源值和运行时数据漏扫 | 修正 `Menu` ShowCase 中中文资源值仍为英文的菜单项、上下文提示、导航项和切换标签；`ItemsSource` / `MenuFlyout` / `NavMenu` 运行时数据改为从本地资源读取，并在语言切换后重建 |
| CheckBox 资源值和运行时数据漏扫 | 修正 `CheckBox` ShowCase 中中文资源值仍为英文的 `Checkbox`、`UnChecked`、`Indeterminate`、`Check all`；受控复选框状态按钮和 `CheckBoxGroup.ItemsSource` 运行时数据改为从本地资源读取，并在语言切换后刷新 |
| DatePicker 资源值漏扫 | 修正 `DatePicker` ShowCase 中已接入资源但中文资源值仍为英文的占位符、尺寸选项、变体占位符和弹出位置选项；将尺寸标签英文资源从 `Expand direction:` 修正为 `Picker size:` |
| TimePicker 资源值漏扫 | 修正 `TimePicker` ShowCase 中已接入资源但中文资源值仍为英文的 `Select time`、`Outline`、`Borderless` 等占位符和变体文案 |
| Form 资源值和帮助文本漏扫 | 修正 `Form` ShowCase 中已接入资源但中文资源值仍为英文的性别、国家、颜色、日期时间占位符、树/级联节点、登录注册、表单状态、控件标签和校验消息；补齐 `FormItem.Help` 的本地资源绑定 |
| LineEdit 资源值漏扫 | 修正 `LineEdit` ShowCase 中已接入资源但中文资源值仍为英文的输入框占位符、变体标题、搜索框文案、文本域占位符；修正英文资源中误写中文的搜索按钮文案 |
| Mentions 资源值漏扫 | 修正 `Mentions` ShowCase 中已接入资源但中文资源值仍为英文的变体占位符、触发提示、禁用和只读占位符 |
| NumberUpDown 资源值漏扫 | 修正 `NumberUpDown` ShowCase 中已接入资源但中文资源值仍为英文的占位符和状态前缀占位符；将 `Raw:` 显示标签迁入本地资源 |
| RadioButton 资源值和运行时数据漏扫 | 修正 `RadioButton` ShowCase 中已接入资源但中文资源值仍为英文的单选框、图表、选项和城市文案；`ItemsSource` 运行时数据改为从本地资源读取，并在语言切换后刷新 |
| Rate 资源值和运行时数据漏扫 | 修正 `Rate` ShowCase 中已接入资源但中文资源值仍为英文的允许清除标签；评分 tooltip 运行时数据改为从本地资源读取，并在语言切换后刷新 |
| Select 资源值和运行时数据漏扫 | 修正 `Select` ShowCase 中已接入资源但中文资源值仍为英文的占位符、选项 Header、国家描述、变体文案、分组选项和附加内容；基础选项、异步选项和长标签运行时数据改为从本地资源读取，并在语言切换后刷新 |
| ToggleSwitch 资源值漏扫 | 修正 `ToggleSwitch` ShowCase 中中文资源值仍为英文的切换禁用和切换加载按钮文案；修正英文资源中误写中文的开关文本 |
| TreeSelect 资源值和运行时数据漏扫 | 修正 `TreeSelect` ShowCase 中中文资源值仍为英文的弹出位置、位置选项和前缀文案；树节点 Header、异步加载子节点 Header 改为从本地资源读取，并在语言切换后重建 |
| Transfer 资源值和运行时数据漏扫 | 修正 `Transfer` ShowCase 中 Source/Target 标题、Reload 按钮、Tag 列标题和 One way 文案；列表项内容、描述和表格示例数据改为从本地资源读取，并在语言切换后重建 |
| Upload 资源值和运行时消息漏扫 | 修正 `Upload` ShowCase 中上传按钮、最大数量、目录上传、PNG 上传等中文资源值；上传校验失败原因、成功提示和默认任务错误信息改为从本地资源读取，并在语言切换后刷新默认任务列表 |
| Alert 资源值和场景标题漏扫 | 修正 `Alert` ShowCase 中中文资源值仍为英文的提示正文、详细描述、操作按钮和循环横幅文案；补齐 `Icon` 独立资源，避免带图标示例误显示为含描述信息场景 |
| Drawer 资源值漏扫 | 修正 `Drawer` ShowCase 中中文资源值仍为英文的抽屉标题、正文内容、弹出位置标签、二级抽屉按钮、当前区域提示和预设尺寸按钮文案；尺寸数值和单位作为演示值保留 |
| Message 运行时消息漏扫 | 修正 `Message` ShowCase 中点击按钮后弹出的普通、信息、成功、警告、错误、加载中和加载完成消息硬编码英文；运行时 `AtomUIMessage.Content` 改为从本地资源读取 |
| Notification 资源值和运行时通知漏扫 | 修正 `Notification` ShowCase 中中文资源值仍为英文的按钮、位置和悬停选项文案；点击按钮后弹出的 `AtomUINotification` 标题和正文改为从本地资源读取 |
| Result 资源值和副标题漏扫 | 修正 `Result` ShowCase 中中文资源值仍为英文的结果标题、按钮和错误说明文案；将 `Result.SubHeader` 中直接硬编码的成功、403、404、500 和提交失败说明迁入本地资源 |
| Skeleton 资源值漏扫 | 修正 `Skeleton` ShowCase 中中文资源值仍为英文的控制项标签、尺寸/形状选项、示例说明段落和显示骨架屏按钮文案 |
| Spin 资源值和 Tip 漏扫 | 修正 `Spin` ShowCase 中中文资源值仍为英文的 Alert 描述和加载状态标签；将 `Spin.Tip` 中直接硬编码的 `Loading...` 迁入本地资源 |
| Watermark 内联长文案漏扫 | 修正 `Watermark` ShowCase 中多行水印文本和自定义配置示例说明直接硬编码英文的问题，统一迁入本地资源 |
| Avatar 资源值漏扫 | 修正 `Avatar` ShowCase 中中文资源值仍为英文的 `USER`、`ChangeUser`、`ChangeGap`；`U`、`K`、`A` 作为头像字母示例保留 |
| Badge 资源值漏扫 | 修正 `Badge` ShowCase 中中文资源值仍为英文的状态、动态按钮、Ribbon 示例、颜色名称和分组标题；`#f50`、`rgb(...)`、`hsl(...)` 作为颜色演示值保留 |
| Card 资源值漏扫 | 修正 `Card` ShowCase 中中文资源值仍为英文的尺寸标题、卡片标题、Tab 标题、分类标题和内容描述；`www.instagram.com` 作为 URL 示例保留 |
| Collapse 资源值漏扫 | 修正 `Collapse` ShowCase 中中文资源值仍为英文的面板标题、尺寸分组、可折叠触发区域标题、正文示例和展开图标位置文案 |
| Descriptions 资源值和字段漏扫 | 修正 `Descriptions` ShowCase 中中文资源值仍为英文的头部、配置描述和硬件信息文案；将 `DescriptionItem.Label` 字段名、状态值、地址等可见自然语言改为本地资源绑定 |
| DataGrid 资源值和标题页脚漏扫 | 修正 `DataGrid` ShowCase 中中文资源值仍为英文的列头、操作、筛选项、分组表头、按钮、分页设置和拖拽场景标题；将表格 Title/Footer 从硬编码文本改为本地资源绑定 |
| Expander 资源值漏扫 | 修正 `Expander` ShowCase 中中文资源值仍为英文的面板标题、尺寸分组、嵌套标题、触发区域标题、正文示例、展开方向和展开图标位置文案 |
| GroupBox 资源值漏扫 | 修正 `GroupBox` ShowCase 中中文资源值仍为英文的标题信息和分组框内容文案 |
| InfoFlyout 资源值漏扫 | 修正 `InfoFlyout` ShowCase 中中文资源值仍为英文的浮层正文、触发按钮和箭头显示/隐藏按钮文案 |
| List 资源值和运行时数据漏扫 | 修正 `List` ShowCase 中选择模式、搜索占位符、新闻列表、空状态按钮、筛选值、颜色和分组等可见文案；运行时生成的列表数据改为从本地资源读取，并在语言切换后刷新 |
| Segmented 资源值漏扫 | 修正 `Segmented` ShowCase 中中文资源值仍为英文的周期、地图模式、列表/看板和长文本文案；修正英文资源中误混入中文的示例文本，并将中文字符资源 key 改为英文命名 |
| Statistic 资源值和时间格式漏扫 | 修正 `Statistic` ShowCase 中中文资源值仍为英文的统计标题、卡片状态、计时器标题和按钮文案；将 Day Level Timer 的时间单位格式迁入本地资源，避免英文界面显示中文时间单位 |
| Tag 资源值漏扫 | 修正 `Tag` ShowCase 中中文资源值仍为英文的预设/自定义分组、颜色名称、状态标签、无图标标题、阻止默认行为和边框示例标签文案；社交品牌名和十六进制颜色值作为演示值保留 |
| Timeline 资源值和 Pending 文案漏扫 | 修正 `Timeline` ShowCase 中中文资源值仍为英文的时间线事件、反转按钮、模式选项和标签示例文案；将 `Pending` 中硬编码的 `Recording...` 迁入本地资源，并清理描述中不符合 AtomUI 语境的 React 术语 |
| TreeView 资源值和运行时数据漏扫 | 修正 `TreeView` ShowCase 中显示连接线/图标、节点悬停模式、右键选择、模板树节点、异步加载节点和右键新增/重命名节点文案；运行时树节点改为从本地资源读取，并在语言切换后刷新 |
| Tooltip 资源值漏扫 | 修正 `Tooltip` ShowCase 中中文资源值仍为英文的基础提示句、多彩提示描述、显示/隐藏选项和预设分组标题；`ToolTip.Placement` 枚举值和十六进制颜色演示值作为非可见文案保留 |
| Tour 资源值和场景标题漏扫 | 修正 `Tour` ShowCase 中中文资源值仍为英文的步骤标题/描述、开始按钮、跳过按钮和高亮区域参数标签；补齐 `Custom Mask` 独立资源，避免自定义遮罩场景误显示为自定义指示器 |
| 保留边界 | `AA` 作为圆形按钮演示字符保留；`Separator`、`SplitButton`、Ant Design、AtomUI 和 `size` / `icon` / `loading` / `danger` / `orientationMargin` 等控件名或属性名作为技术词保留 |
| 验证结果 | `Button` / `SplitButton` / `Separator` / `CustomizeTheme` / `DropdownButton` / `Menu` / `CheckBox` / `DatePicker` / `TimePicker` / `Form` / `LineEdit` / `Mentions` / `NumberUpDown` / `RadioButton` / `Rate` / `Select` / `ToggleSwitch` / `TreeSelect` / `Transfer` / `Upload` / `Alert` / `Drawer` / `Message` / `Notification` / `Result` / `Skeleton` / `Spin` / `Watermark` / `Avatar` / `Badge` / `Card` / `Collapse` / `Descriptions` / `DataGrid` / `Expander` / `GroupBox` / `InfoFlyout` / `List` / `Segmented` / `Statistic` / `Tag` / `Timeline` / `TreeView` / `Tooltip` / `Tour` Catalog/XLIFF unit 对齐；对应 XAML 和非 Localization 代码中硬编码可见自然语言文案扫描为 0；`AtomUIGallery.Desktop` Debug build 通过 |

## P6 执行记录

| 项 | 结果 |
| --- | --- |
| 根因 | `LanguageResourceExtension` 只有在目标是 Avalonia 对象时才能返回动态资源；`SelectOption`、`CascaderOption`、`TreeItemNode`、`AutoCompleteOption` 等普通 POCO 选项对象会得到一次性静态值，语言切换后不会自动刷新 |
| 控件层修复 | `Cascader`、`CascaderView`、`Select`、`TreeSelect`、`TreeView` 在数据源刷新后按稳定 `ItemKey` / `Content` / `Value` 恢复已选项、默认路径、展开和勾选状态 |
| Gallery 结构修复 | `Cascader` ShowCase 拆分为 lazy tab 子场景；`CascaderView` 示例改为 ViewModel `OptionsSource`，避免在 XAML 内联本地化 `CascaderOption` |
| 规范补充 | `docs/gallery/organization.md` 已明确禁止把 ShowCase 语言资源直接写到普通选项数据对象上；选项 Header/Description 由 ViewModel 重建，稳定身份使用 `ItemKey`、`Content` 或 `Value` |
| 剩余示例层风险 | 已迁移 `SelectShowCase.axaml`、`FormPresetShowCase.axaml`、`FormControlsShowCase.axaml`、`FormBasicShowCase.axaml`、`FormStateShowCase.axaml`、`FormValidationShowCase.axaml`、`PriceInputTheme.axaml`、`SpaceCompactFormShowCase.axaml`；全局 `SelectOption` / `CascaderOption` / `TreeItemNode` / `AutoCompleteOption` 上 `LangResource` 扫描为 0；`DonationTheme.axaml`、`PhoneNumberTheme.axaml` 只包含符号/区号静态值，不属于本地化动态风险 |
| 验证结果 | `git diff --check` 通过；`AtomUIGallery.Desktop` Debug build 通过，0 warning / 0 error |

## 执行进度

| ShowCase | 分类 | 状态 | 已完成内容 | 验证 |
| --- | --- | --- | --- | --- |
| Button | General | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| AutoComplete | DataEntry | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| DataGrid | DataDisplay | 已完成 P1/P3/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 22 组 `ShowCaseItem.Title` / `Description`；父级场景 Tab 迁入本地资源；补齐列头、操作、筛选、分组表头、按钮、分页设置和标题页脚中文资源值 | Debug build 通过 |
| Avatar | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐用户文本和切换按钮资源值 | Debug build 通过 |
| Badge | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐状态、动态按钮、Ribbon 示例和颜色名称资源值 | Debug build 通过 |
| Calendar | DataDisplay | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 1 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Card | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐尺寸标题、卡片标题、Tab、分类和内容描述中文资源值 | Debug build 通过 |
| Carousel | DataDisplay | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Collapse | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐面板标题、尺寸分组、触发区域、正文示例和展开图标位置中文资源值 | Debug build 通过 |
| Descriptions | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description`；补齐字段标签、状态值、用户信息、配置和硬件信息中文资源值 | Debug build 通过 |
| Empty | DataDisplay | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Expander | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐面板标题、尺寸分组、嵌套标题、触发区域、正文示例、展开方向和展开图标位置中文资源值 | Debug build 通过 |
| GroupBox | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐标题信息和分组框内容中文资源值 | Debug build 通过 |
| ImagePreviewer | DataDisplay | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| InfoFlyout | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐浮层正文、触发按钮和箭头显示/隐藏按钮中文资源值 | Debug build 通过 |
| List | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 11 组 `ShowCaseItem.Title` / `Description`；补齐选择模式、搜索、按钮、新闻、颜色、分组、动态项和分页项中文资源值，并让运行时列表数据跟随语言切换刷新 | Debug build 通过 |
| QRCode | DataDisplay | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 8 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Segmented | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐周期、地图模式、列表/看板、长文本示例等中文资源值，并修正英文资源中的中文示例文本和中文字符资源 key | Debug build 通过 |
| Statistic | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐统计标题、卡片状态、计时器标题、按钮文案和 Day Level Timer 时间单位格式资源 | Debug build 通过 |
| Tag | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐预设/自定义分组、颜色名称、状态标签、无图标标题、阻止默认行为和边框示例标签中文资源值 | Debug build 通过 |
| Timeline | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐时间线事件、反转按钮、模式选项、标签示例和 Pending 文案中文资源值，并修正 React 术语描述 | Debug build 通过 |
| Tooltip | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐基础提示句、多彩提示描述、显示/隐藏选项和预设分组标题中文资源值 | Debug build 通过 |
| Tour | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description`；补齐步骤标题/描述、开始/跳过按钮和高亮区域参数标签中文资源值，并修正自定义遮罩场景标题资源 | Debug build 通过 |
| TreeView | DataDisplay | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐静态树节点、控制标签、上下文菜单、异步加载节点和右键新增/重命名运行时文案，并让模板树和异步树节点随语言切换刷新 | Debug build 通过 |
| Cascader | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 20 组 `ShowCaseItem.Title` / `Description`；补齐占位符、位置选项、模板分隔标题、静态/运行时级联节点和懒加载节点文案 | Debug build 通过 |
| CheckBox | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐复选框内容、受控状态按钮和 `ItemsSource` 运行时数据多语言 | Debug build 通过 |
| ColorPicker | DataEntry | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 11 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| DatePicker | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description`；补齐占位符、尺寸选项、变体占位符和弹出位置选项资源值 | Debug build 通过 |
| Form | DataEntry | 已完成 P1/P3/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 20 组 `ShowCaseItem.Title` / `Description`；父级场景 Tab、运行时消息和局部辅助控件模板迁入本地资源；补齐表单内主要可见文案和 `FormItem.Help` | Debug build 通过 |
| LineEdit | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 16 组 `ShowCaseItem.Title` / `Description`；补齐输入框占位符、变体标题、搜索框文案和文本域占位符资源值 | Debug build 通过 |
| Mentions | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description`；补齐变体占位符、触发提示、禁用和只读占位符资源值 | Debug build 通过 |
| NumberUpDown | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 13 组 `ShowCaseItem.Title` / `Description`；补齐输入占位符、键盘/滚轮提示、状态前缀占位符和原始值标签资源值 | Debug build 通过 |
| RadioButton | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐单选框、图表、选项、城市资源值和 `ItemsSource` 运行时数据多语言 | Debug build 通过 |
| Rate | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐允许清除标签和评分 tooltip 运行时文案多语言 | Debug build 通过 |
| Select | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 14 组 `ShowCaseItem.Title` / `Description`；补齐占位符、选项 Header、国家描述、变体文案、分组选项、附加内容和运行时选项数据多语言 | Debug build 通过 |
| Slider | DataEntry | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| TimePicker | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description`；补齐时间占位符和变体占位符资源值 | Debug build 通过 |
| ToggleSwitch | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐切换禁用、切换加载按钮和开关文本资源值 | Debug build 通过 |
| Transfer | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description`；补齐 Source/Target 标题、Reload 按钮、One way 开关、列表项内容/描述和表格示例数据多语言 | Debug build 通过 |
| TreeSelect | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 11 组 `ShowCaseItem.Title` / `Description`；补齐占位符、弹出位置、前缀、开关文案、静态/运行时树节点 Header 和异步加载节点 Header 多语言 | Debug build 通过 |
| Upload | DataEntry | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 10 组 `ShowCaseItem.Title` / `Description`；补齐上传按钮、校验失败原因、成功提示和默认任务错误信息多语言 | Debug build 通过 |
| Breadcrumb | Navigation | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 个 `ShowCaseItem.Title` 和 5 个 `Description` | Debug build 通过 |
| ButtonSpinner | Navigation | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| ComboBox | Navigation | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 8 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| DropdownButton | Navigation | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐按钮内容和菜单项中文资源值翻译 | Debug build 通过 |
| Menu | Navigation | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 15 组 `ShowCaseItem.Title` / `Description`；补齐菜单项资源值和 ItemsSource 运行时数据多语言 | Debug build 通过 |
| Pagination | Navigation | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Steps | Navigation | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 14 组 `ShowCaseItem.Title` / `Description`；补齐步骤项 Header/Description/SubHeader、内联说明和运行时按钮文案 | Debug build 通过 |
| TabControl | Navigation | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 25 组 `ShowCaseItem.Title` / `Description`；补齐 Tab 内容、位置/尺寸选项、额外操作、ItemsSource 和新增标签运行时文案 | Debug build 通过 |
| Alert | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description`；补齐提示正文、详细描述、操作按钮和循环横幅中文资源值，并修正带图标场景标题资源 | Debug build 通过 |
| Drawer | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description`；补齐抽屉标题、正文内容、弹出位置标签、二级抽屉按钮、当前区域提示和预设尺寸按钮中文资源值 | Debug build 通过 |
| Message | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐普通、信息、成功、警告、错误、加载中和加载完成运行时消息内容多语言 | Debug build 通过 |
| Modal | Feedback | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Notification | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐按钮、位置、悬停选项资源值和运行时通知标题/正文多语言 | Debug build 通过 |
| PopupConfirm | Feedback | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| ProgressBar | Feedback | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 19 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Result | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 8 组 `ShowCaseItem.Title` / `Description`；补齐结果标题、按钮、错误说明和 `SubHeader` 多语言 | Debug build 通过 |
| Skeleton | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐控制项标签、尺寸/形状选项、示例说明和按钮文案中文资源值 | Debug build 通过 |
| Spin | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐 Alert 描述、加载状态标签和 `Spin.Tip` 多语言 | Debug build 通过 |
| Watermark | Feedback | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐多行水印文本和自定义配置说明长文案多语言 | Debug build 通过 |
| CustomizeTheme | General | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 4 组 `ShowCaseItem.Title` / `Description`；补齐中文资源值中的按钮文案翻译 | Debug build 通过 |
| FloatButton | General | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 11 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Separator | General | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 6 组 `ShowCaseItem.Title` / `Description`；补齐中文资源值中的示例文案翻译 | Debug build 通过 |
| SplitButton | General | 已完成 P1/P5 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 5 组 `ShowCaseItem.Title` / `Description`；补齐控件内联按钮内容资源化 | Debug build 通过 |
| FlexPanel | Layout | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 11 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Grid | Layout | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 8 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |
| Space | Layout | 已完成 P1/P3 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 9 组 `ShowCaseItem.Title` / `Description`；父级场景 Tab 迁入本地资源 | Debug build 通过 |
| Splitter | Layout | 已完成 P1 | 新增本地 Catalog 和 `en-US` / `zh-CN` / `zh-TW` XLIFF，迁移 7 组 `ShowCaseItem.Title` / `Description` | Debug build 通过 |

## 资源命名规则

| 类型 | 命名示例 | 说明 |
| --- | --- | --- |
| 基础标题 | `BasicTitle` | 对应 `ShowCaseItem.Title` |
| 基础描述 | `BasicDescription` | 对应 `ShowCaseItem.Description` |
| 语义场景标题 | `CustomIconTitle` | 同一 ShowCase 内有多个相近场景时使用语义名 |
| 语义场景描述 | `CustomIconDescription` | 与标题保持前缀一致 |
| 控件内文案 | `SubmitButtonText`、`UserNameWatermark` | 按控件用途命名，避免只用 `Text1` 这类无意义 key |

## 验收标准

| 项 | 标准 |
| --- | --- |
| 本地化文件 | 每个包含可见演示文案的 ShowCase 都有本地 Catalog enum 和 `en-US.xlf`、`zh-CN.xlf`、`zh-TW.xlf` |
| 标题和描述 | 不再存在硬编码的 `ShowCaseItem.Title` / `Description`，除非该文本是刻意展示的代码或数据内容 |
| 主要演示文案 | 用户可见的主要标题、按钮、输入提示、分组标题支持语言切换 |
| 生成资源 | `LocalizationGenerator` 能为所有新增 Catalog 生成 descriptor、Translation Bundle 和资源扩展 |
| 编译 | `AtomUIGallery.Desktop` Debug build 通过 |
| 行为 | `Form`、`DataGrid`、`Space` 等 lazy tab 页面首次打开正常，不因多语言迁移引入延迟创建行为问题 |

## 边界和风险

| 项 | 处理方式 |
| --- | --- |
| 左侧菜单 | 不纳入本轮 ShowCase 本地多语言范围，菜单已有独立资源体系 |
| 代码示例和 mock 数据 | 初期不强制翻译，避免破坏示例语义或测试数据 |
| 公共场景名 | 已删除 `ShowCaseScenarioLang`；场景名按 ShowCase 就近维护 |
| 未提交改动 | 实施时避免把工作区已有的无关改动混入多语言提交 |
| 文案质量 | 英文和中文都需要按 UI 场景表达，不做机械直译 |
