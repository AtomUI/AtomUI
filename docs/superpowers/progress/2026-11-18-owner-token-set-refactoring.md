# 桌面控件 Token 读取重构执行记录

[实施计划](../plans/2026-11-18-owner-token-set-refactoring.md)

## G00 原生驱动基线

2026-11-18 已通过。已有 `fix/shadcn` 工作树的产物用于 A，包含启动时的未提交改动。macOS 26.6.2，原生 `atom:Window`，默认 Ant Design DaybreakBlue，客户区 520 × 360 DIP，缩放 2。执行入口为会话临时项目 `dotnet build Acceptance.csproj --no-restore` 和 `dotnet AtomUI.OwnerTokenAcceptance.dll <control|popup> <证据目录>`。采样使用 `screencapture -x -o -l <NSWindow number>`，比较使用 `frame_compare.py` 的精确模式。

运行时 SHA256

- Core `7c337de01fcdf26a413af5c05b217745ba6086dac68a27157234fbf5f62ee57c`
- Desktop.Controls `5fad4d8eefa835e36ce9ec3f7035160bdce7b17d28e37d4ce80faa7e47d4715e`

| 场景 | A/A 差异像素 | 1 DIP 扰动 | 复位差异像素 | 状态与视觉审查 |
| --- | ---: | ---: | ---: | --- |
| LineEdit | 0 | 6,647 | 0 | 字符串、轮廓和内边距清晰可见 |
| ComboBox | 0 | 29,232 | 0 | 原生截图可见展开项、选中背景、焦点环和锚定位置。脚本断言打开、初始选中和关闭 |

关键原生帧已查看，采样进程已退出。G00 的 8 张帧及取证清单已通过 `evidence.py audit` 并删除。参数化驱动与冻结的 A 产物在项目外复用于 G01。

## G01 Core 读取器

2026-11-18 已通过。固定资源键保留原对象和宿主查找，保留继承键和已有逐槽通知。整组模式先提交完整值，再回调一次。失败时保留上次完整值，并清空发布追踪，让下一次通知完整重读。

- 临时复现 3 项在原实现失败。两项值同时变化时旧实现通知两次，首次回调观察到新边框色和旧主色。改后只收到一次完整值。固定键的裸键、本地覆写及 Type/identity 键保持原语义，无关发布不会重新查找已读固定键。
- 独立代码审查发现切换 owner 后查找失败可能导致过期缓存。补充复现先失败，实测宽度档输入停在 31，预期为 36。重试改动后由下一次资源发布完整恢复，只有一次回调。
- 清理前 `OwnerTokenSet|ThemePublishDeltaTests|TokenResourceBinderPublishTests|ThemeTokenResourceProviderTests` 共 27/27。4 项临时验证已删除。清理后同范围 23/23，`InputFrameThemeVariableTests` 5/5，使用 `dotnet test <所属项目> --framework net10.0 --no-restore --filter <上述过滤器>`。 `git diff --check` 通过。

原生 `atom:Window` 采用与 G00 相同的客户区、缩放、默认 Ant Design DaybreakBlue 和截图工具。入口为 `dotnet AtomUI.OwnerTokenAcceptance.dll input <证据目录>`，加载 Core SHA256 为 `b6fa02e419acea8ea2cbc0faf465145d2da87f09c045bd34cd6dabb63b663ea3`。首轮的差异定位到原生窗口失活样式和闪烁光标。两侧同样固定激活、以公开选择属性重放选中文字状态后重新完整执行，逐像素比较整张截图。

| 场景 | A/A 差异像素 | A/B 差异像素 | 逻辑观测 |
| --- | ---: | ---: | --- |
| Outlined 默认 | 0 | 0 | 未聚焦，白底，灰边，256 × 32 DIP |
| Outlined 错误与聚焦前后 | 0 | 0 | 两帧均相同，错误红边和焦点环 |
| Filled 错误聚焦 | 0 | 0 | 透明底，错误红边 |
| Filled 局部错误色 | 0 | 0 | 原作用域紫边 `#ff722ed1` |
| Borderless 警告 | 0 | 0 | 透明底与边，256 × 30 DIP |
| Outlined 恢复 | 0 | 0 | 白底，默认焦点蓝边 |

关键帧已查看，状态日志逐条一致。取证审计未发现误入仓库的二进制，已删除本项所有媒体，进程已退出。

## G02 Gallery 断点试点

2026-11-18 已通过。六个内部中转属性和六条绑定收敛为一组固定键观察。仍逐槽维护有效阈值，保留 `Large → Medium` 的公开事件序列。局部资源的隐式整数转换沿用 Avalonia 原绑定转换规则。

- 修改前，阈值不注册中转属性的临时断言失败，六阈值、整数兼容、双阈值事件顺序和卸载释放的用例通过。改后两项均通过，连同 `GalleryShellViewCollapseTests` 共 23/23。临时用例删除后，`dotnet test tests/AtomUI.Toolkits.GalleryBase.Tests/AtomUI.Toolkits.GalleryBase.Tests.csproj --framework net10.0 --no-restore --filter 'FullyQualifiedName~GalleryShellViewCollapseTests'` 为 21/21。
- 原生 Ant Design DaybreakBlue，客户区 760 × 440 DIP，缩放 2。加载 GalleryBase A SHA256 `0e8dacb7fe4ddedc2d8212a362025e14adb4aa9c3a20408a255a402a29a87d19`，B SHA256 `624425352cd464e278f4838d77d095f97d9ab2db94d1c4a2a1bfc4a76ad59322`。脚本入口 `dotnet AtomUI.OwnerTokenAcceptance.dll breakpoint <证据目录>`，原生 `screencapture -x -o -l`，`frame_compare.py` 精确比较。
- 启用断点的 GalleryBrowserShellView 及实际路由内容宿主分别验证六个阈值的前／等于／后，36 条状态记录和多阈值公开通知序列逐条相同。A/A 0 差异像素，1 DIP 检出 6,637 像素，复位回到 0。首次重放有一对原生标题颜色过渡差异，稳定窗口激活并确保真实路由就绪后，同操作完整重放的 18 对整窗截图全部为 0 差异像素。
- 已查看关键帧，见证侧栏展开与折叠、路由内容位置及标题栏。取证审计无仓库二进制，已清理媒体和临时用例。 `git diff --check` 通过。

## A01a 线形进度条字号和线宽

2026-11-18 已通过。移除两档字号与三档线宽的五个中转属性。线形进度条使用一组资源观察，模板仍设置显示字号。主题链通过一处模式标记决定是否启用字号和线宽资源读取，完整替换主题继续使用原回退规则。

- 结构断言从原实现失败转为通过。定向覆盖横竖固定尺寸、局部裸 Own 键覆盖及还原、无关发布不使测量失效。清理 3 个临时用例后，最终桌面 79/79、共享控件 21/21、线形与分段渲染 50/50、Shadcn 47/47。命令采用所属项目的 `dotnet test --framework net10.0 --no-restore --filter`，对应过滤器为 `.ProgressBar.|StepsProgressTests`、`.ProgressBar.`、`StepsProgressFamilyEffectTests.ProgressBar_Token_Has_Effect|StepsProgressFamilyEffectTests.StepsProgressBar_Token_Has_Effect`、`ShadcnControlTests`。共享构建产物发生并行锁冲突的桌面调用已改为串行复跑并通过。
- 独立审查补充了两项兼容复现。早期实现读取装箱字号整数会抛异常，未启用内置样式的替换主题也会提前收到作用域线宽。冻结 A 验证整数覆盖仍生效，替换主题按原逻辑回退 6 DIP。兼容修正后，B 的整数覆盖、轨道、标签、图标、状态与几何日志逐条与 A 相同。
- 原生 Ant Design DaybreakBlue，客户区 620 × 500 DIP，缩放 2。入口 `dotnet AtomUI.OwnerTokenAcceptance.dll progress <证据目录>`，使用原生窗口编号截图及 `frame_compare.py` 精确比较。最终 Desktop.Controls SHA256 `b7af62f7f785b5cc6c49654a1f1a4e6a561dff1301fa2070d6344be97433b86b`。
- A/A 0 差异像素，1 DIP 检出 12,300 像素，复位 0。最终 11 对原生整窗截图均为 0 差异像素。已查看固定尺寸、异常和兼容场景。校验覆盖作用域重挂、0% 内置标签、完成和异常状态、盒装字号及替换主题。媒体留存，取证审计未发现仓库副本。A01b 独立验收。

## A01b 线形进度条间距、对比色和分段尺寸

已通过。桌面间距、对比色、步距和段宽取自各自挂树期读取组。移除四个对比色和分段尺寸中转属性。共享间距属性继续提供现有兼容回退，桌面默认主题改走资源读取组。

- 临时结构断言在修改前失败，改后通过。定向用例验证主题范围内的颜色变化保持测量有效，步距和最小段宽的局部覆盖只使分段布局失效。临时用例已删除。
- 清理后沿用 A01a 的四个精确过滤器，桌面 79/79、Token 渲染 50/50、共享控件 21/21、Shadcn 47/47。
- 原生 Ant Design DaybreakBlue，窗口 620 × 500 DIP，缩放 2。与 A01a 同一驱动入口，增加内外间距、对比色和步距的显式局部主题配置。全部 11 对 A/B 整窗帧为 0 差异像素，逻辑日志逐条一致。A/A 和复位均为 0，1 DIP 扰动检出 12,300 像素。已查看固定尺寸及完成态关键帧，取证审计无仓库副本。

## 圆形进度条在矩形槽内变形

在 A02 修改前的原生窗口中复现。`CircleProgress` 切到 `98 × 86 DIP`、12 段时，轨道路径用 `92 × 80 DIP` 分别计算两轴；`DashboardProgress` 的 `74 × 66 DIP` 控件也用了 `69 × 61 DIP` 的路径槽。修复让模板路径和自绘回退路径使用居中的正方形，各自保留原描边、标签布局和外部尺寸。圆形轨道变为 `80 × 80 DIP`，仪表盘以 `61 × 61 DIP` 路径槽生成原缺口形状。

- 两项临时复现在原实现均失败。改后含复现的桌面定向测试 81/81；移除用例后桌面 79/79，共享 21/21，圆形与仪表盘 Token 渲染 32/32。沿用 A01 的桌面与共享过滤器，渲染过滤器为 `StepsProgressFamilyEffectTests.CircleProgress_Token_Has_Effect|StepsProgressFamilyEffectTests.DashboardProgress_Token_Has_Effect`。`git diff --check` 通过。
- 同一个 Ant Design DaybreakBlue 原生窗口，客户区 `560 × 400 DIP`，缩放 2，相同状态顺序。已查看分段状态前后两张整窗图。分段状态的整窗差异为 9,747 像素，分布在两个圆弧的区域；初始状态的 3,534 差异像素在仪表盘区域，原本为正圆的上方 `CircleProgress` 未变。修正后重复采样和扰动复位均为 0 差异像素，1 DIP 扰动检出 7,588 像素。修正后的 9 帧作为 A02 重构基线。

## A02 圆形与仪表盘的尺寸下限

已通过。桌面每个终端用一组读取各自的两个裸 Own Token。整组变化时一起计算字号和图标输出，沿用主题投射。原共享下限属性仍由 Mobile 模板使用，并为替换主题保留兼容入口。

- 临时结构断言修改前失败，终端独立覆盖与替换主题断言通过。改后 81/81。清理后桌面 79/79，共享 21/21，圆形与仪表盘 Token 渲染 32/32。使用圆形路径修复的同组精确过滤器，`git diff --check` 通过。
- 同一原生窗口与输入序列，以修好的圆形路径为基线。Ant Design DaybreakBlue 全部 9 对整窗截图为 0 差异像素。两终端的控件尺寸、文字字号、图标尺寸及轨道几何日志逐条一致。已查看分段圆形和仪表盘关键帧，取证审计无仓库副本。

## A03 Empty 预设插画配色

已通过。桌面默认主题的五个 Effective Global 配色输入在使用预设 SVG 时按 `Empty` 身份成组读取。保留自定义图片和替换主题的回退入口。离开逻辑树时释放观察。

- 单次局部资源变更的临时测试，原实现 Default SVG 提交 3 次，改后 Default 和 Simple 各提交 1 次，自定义图像未触发预设 SVG。含临时测试 27/27，移除后桌面 26/26、共享 5/5、Token 渲染 16/16、Shadcn 插画梯度 4/4。精确过滤器为 `EmptyObjectContentTests|EmptySemanticPartTests|EmptyIndicatorContractTests`、`EmptyExtensionTests`、`DisplayFamilyEffectTests.Empty_Token_Has_Effect`、`ShadcnControlTests.Empty_Illustration_Keeps_A_Fill_Ramp`。桌面复用的挂树测试从作用域读出期望颜色，继续独立核对生成的 SVG。`git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `560 × 340 DIP`，缩放 2。全部 8 对整窗截图为 0 差异像素，浅深主题、局部配色和恢复的 SVG 指纹、描述、占位及自定义图像状态逐条一致。A/A 0，1 DIP 扰动检出 10,323 像素，复位 0。已查看浅色和深色局部配色关键帧，取证审计无仓库副本。

## A04 圆点与数量徽标阴影

已通过。两种桌面徽标的内部绘制部件分别按各自原 Own 键成组读取，保留 Count 阴影输出和内部样式覆盖。观察随指示器视觉树挂载和移除。

- 同次局部更新的临时测试，Count 原阴影输出变化 2 次，改后 1 次。覆盖无描边和恢复。临时测试通过后已移除。清理后桌面 55/55、共享 12/12、Token 渲染 35/35。精确过滤器为 `.Badge.|FloatButtonBadgeColorTests`、`BadgeExtensionTests`、`TagBadgeFamilyEffectTests.DotBadge_Token_Has_Effect|TagBadgeFamilyEffectTests.CountBadge_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `540 × 400 DIP`，缩放 2。全部 7 对整窗截图为 0 差异像素，覆盖独立与装饰型徽标、局部阴影、无描边和恢复。原生探针的数量、描边及部件几何逐条相同。A/A 0，1 DIP 扰动检出 12,806 像素，复位 0。已查看着色描边与无描边关键帧，取证审计无仓库副本。

## A05 丝带折角

已通过。桌面 Adorner 原位分组读取偏移、变换和暗化 Own Token。公开 `Offset`、配色、标签和左右放置按原几何计算。暗化和圆角变化直接触发自绘重绘，避免依赖其他资源附带使画面失效。

- 临时测试记录了原实现发布 3 个计算中转属性，改后为 0；几何、公开偏移和折角变换仍生效。含用例的桌面测试 38/38，清理后桌面 37/37、共享 12/12。原产物的 Ribbon Token 效果通过 10/10，早期重构使 `BorderRadiusSM` 一项失效。声明自绘依赖圆角后，同过滤器 `TagBadgeFamilyEffectTests.RibbonBadge_Token_Has_Effect` 为 10/10，`git diff --check` 通过。
- 用户指出的示例错位在修改前已复现。同一组手动水平偏移在 `caller-override` 使两侧分别外伸 1 和 3 DIP，宽 11 DIP 的折角压入卡片；切换方向后旧偏移仍生效，外伸变成 21 和 19 DIP，折角离开卡片。原图留存供核对。
- 修正原生驱动的示例后，外部 Offset 场景演示垂直调整，左右互换前恢复默认值，行标签和实际方向同步。布局探针断言外伸长度等于折角宽度，正文与丝带分离。参考本地 Ant Design `/Users/chinboy/Projects/ReferenceProjects/ant-design/components/badge/style/ribbon.ts`，提交 `c811baa979`。原生 Ant Design DaybreakBlue，客户区 `540 × 350 DIP`，缩放 2。冻结的原控件和当前控件使用同一修正后的驱动，8 对整窗全部 0 差异像素，状态和几何探针逐条相同。A/A 与复位均为 0，1 DIP 扰动检出 10,680 像素。已人工查看局部 11 DIP 和互换后默认 8 DIP 的两张关键帧。

## A06 分隔线布局度量

已通过。桌面按横线标题和竖线的实际用途成组读 Own Token，保留公开边距与共享样式回退。标题间距和边缘比例变化只重排，竖线间距变化重新测量。

- 初始局部资源测试的几何断言通过，中转属性断言在旧实现失败。改后 18/18。清理后桌面 24/24，Token 渲染 16/16。精确过滤器为 `.Separator.|SplitButtonSeparatorStabilityTests` 与 `LayoutFamilyEffectTests.Separator_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `560 × 420 DIP`，缩放 2。无标题、左中右标题、竖线、作用域度量和显式布局覆盖的全部 7 对整窗为 0 差异像素。标题与线段的位置日志逐条一致。A/A 和复位均为 0，1 DIP 扰动检出 11,581 像素。已查看关键帧，取证审计无仓库副本。

## A07 轮播箭头与分页边距

已通过。桌面轮播成组读取两个 Own Token，按变化分别更新有效边距。原投射属性、分页位置、翻页状态和显隐仍按同一路径工作。

- 临时测试的中转属性变化由 2 次降至 0，三个有效边距的断言在前后均符合原值。清理后桌面 1/1、Token 渲染 15/15。过滤器为 `.Carousel.` 与 `DataFamilyEffectTests.Carousel_Token_Has_Effect|DataFamilyEffectTests.CarouselNavButton_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户端 `530 × 400 DIP`，缩放 2。全部 9 对整窗为 0 差异像素，覆盖分页四侧、前进和后退、边缘箭头显隐、同时调整两项偏移及改宽高。控件、页码和部件状态日志逐条一致。A/A 和复位为 0，1 DIP 扰动检出 2,903 像素。已查看翻页和换边关键帧，取证审计无仓库副本。

## A08 Result 行高与状态图标

已通过。两项相对行高、状态图标尺寸和实际状态颜色由一组资源读取。继续以原优先级应用状态图标，绝对行高仍由字号与比例相乘。

- 临时测试记录原实现发布 5 次计算中转属性，改后为 0。行高、尺寸和状态色断言均通过。含临时用例桌面 22/22，清理后桌面 21/21、共享 4/4、Token 渲染 31/31。精确过滤器为 `ResultIconContentTests|ResultSemanticPartTests|ResultStatusIconColorTests`、`ResultExtensionTests`、`DisplayFamilyEffectTests.Result_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `560 × 360 DIP`，缩放 2。覆盖状态往返、自定义图标、字号及局部覆盖的全部 10 对整窗为 0 差异像素，图标颜色、尺寸和文本位置日志逐条相同。A/A 和复位为 0，1 DIP 扰动检出 8,665 像素。已看警告及自定义图标，媒体审计无仓库副本。

## A09 Segmented 滑块背景与阴影

已通过。桌面按原 Own 与 Effective Global 键成组读取，保留共享主题回退。纯配色变化不使测量失效，滑块的位置、尺寸和过渡仍走现有通道。

- 临时测试的旧实现中转属性断言失败。改后含用例桌面 36/36，清理后 35/35，Token 渲染 41/41。精确过滤器为 `.Segmented.` 与 `DataEntryFamilyEffectTests.Segmented_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `540 × 360 DIP`，缩放 2。10 对整窗全部为 0 差异像素，覆盖横竖、连续选择、深浅切换、局部阴影与项目隐藏。动效状态按 `True → False` 往返，控件和项目坐标日志逐条一致。A/A 和复位为 0，1 DIP 扰动检出 11,058 像素。前两轮因窗口在截图过程中失活而退出，脚本丢弃失活帧并在同一场景重采后通过。已查看隐藏项与深色关键帧。

## A10 Skeleton 微光配色

已通过。活动桌面宿主成组读取两色并提交场景，停止时释放观察。公开时长、颜色叠加、块几何和亮带运动仍按原路径工作。

- 临时测试锁定活动期间成组覆写、停止后换色重启以及几何不重复构建。旧实现的中转属性断言失败，改后含用例 30/30。清理后桌面 29/29、共享 23/23、Token 渲染 38/38。过滤器为 `.Skeleton.` 与 `DataFamilyEffectTests.Skeleton`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `540 × 330 DIP`，缩放 2。使用公开缓动属性在真实 compositor 中采样两个固定相位。两相位验出 71,538 像素差异，1 DIP 扰动检出 3,857 像素，A/A 和复位为 0。全部 10 对原生整窗为 0 差异像素，覆盖微光、局部换色、深色以及加载完成。活动探针在实际截图后采样，避免把窗口前台切换阶段的短暂停止误记为截图状态。已查看活动与局部配色关键帧。

## A11 BorderBeam 绘制参数与大圆角示例

已通过。宿主按两个 Effective Global 色键和两个裸 Own 键成组读取，通过一个模板结果传给 Presenter。透明度单独变化时沿用现有渐变缓存。

- 临时测试记录旧宿主发布 4 个中转属性，改后更新 0 个中转属性并一次发布绘制参数。外部保留色标集合，控件和 Presenter 在卸载后均可回收。含临时用例 25/25，清理后桌面 23/23、角点与运动渲染 3/3。精确过滤器为 `.BorderBeam.` 与 `BorderBeamCornerPathRenderingTests|BorderBeamMotionRenderingTests`。 `git diff --check` 通过。
- 用户指出的下方 26 DIP 圆角示例已精确复现。原场景给 2 DIP 边框设置了 `Outset=3 DIP`，在 2 倍缩放下原底部边框为第 682 到 685 像素行，光带为第 688 到 691 行，中间 2 行空白。保留进度、圆角和变更后的色标，只恢复该示例的默认 `Outset` 时，光带回到第 682 到 685 行。整窗 1,596 个差异像素全部落在下方光带区域。显式外移另列为标注的验收帧。
- 原生 Ant Design DaybreakBlue，客户区 `560 × 410 DIP`，缩放 2。修正示例后的 11 对整窗全部 0 差异像素，覆盖双圆角、光束数量、局部覆盖、色标原地改色与比例、显式外移及摘挂。A/A 和复位为 0，1 DIP 扰动检出 11,377 像素。已查看贴边改色和显式外移，审计无仓库副本。

## A12 通知倒计时进度条

已通过。运行时进度部件成组读取高度、底槽和填色的三个 Own 键。高度变化重新测量，纯画刷变化只重绘。

- 临时测试记录原实现更新 3 个中转属性，改后为 0。验证 10 秒总时长、6 秒剩余、局部高度和画刷，纯配色时测量仍有效。含临时用例 30/30，清理后桌面 29/29、Token 渲染 46/46。过滤器为 `.Notifications.` 与 `FeedbackFamilyEffectTests.NotificationCard_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `600 × 320 DIP`，缩放 2。真实窗口通知宿主重放满量、部分填充、归零、主动关闭与真实调度到期。最初的差异只出现在卡片阴影后面的原生标题文字区域，样本为标题留出空间后从冻结产物重新作 A/B。全部 9 对整窗 0 差异像素。A/A 和复位为 0，1 DIP 扰动检出 24,385 像素。已查看部分填充及归零，审计无仓库副本。

## A13 BackTop 回顶与进度环

已通过。先修正回顶动画的结束位置，再合并进度环在实际绘制部件上读取的三个原 Effective Global 键。

- 回顶原生复现参数为 648 DIP 内容、230 DIP 视口、418 DIP 起点与 100 ms 动画。旧产物结束偏移 2.225432 DIP、进度 0.005324，临时用例也在实际按钮调用入口失败。动画结束后以 `SetCurrentValue` 提交精确的纵向零点，换目标与卸载取消旧动画。改后原生偏移和进度均为 0。修复前后其他 9 帧全部 0 差异像素，回顶帧的 16,956 个差异像素位于滚动区。临时复现通过，删除后 `BackTopFloatButtonProgressTests` 19/19。
- 进度环临时测试记录旧路径写入 3 个中转属性，改后为 0。局部线宽和两色变化前后，同一个真实滚动视口仍得到对应的位置与进度。含临时用例桌面 57/57，清理后桌面 56/56、Token 渲染 16/16。过滤器为 `.FloatButton.` 与 `ButtonFamilyEffectTests.BackTopFloatButton_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `540 × 370 DIP`，缩放 2。以精确回顶的产物为基线，10 对整窗均为 0 差异像素，含零、阈值前后、中段、底部、局部线宽配色与回顶。状态日志逐条一致。1 DIP 扰动检出 10,579 像素，重复与复位为 0。已查看中段和回顶最终帧。

## A14 TextArea 缩放手柄

已通过。斜线绘制部件用组读取颜色 Own 键及随 owner 解析的 `LineWidth`。手柄尺寸、命中区域和拖动逻辑沿用现有入口。

- 临时测试原实现更新 2 个中转属性，改后为 0，手柄位置和尺寸一致。含临时用例桌面 23/23，清理后桌面 22/22、Token 渲染 57/57。过滤器分别为 `TextArea` 与 `InputFamilyEffectTests.TextArea_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `510 × 320 DIP`，缩放 2。脚本通过 Avalonia 指针事件完成捕获、拖动和释放。斜向拖动仅改变高度，宽度为 330 DIP，原生最小高度为 70 DIP。含局部画刷与线宽、显隐和恢复的 9 对整窗全部 0 差异像素，日志逐条一致。1 DIP 扰动检出 6,779 像素，重复与复位为 0。已查看拖动后和最小高度帧，审计无仓库副本。

## A15 焦点描边

已通过。默认焦点装饰在实际 Adorner 上成组读两个 Effective Global 键及偏移 Own 键。失焦脱树释放观察，轮廓形状沿用原有目标与裁剪。

- 临时测试原实现发出 3 个绘制输入属性变化，改后为 0。验证作用域配色、线宽与偏移都到达真实 Adorner，焦点移走后清除装饰对象与目标关系。含临时用例桌面 54/54，清理后桌面 53/53、Token 渲染 2/2。过滤器为 `AdornerLayerTests` 与 `DisplayFamilyEffectTests.AdornerLayer_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `540 × 430 DIP`，缩放 2。首轮内嵌帧在捕获前失去键盘焦点。相同控件和样本在原生窗口稳定激活后重放焦点动作，重新从冻结产物做 A/B，10 对整窗全部 0 差异像素。覆盖外置、内嵌、大圆角、嵌套裁剪、局部配色和失焦。1 DIP 扰动检出 28,807 像素，重复与复位为 0。已查看内嵌和裁剪帧，审计无仓库副本。

## B01 Slider 轨道与静态刻度

已通过。轨道成组读取五项输入。尺寸变化刷新轨道布局，刻度背景、线宽和尺寸一次同步给实际绘制部件。动画状态画刷继续沿用原来的主题目标和优先级。

- 临时测试记录局部发布从 5 个中转属性变为 0，四个刻度宽度按作用域更新，当前值保持一致。含临时用例桌面 39/39，清理后桌面 38/38、Token 渲染 40/40。过滤器为 `.Slider.` 与 `ChoiceFamilyEffectTests.Slider_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `560 × 360 DIP`，缩放 2。横向单值、竖向范围、局部刻度、禁用、恢复共 8 对原生整窗均为 0 差异像素。框架指针点击 75 刻度，两侧命中坐标均为 `233,22`，值均为 75。1 DIP 扰动检出 9,679 像素，重复与复位为 0。已查看局部刻度和禁用帧，审计无仓库副本。

## B02 Steps 进度环与连接线

已通过。可见进度环读取一组画刷。布局面板成组读取连接线及相邻的基础布局参数，保留 Dot、Navigation、Panel 的样式覆盖与末项规则。

- 临时测试记录局部资源更新时旧实现数出 4 次中转属性变化，改后为 0。确认默认连接线为 3 DIP，并在 OutlineDot 分支由状态样式改为 4 DIP。原有重绘用例改用实际资源写入，消费检查同时覆盖主题和 C# 读键。含临时用例桌面 204/204，清理后桌面 203/203、Token 渲染 128/128。过滤器为 `.Steps.` 与 `StepsProgressFamilyEffectTests.Steps_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，缩放 2。窄版与宽版分别 8 对整窗，均为 0 差异像素。宽版 `800 × 505 DIP` 为副标题留足空间，并断言前两条横向连接线均有绘制宽度。覆盖横竖、进度环、作用域、Navigation、OutlineDot、Panel 与末项，连接线几何、显隐与命中属性日志逐条一致。两版 A/A 与复位均为 0，1 DIP 扰动分别检出 23,334 和 23,333 像素。已查看进度、末项和宽版关键帧。

## B03 RadioButton 圆点几何

已通过。指示器按三个 Own 键成组计算圆点尺寸，主题负责最终宽高和状态颜色，圆点过渡继续使用现有动画属性。

- 临时测试记录原实现 3 个中转属性更新，改后为 0。覆盖局部外框 22 DIP、启用圆点 12 DIP、禁用圆点 14 DIP。含临时用例桌面 44/44，清理后桌面 43/43、Token 渲染 22/22。过滤器为 `.RadioButton.` 与 `ChoiceFamilyEffectTests.RadioButton_Token_Has_Effect|ChoiceFamilyEffectTests.RadioButtonGroup_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `470 × 300 DIP`，缩放 2。8 对整窗均为 0 差异像素。框架键盘空格切换组内选择，覆盖焦点、局部度量、禁用与动画结束后的形状。圆点与指示器日志逐条一致，1 DIP 扰动检出 7,526 像素，重复与复位为 0。已查看键盘选择和禁用关键帧。

## B04 ToggleSwitch 内容间距与加载绘制

已通过。桌面开关按尺寸档读取内容间距。加载把手只在活动加载期间成组观察线宽和亮弧透明度。状态画刷、把手过渡和旋转周期沿用原入口。

- 临时测试记录局部更新由 4 个中转属性变化降至 0，并核对普通、小号及实际加载部件的有效度量。含临时用例桌面 15/15，清理后桌面 14/14、Token 渲染 45/45。过滤器为 `.Switch.` 与 `ChoiceFamilyEffectTests.ToggleSwitch_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `475 × 315 DIP`，缩放 2。8 对整窗均为 0 差异像素，覆盖点击命中、标准开关激活、普通与小号、禁用、加载开始和结束、局部配色与尺寸。先观察真实加载旋转，再固定角度取样，部件日志逐条一致。1 DIP 扰动检出 9,329 像素，重复和复位为 0。已查看局部加载帧，审计无仓库副本。

## B05 Collapse 默认内边距

已通过。折叠项按尺寸档成组读标题与内容的默认值，Custom 使用 Middle 键。调用方与 owner 的优先级沿用原输出通道。

- 临时测试记录旧实现更新 2 个中转属性，改后为 0。核对 Small、Middle、Custom 以及局部项覆盖高于 owner。含临时用例桌面 51/51，清理后桌面 50/50、Token 渲染 39/39。过滤器为 `CollapseBehaviorTests|CollapseSemanticPartTests|CustomizableSizeTypeContractTests.Collapse` 与 `.Collapse_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `515 × 395 DIP`，缩放 2。9 对整窗均为 0 差异像素，覆盖展开折叠、大小号、局部与实例覆盖、Custom、替换模板和恢复。日志逐条一致。1 DIP 扰动检出 8,620 像素，重复和复位为 0。已查看自定义内边距与模板关键帧。

## B06 TabControl 分隔线与卡片圆角

已通过。基础分隔线成组读取宿主画刷与厚度，卡片终端按当前尺寸和自身 Token 身份读取圆角。分隔线和方向相关的有效圆角继续通知模板和生成容器。

- 临时测试核对作用域内画刷、分隔线厚度和两个卡片终端圆角，记录旧实现 5 个中转属性通知，改后为 0。含临时用例桌面 284/284，清理后桌面 283/283、Token 渲染 144/144。过滤器为 `.TabControl.` 与 `TabControl_Token_Has_Effect|CardTabStrip_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `580 × 565 DIP`，缩放 2。11 对整窗均为 0 差异像素，覆盖三个终端的真实溢出菜单、选中、关闭、左侧放置与恢复。日志逐条一致。1 DIP 扰动检出 25,165 像素，重复与复位为 0。已查看选中与实际菜单关键帧。

## B07 NavMenu 弹层尺寸

已通过尺寸和交互验收。弹层框成组读取宽高上限，顶层项从自身作用域读最小宽度输入。有效尺寸、公开语义样式和已有测量路径继续生效。

- 临时测试先确认宽高上限改变时原实现的视口大小，再记录旧实现的中转属性。改后不再注册这三项原始值，清理前桌面 319/319，清理后 318/318，使用 `.NavMenu.` 过滤器。 `NavMenu_Token_Has_Effect` 改前和改后均 97/99，同两项 `MenuPopupShadows`、`BoxShadowsSecondary` 的 headless 阴影断言均为 0 差异像素。使用冻结的旧桌面程序集确认。弹层尺寸相关用例均通过。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `650 × 560 DIP`，缩放 2。12 对整窗均为 0 差异像素，含横向、内联展开、折叠、限高、滚动、语义宽度和关闭。折叠弹层 `174 × 144 DIP` 的视口高度为 140 DIP，实际滚轮后偏移 100 DIP，命中项从第 1 项到第 3 项。语义最小宽度覆盖为 228 DIP，最大宽度为 250 DIP。1 DIP 扰动检出 14,547 像素，重复与复位为 0。已查看滚动与语义宽度的关键帧。

## B08 TreeView 与 TransferTreeView 拖放

已通过。两种树在拖动期间成组观察当前宿主的有效主色和终端自己的线宽，结束或离树时释放。保留行头的既有输入和拖放索引。TransferTreeView 原实现以普通 TreeView 的 `NodeHoverBg` 键绑定预览，启动拖动时报错。按实际树选择预览键后保留拖拽源的资源作用域。

- 临时用例修改前复现作用域线宽有效但仍注册两个中转属性，以及 Transfer 拖动开始的 `InvalidOperationException`。改后含临时用例桌面 141/141，清理后 139/139、渲染 45/45。过滤器为 `.TreeView.|.Transfer.` 与 `TreeView_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `535 × 510 DIP`，缩放 2。普通树 9 对整窗均为 0 差异像素，行前、行中、行后目标和插入线坐标逐条一致。Transfer 原始重放的四个静态场景均为 0 差异像素。原实现的活动拖拽停在异常，修复后单独验证活动插入线、释放和重挂三帧。两组 1 DIP 扰动均检出 17,234 像素，重复与复位为 0。已查看拖入行中及 Transfer 插入线关键帧。

## B09 Calendar 日期范围条

已通过。公历及农历面板按原作用域成组读取缩进、字号和画刷，程序设置的标签值优先。验收发现三个既有问题并修复。复用单元后换月会发布新的日期几何。日期单元的自然高度参与测量，限高时内容按实际网格行高排列。范围条初次创建即使用宿主字号和画刷。

- 临时测试改前失败，分别发现 1 个旧月份范围条留在第二月、首行日期上移 15 DIP 进入星期标题、标签从首次 14 DIP 变成重排后 12 DIP，以及原输入属性仍注册。改后桌面 175/175，清理后 171/171，过滤器 `.Calendar.|.DateViewer.`。Token 渲染 97/97，过滤器 `Calendar_Token_Has_Effect|Range_Bar_Label_Follows_ColorTextLightSolid`。自然高度 Token 用提供足够空间的窗口验证，原限高窗口继续复验排版。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `660 × 610 DIP`，缩放 2。使用真实月份选择控件，首行日期与星期标题间距从重叠 15 DIP 修正为 6 DIP。一月 4 个范围段、二月 0 个、切回一月 4 个。原标签漂移检出 4,275 像素。修复视觉缺陷后的两种读取实现共 10 对全窗为 0 差异像素，覆盖跨周、局部标签、换月、农历主题与恢复。1 DIP 扰动检出 21,040 像素，重复和复位为 0。

## B10 Timeline 节点默认值

已通过。桌面主题传入两个准确的 Timeline 资源键，节点成组读取行高比例和默认色。当前字号、显式节点色和既有几何及有效颜色输出继续生效，共用基类保留原直接设置入口。

- 临时测试确认作用域行高、画刷、直接颜色值及条目覆盖。旧实现写入两种中转属性共 3 次，改后为 0。含临时用例桌面 53/53，清理后 52/52、Token 渲染 19/19，过滤器 `.Timeline.` 与 `Timeline_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `580 × 390 DIP`，缩放 2。9 对整窗均为 0 差异像素，覆盖短长内容、默认和自定义节点、末项、字号变化与切换方向。1 DIP 扰动检出 19,673 像素，重复和复位为 0。已查看字号和横向轨迹关键帧。

## B11 ColorPicker 空色、棋盘格与起始间距

已通过。色块按活动状态成组读取空色斜线或棋盘格画刷，滑块和色块直接消费继承的尺寸并复用现有图案缓存。用户指出的固定宽度间距来自普通触发器的整体居中，带文字时改为按主题内边距从逻辑起始边排列。

- 临时用例先确认子部件使用 9/11 DIP 局部棋盘格，再验证移除色块的四项中转及滑块的尺寸中转。两个固定宽度用例改前失败，改后与其他用例共 23/23，清理后桌面 20/20、Gallery 自定义尺寸 3/3。ColorPicker 渲染 182 项通过，另有 1 项来源清单按 C# 的真实观察组补齐后定向通过。过滤器为 `.ColorPicker.`、`ColorPickerFamilyEffectTests`、`ColorPickerCustomSizeTests`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `712 × 540 DIP`，缩放 2。读取重构的 11 对整窗为 0 差异像素，覆盖空色、透明色、渐变、真实弹层和颜色编辑回写、明暗往返。定向间距复现按原始 132/166 DIP 宽度执行，两者起始间距从 29/27.5 DIP 变为 4 DIP。加宽及 RTL 仍为 4 DIP，局部内边距将间距切换到 10 DIP。纯色块及自然宽度的两对整窗为 0 差异像素。其余四对的变化均落在两个普通触发器内。已查看渐变弹层和修复后原始尺寸帧。

## B12 AddOnDecoratedBox 状态色

已通过。主题选择警告、错误或禁用调色状态，外框通过一个观察组读取当前状态的前景和图标色。禁用态共用同一条读取。运行时样式继续使用 Template 优先级，离树时释放观察与临时值。

- 临时用例确认原来的作用域错误与禁用色正确，但仍有 2 个中转属性。改后含临时用例桌面 335/335，清理后 334/334。过滤器覆盖 Input、ButtonSpinner、NumericUpDown、ComboBox、Select、TreeSelect、Cascader、DatePicker、TimePicker、AutoComplete、Mentions 及 `AddOnDecoratedBoxVariantTests`。状态色和来源台账相关渲染 95 项通过，另 2 项在补齐 A14 ResizeHandle 和 B09 Calendar 的 C# 资源来源后定向通过。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `789 × 620 DIP`，缩放 2。12 对整窗为 0 差异像素。17 个实际入口包括 LineEdit、TextArea、SearchEdit、ButtonSpinner、NumericUpDown、ComboBox、Select、TreeSelect、Cascader、DatePicker、RangeDatePicker、TimePicker、RangeTimePicker、AutoComplete、AutoCompleteSearchEdit、AutoCompleteTextArea、Mentions。每个入口核对正常、警告、错误、禁用、恢复、替换和卸载，LineEdit 通过真实语义样式验证优先级。1 DIP 扰动检出 72,383 像素，重复和复位为 0。

## B13 WaveSpirit 波纹配置

已通过。桌面波纹通过一组资源读取扩散范围、起始透明度和默认时长，尺寸与淡出共用一次时长读取。实际尺寸和淡出时长可分别设置，颜色继续沿用真实宿主与画刷入口。卸载释放资源观察并取消播放。

- 临时用例确认旧实现产生四项原始属性通知，改后为 0，并验证显式零时长取消播放、清除覆盖后恢复及离树释放。含临时用例桌面 26/26，清理后 25/25，过滤器为 `PrimitivesScopeContractTests|StepsWaveTests`，资源读取清单 1/1。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `486 × 318 DIP`，缩放 2。10 对整窗为 0 差异像素。实际点击 Button 和 CheckBox 触发动画，通过框架时钟驱动到同一时间点取帧，核对扩散、透明度、颜色和边界。紫色画刷路径读取 7 DIP、0.65、620/670 ms，完成与卸载后动画停止。1 DIP 扰动检出 6,513 像素，重复与复位为 0。

## B14 OverlayDialogPresenter 开场与离场时长

已通过。两个时长共用当前 presenter 作用域的资源观察，独立设置仍可覆盖。新动画按启动时取值，保留原点、焦点、在途轨迹与关闭清理。

- 临时用例改前有 2 个时长中转通知，改后为 0，验证 presenter 本地资源刷新以及显式零时长和清除后的回退。作用域场景在打开前配置，复用原有外层资源传播行为。含临时用例桌面 268/268，清理后 267/267，过滤器 `.Dialog.`，反馈部件读取清单 1/1。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `620 × 450 DIP`，缩放 2。通过项目的 Motion 测试时钟推进实际 overlay 动画，12 对整窗均为 0 差异像素。默认 300/200 ms，局部 500/280 ms，实际遮罩透明度、缩放、焦点及 `(122, 131)` 的固定原点逐条一致。1 DIP 扰动检出 6,092 像素，重复与复位为 0。已查看真实遮罩和开场关键帧。

## B15 Select、TreeSelect、Cascader 弹层几何

已通过。三个终端成组读取候选高度与弹层边界。Select 和 TreeSelect 的弹层高度随内边距更新，Cascader 默认使用自身的 180 DIP 高度 Token。显式设置的页大小保留计算入口，匹配宽度和语义模板输出保持原契约。

- 临时用例改前复现 Select、TreeSelect 只改内边距时上限停在 132 DIP，Cascader 默认上限被共同计算覆盖到 320 DIP。改后含临时用例 158/158，清理后 155/155，过滤器 `.SelectControl.|.TreeSelectControl.|.Cascader.`。Token 渲染 253/253，过滤器 `Select_Token_Has_Effect|Cascader_Token_Has_Effect`。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `724 × 500 DIP`，缩放 2。24 对整窗中 20 对为 0 差异像素，4 对体现预期的高度修复。Select 和 TreeSelect 的局部内边距从 4 调到 16 DIP 时，最终上限由 164 到 176 DIP，候选实际视口保持 160 DIP。Cascader 的显式四行上限保持 128 DIP。三种弹层真实滚轮到达边界，外层 offset 保持零，实际候选选择和滚动条可见性通过。标准场景 1 DIP 扰动检出 12,731 像素，重复与复位为 0。两侧统一关闭光标闪烁以稳定聚焦帧。

## B16 AutoComplete 三个入口的弹层

已通过。三个终端成组读取各自 OptionHeight 和 PopupContentPadding，内边距单独更新时也重算高度。最小宽度移到实际 popup frame 的模板终点。背景继续从输入控件的原资源位置读取。

- 临时用例确认三个入口原来的上限停在 164 DIP，改后随内边距更新为 175 DIP，ItemHeight 和 MinPopupWidth 中转移除。含临时用例以及复用读取辅助后的 Select 家族测试 170/170，清理后 167/167。三个 AutoComplete 终端 Token 渲染 27/27。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `760 × 460 DIP`，缩放 2。标准 23 对整窗均为 0 差异像素，核对最小宽度 210/218/225 DIP、各自局部背景、真实输入、滚动、选中、空结果、取消和重开。局部内边距测试的另外 6 对中，3 对参考帧为 0，3 对只体现预期的限高修复。修复后可见视口保持 160 DIP。1 DIP 扰动检出 12,880 像素，重复和复位为 0。已查看多行输入的实际候选弹层。

## B17 Mentions 弹层高度与最小宽度

已通过。Mentions 成组读取候选尺寸与内边距，最小宽度移到弹层边框读。内边距独立更新后重算高度，实际输入位置、候选插入和焦点保持原契约。

- 临时用例复现上限停在 164 DIP，改后为 175 DIP，并确认移除 ItemHeight、MinPopupWidth 中转。临时用例 1/1，清理后 Mentions 行为、禁用项、语义部件 23/23，Token 渲染 8/8。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `554 × 445 DIP`，缩放 2。标准 10 对整窗均为 0 差异像素。实际输入 `@` 后滚动 300 DIP，过滤并以键盘插入 `@teammate19`，关闭后仍聚焦输入。局部内边距另有一对参考帧为 0，另一对体现限高修复，候选视口由原来的 149 DIP 恢复为 160 DIP。1 DIP 扰动检出 6,277 像素，重复和复位为 0。

## B18 ComboBox 行高与弹层内边距

已通过。行高与内边距从同一资源观察读取，保持 Style 优先级。候选行继续使用有效行高，弹层基于完整的一组有效尺寸刷新高度。

- 临时用例先确认旧实现的实际行高和局部覆盖可用，随后记录两路动态资源绑定。改后用一个资源观察组更新两项值，现有候选行能随主题更新，显式值及清除后的回退均通过。含临时用例桌面 81/81，清理后 80/80，过滤器 `.ComboBox.`，Token 渲染 67/67。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，客户区 `575 × 440 DIP`，缩放 2。14 对整窗均为 0 差异像素。普通及可编辑入口验证 35 DIP 行高和 175 DIP 五行视口，暗色往返恢复到 32 DIP 行高和 160 DIP 视口。真实滚轮、文本输入筛选、键盘选中及清除均通过。1 DIP 扰动检出 10,928 像素，重复和复位为 0。

## DropdownButton 固定宽度箭头对齐

已通过。用户截图中原模板把内容与箭头一起居中。触发器现在将箭头向逻辑尾端停靠，文字和前置图标继续使用内容对齐属性。

- 临时用例改前复现固定宽度的尾端距离错误，改后 3/3。清理后 DropdownButton 语义部件、Button 纯图标和尺寸用例 34/34，DropdownButton Token 渲染 79/79。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，原始 142 DIP 下箭头尾端距离由 37.5 修正至 15 DIP。166 DIP、前置图标、RTL 和实际展开状态均为 15 DIP。五个改动状态各 152 像素，差异只落在箭头上。隐藏箭头与自然宽度的整窗均为 0 差异像素。首轮的额外差异来自 Reset 按钮进入 hover，复验时统一其实际指针状态。

## C01 Button 变体

已通过。活跃变体的资源槽共用一个惰性读取组，字面值、预设色计算、有效值的本地优先级及各终端 Own 与 Shared 作用域保持原契约。

- 临时用例确认两次作用域更改及切换变体输出一致，旧实现默认态持有 10 个独立绑定，改后以一个资源观察组代替。含临时用例桌面 145/145，清理后 144/144，Button、DropdownButton 语义和 Dialog 键盘用例通过，Button 家族 Token 渲染 316/316。 `git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，原始截图间距修复后的基线共 14 对整窗，全部为 0 差异像素。涵盖 Button、DropdownButton、SubmitButton、ResetButton、真实 DialogButton 与 Gallery IconInfoItem。验证局部颜色、预设色、指针进入和按压、禁用、幽灵背景、实际下拉弹层及 Dialog 默认行为。1 DIP 扰动检出 13,360 像素，重复与复位为 0。

## C02 Calendar 适配器度量

已通过。公历和农历适配器各按原所有者读取三项度量。每个 Calendar 共用一组资源监听，实际模板绑定继续处理 Template 优先级、Avalonia 的数值转换和缺值。

- 原实现记录到 3 个独立订阅。改后临时用例通过，验证同一实例公历→农历→公历、局部资源 112 和 135 DIP、本地覆盖清除后恢复到 114 DIP，以及旧农历适配器可回收。含临时用例 172/172，移除后 Calendar 和 DateViewer 171/171。Core 读取及发布相关测试 9/9。
- 以各自正式模板运行公历和农历组件，覆盖月份切换及返回、实际日期数字、选中和范围条、局部三项度量及迷你模式。11 对 Ant Design DaybreakBlue 原生整窗帧均为 0 差异像素。A/A 与复位均为 0，1 DIP 扰动检出 20,159 像素。初始探查使用混合模板出现错误日期文字，已改用各自正式入口重新采样。
- 从源码重建渲染项目时发现 B16 两条验收仍使用已移除的 `MinPopupWidth` 中转。原有隔离断言已迁到实际弹层框的 `MinWidth`，两条入口及 Calendar、LunarCalendar Token 和 Picker 读取清单测试共 98/98。`git diff --check` 通过。

## C03 独立主题输入与宿主重挂

已通过。ImagePreviewerCover、DialogSurface 和 TimePickerPresenter 的补齐路径各共用一组资源监听，输出继续使用 Avalonia 的 Style 绑定。正式图片宿主的继承值和普通／范围时间选择器显式转发的身份仍各自生效。

- 原生复现中，独立封面重挂到已就绪的 Window 后继承了 Window 身份，尺寸输入从 300 降到 0 DIP。独立 TimePicker 面板也用错身份，0 DIP 行高导致循环列除数为零，抛出 `Invalid Arrange rectangle`。现在只有兼容的继承宿主能供应这些输入，独立部件使用自己的默认身份。
- 两项临时测试修改前分别确认旧绑定数组和无关宿主下的 0 DIP。改后含临时用例 281/281，验证装箱整数、空资源回退、本地覆盖与清除、卸载释放和在新宿主下恢复。临时测试已删除，清理后渲染 279/279，桌面 398/398，`git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，720 × 580 DIP，缩放 2。含单选／范围时间弹层及 Dialog／MessageBox 各自的挂树和独立锚点在内，13 对原有正常场景整窗均为 0 差异像素，状态及焦点记录相同。独立封面重挂和清除主题后分别恢复 300 和 200 DIP，只在预期封面矩形内出现 359,995 和 159,995 个差异像素。两个独立时间面板均完成原生渲染，行高 32 DIP。验证了原版单选独立面板的崩溃，原版比较重放跳过了两种独立时间面板，继续记录正式弹层。

## C04 Breadcrumb 分隔符

已通过。每个 Breadcrumb 的所有分隔符从同一组读取 `SeparatorColor` 和 `SeparatorMargin`，分别投射到各自 presenter 的 Template 绑定。删除分隔符时释放其输出，最后一个移除或控件卸载时销毁资源观察，保留条目模板和 Semantic Style 定制入口。

- 原实现每个分隔符各有两个资源绑定，临时测试对共享观察的断言失败。改后 19/19，验证已有和新增的分隔符能读到同一作用域，已删除的 presenter 不再随资源更新。临时用例已删除，清理后含共用输出回归的桌面测试 189/189、渲染 22/22、Core 9/9。`git diff --check` 通过。
- 用户指出的原始验收图中，模板实际绘制的是 19 DIP 的 `›` 文本。行中心 119.5 DIP，文本内容框中心 119.75 DIP，字形的像素中心比行低 2.25 DIP。以冻结的原控件同尺寸重放，使用 `RightOutlined` 作为 `SeparatorTemplate` 时图标和行的中心均为 119.5 DIP。最终示例将图标描边绑定到分隔符前景，保留默认灰色和局部紫色。生产控件的行布局沿用原实现。
- 以修正后的同一原生示例作为前后基线。Ant Design DaybreakBlue，610 × 340 DIP，缩放 2。覆盖焦点、插入、移除、箭头模板、作用域间距与前景、卸载和重挂。12 对整窗全部为 0 差异像素，状态和布局日志相同。A/A 与复位为 0，1 DIP 扰动检出 6,254 像素。已查看原始字形和最终灰色箭头关键帧。

## C05 Flyout 定位目标的资源监听

已通过。一次打开共用定位目标上的资源观察。默认阴影、时长保持 Template 优先级，Pointer 偏移保持 Style 优先级和原始自定义键。成功关闭和未进入等待的打开取消会释放资源组。

- 原版在锚点 A 取消打开后，从 B 重试仍然读到 A 的 17 DIP。修复跟随实际打开目标，读到 B 的 36 DIP，修改 A 不再影响 B。含临时用例 26/26，清理后 Flyout 25/25、Flyout 和 TreeFlyout 直接 Token 渲染 22/22、Core 9/9。追加的 MenuFlyoutPresenter 渲染检查中四条边框及阴影用例仍失败，用同一测试程序集加载冻结的 C05 旧版重跑，复现相同四条。`git diff --check` 通过。
- Ant Design DaybreakBlue 原生 650 × 470 DIP，缩放 2。正常锚定、阴影和时长覆写、关闭重开、Pointer 自定义／Own 键切换锚点、旧锚点更新和移除自定义键共 17 对整窗全部为 0 差异像素。取消后重试的单独重放固定指针起点 `(252, 167)` DIP。修复后由 B 的偏移 `(36, 20)` 算出弹层左上角 `(288, 187)` DIP，旧版为 `(269, 187)`。另外 5 对整窗为 0，唯一变化帧为 29,141 差异像素。已查看实际弹层关键帧，媒体留存。

## C06 Drawer 尺寸、推移和边线

已通过。每个 Drawer 挂载时观察一组 Own Token，尺寸档切换只更换活跃尺寸键。公开尺寸、推移比例、边线颜色和厚度都保留原有 Template 默认优先级。

- 临时测试确认原来没有共享读取组。改后 43/43，核对局部小号 225 和中号 310 DIP、显式 265 DIP、拆卸时清除读取组以及重挂后读到 333 DIP。清理后 Drawer 行为 42/42、Token 渲染 32/32。`git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，800 × 520 DIP，缩放 2。14 对整窗全部为 0 差异像素，开合动画中途、默认右侧、左侧显式 320 DIP、恢复 290 DIP、嵌套推移 58 DIP 和结束释放均有帧。关键画面已查看。早期波动仅在 macOS 标题文字，同一冻结旧版也复现。最终同时检查原生应用激活、主窗口与 key window，保留整窗进行精确比较。

## C07 Space 横纵间距

已通过。当前尺寸档的间距只解析一次，两个布局属性分别消费原 Template 优先级绑定。换档后资源组仍由同一个 Space 持有，离树时释放。

- 原版局部资源单次通知查找 2 次，改后为 1 次。临时验证覆盖本地 ItemSpacing 单独覆写、清除恢复和卸载后 0 次查找。清理前 Space 定向测试 17/17，清理后桌面 16/16、Token 渲染 6/6。`git diff --check` 通过。
- 原生 Ant Design DaybreakBlue，595 × 365 DIP，缩放 2。Small、Middle、Large、Custom、局部 Token 17 DIP、独立项间距 24 DIP、样式触发行间距 29 DIP、换行、卸载与恢复，共 13 对整窗为 0 差异像素。调整示例的两个按钮短标签以适应原固定宽度，用同一驱动重放前后，实际按钮和行的位置逐条相同。

## C08 MarqueeLabel 默认速度与周期

已通过。桌面 MarqueeLabel 挂载期间共用一组读取速度与循环间距，继续通过公开 Avalonia 属性供实际滚动运行时消费。卸载解除监听。

- 临时验证原版为两条单独资源绑定，改后只持有一个资源组。装箱整数、显式速度覆盖、清除后实时读值和卸载回退均通过。清理前 11/11，清理后 `RecurringVisualWorkTests.Marquee|AlertSemanticPartTests` 10/10。`git diff --check` 通过。
- 555 × 320 DIP 的 Ant Design 原生窗口，缩放 2。用 Avalonia 实际时钟驱动滚动，短文本保持静止。速度从默认 150 变为局部 210，再被公开值 90 覆写。周期、偏移、视口、浅深色、停动和卸载日志逐项相同，11 对整窗均为 0 差异像素。已查看实际滚动与裁剪关键帧。

## C09 Gallery 侧栏页脚

已通过。页脚共用一组资源监听，仍在页脚本地解析线宽与次级边框色。原线宽转换只接受 `double`，输出为仅上边的 `Thickness`，两个输出均使用 Template 优先级。

- 原版对共享组的临时断言失败。改后含临时用例 22/22，验证局部 3 DIP、公开 7 DIP 覆盖、清除回到 2 DIP、不符合转换要求的装箱整数回退，以及 Shell Dispose 后移除输出。临时用例已删除，清理后 Shell 折叠与缩放定向用例 22/22。
- 原生窗口保留 GalleryShell、实际 NavMenu 和 RoutedViewHost。按正式侧栏展开入口重放，覆盖默认浏览、局部线色线宽、显式覆盖、清除、浅深色、缩放窗口和恢复。11 对原生整窗为 0 差异像素，布局与边线探针相同，深色关键帧已查看。

## D01 ButtonSpinner、NumericUpDown

已通过。移除了 `ButtonSpinnerDecoratedBox` 无消费者的一条边框厚度读取和对应属性。实际绘制手柄的厚度入口继续用于描边，NumericUpDown 固定步进分隔线继续读取框体的有效厚度。

- 临时物理注册断言修改前失败，改后含用例 8/8，清理后桌面定向 7/7。ButtonSpinner、NumericUpDown 的 Token 效果、独立身份、裁剪和读取清单定向渲染 112/112。`git diff --check` 通过。
- Ant Design DaybreakBlue 原生 615 × 350 DIP，缩放 2。两终端分别记录布局与实际按钮，使用框架指针走过浮动手柄并按下步进，验证填充、不同尺寸、禁用与 NumericUpDown 固定模式。局部框厚 2／3 DIP、对应图标 13／12 DIP 分别沿各自身份应用。11 对整窗全部 0 差异像素，已查看固定模式关键帧。

## D08 DialogSurface 动效时长

已移除共享 `DialogSurface` 的无消费者时长属性和重复的 Owner Token 读取。`OverlayDialogPresenter` 继续使用自身运行时读取的 Slow 和 Mid 驱动实际表面和遮罩。CollapseItem、Expander 和 NavMenuItem 的时长各有真实模板或覆写消费者。

- 原版存在内部注册，临时断言失败。改后含用例 44/44，覆盖早期打开、中途反转、关场回收和 Window 宿主的实际协议。将原渲染用例的断言迁到实际播放 presenter，继续验证作用域时长。临时用例已删除，清理后的 Overlay 动效与 Window host 定向用例 43/43。
- Ant Design DaybreakBlue 原生 Overlay 操作分别验证 Dialog、MessageBox 的开合、局部时长和打断，各 12 对整窗为 0 差异像素，动画和遮罩探针相同。独立 Window 宿主重放实际 Dialog 和 MessageBox 子 NSWindow 及主窗口恢复，共 7 对为 0。已查看原生 MessageBox，子窗口大小、表面尺寸和焦点一致。

## D 阶段的保留项

已按源码、实际属性优先级和资源起点逐项核对。下表各项保留原实现。D01 和 D08 的无消费者读取各有独立改动及验收。

| 项目 | 保留依据 |
| --- | --- |
| D02 WindowNotificationManager | `ThemePadding` 给顶部和底部默认间距，公开 `Padding` 仍可被应用普通样式覆盖。位置触发器直接写 `Padding` 会提升优先级。 |
| D03 RangeDateViewer、DatePanel | `PanelSpacing` 共供标题和正文，RangeDatePicker 有 Template 优先级的独立来源。`WeekHeaderMinHeight` 汇合 Viewer 和 Calendar 两种模式的来源，再写到重复使用的星期标签。 |
| D04 Form | Inline 模板面板从 Form 所在作用域取得间距。错误、警告前景各自通过 FormItem 驱动运行时消息和帮助文字。 |
| D05 CarouselPagination | `ItemSpacing` 从分页控件起始解析。面板直读会纳入中间的 Border、ItemsPresenter 和面板自己的资源。 |
| D06 PaginationNav | 导航的尺寸触发器保留原 Pagination 键。编译绑定把间距以 LocalValue 写到 StackPanel，简易布局有独立的主题入口。 |
| D07 CardTab | 同一宿主厚度同时供新增按钮模板和生成的 TabItem／TabStripItem 本地值。容器复用和拆卸按原入口释放绑定。 |
| D08 开合时长的保留项 | CollapseItem 和 Expander 保留公开的 `MotionDuration`，自定义视口可绑定。NavMenuItem 的 Inline 展开时长保留显式覆写和缺值回退。 |
| D09 AvatarGroup | 重叠度参与测量、总宽和每个子项位置，弹层面板的 GroupSpace 是独立消费路径。 |
| D10 ListBox、ListView | 行分隔线随容器类型解析，保留显式 `BorderThickness`、派生项与 ListView 分组行规则。 |
| D11 Descriptions、Upload | 每个部件用实际字号与比率计算行高，上传标题共享该结果作高度和删除按钮宽度。 |
| D12 TimeView | 一次 LineWidth 读取经过缩放换算，结果供四个分隔线的宽度。 |
| D13 Tour | 原 Own 键与当前步骤公开偏移合成，通过 `SetCurrentValue` 更新 Popup 的实际锚点距离。 |
| D14 Rate | HoverScale 经共享控件计算 `TransformOperations`，由桌面主题在 hover 和键盘起点焦点状态消费。 |
| D15 OptionButtonGroup、SplitButton | 组选中项边框使用组身份和自绘，SplitButton 按主按钮与 Danger 状态提供画刷驱动圆角底层。 |
| D16 小型布局部件 | 候选行间距转换只保留前缘。WindowTitleBar 和 Gallery 元数据行的间距已通过 AXAML 直接送入实际 Measure/Arrange。 |

## 最终定向复验

2026-11-19，`fix/shadcn`，HEAD `eab63ea2c`。在删除本次临时测试后执行 `dotnet test <所属项目> --framework net10.0 --no-restore --filter <目标用例>`，完成后 `git diff --check` 通过，暂存区为空。

| 复验范围 | 通过 |
| --- | ---: |
| Core 的 OwnerTokenSet、TokenResourceBinder 发布、ThemePublishDelta 与 ResourceProvider | 23/23 |
| 桌面 ProgressBar、BorderBeam、Badge、Calendar、DateViewer、ImagePreviewer、TimePicker、Breadcrumb、Flyout、Drawer、Space、Dialog、MessageBox、Marquee、Alert 与 Spinner | 812/812 |
| Dialog、MessageBox、部件读取清单、ProgressBar 与 BorderBeam 定向渲染 | 132/132 |
| 共享 ProgressBar、Badge、InputFrame 主题变量 | 33/33 |
| Shadcn 控件 | 47/47 |
| GalleryShell 折叠与缩放 | 22/22 |

B07 和 C05 记录了冻结旧版也会失败的 NavMenu／MenuFlyoutPresenter 边框及阴影无头用例。两项各自已有通过的原生整窗验收。
仓库原有已跟踪的 `FinalVisualTemporaryTests.cs` 保持原状态，定向复验没有执行该文件。已登记的 89 个项目外媒体目录全部可访问。

最终构建产物 SHA256

- Core `585abb87fe8beb67fef8e173343dfd6ece3eec570fc954d54e14603ceaad0e7e`
- Desktop.Controls `17ab2c42a857f3b2de03919f7aadbf9629bfced48edc7e0f4a2248ea9ef2df24`
- GalleryBase `b0e72607e162b13681515c34e8f34d18d555da957f6e6410ea972f56fe43da96`

## NavMenu 与 MenuFlyoutPresenter 阴影、边框复验

2026-11-19，在 `fix/shadcn` 复现原定向范围 134/140。NavMenu 的两个阴影用例此前走内联菜单。定向改为打开垂直子菜单并比较实际弹层范围，冻结旧版的 Own 和 Effective Global 阴影各改变 52,120 像素。其修复落在测试场景。

MenuFlyoutPresenter 的两个边框颜色用例需要用 1 DIP 厚度激活边框，测试前后都设置该伴随值。原生重放同时复现了打开后的实际缺陷：定位目标和绘制部件都能查到新值，但独立 Popup 未收到定位目标的资源更新。仅转发同一通知的诊断对照已在冻结旧版画出 1 DIP 边框。正式实现把这条订阅并入定位会话，保留边框原来的 Style 优先级，关闭、取消或换定位目标时解绑。

MenuFlyoutPresenter 的两个阴影用例此前只有 presenter 的 Token 更新，顶层 Popup 仍使用 FlyoutHost 默认值。独立 MenuFlyout 现在把同一菜单 Token 接到两个 Popup 阴影属性，仍以 Template 优先级提供默认值。临时用例确认两个宿主属性、实例覆盖和清除恢复。修复后删除临时用例。

清理后的三个定向 `dotnet test --framework net10.0 --no-restore --filter` 批次通过。渲染 150/150，含原 NavMenu 和 MenuFlyoutPresenter 的 140 个用例与通用 Flyout 10 个。桌面 NavMenu、Flyouts、DropdownButton 及 Token 优先级 346/346。Shadcn 的菜单环和阴影 Token 12/12。`git diff --check` 通过，成员布局审计检查两个控件文件，0 项问题。

原生 Ant Design DaybreakBlue，760 × 530 DIP，缩放 2。统一显式关闭 NavMenu 子菜单后再打开 Dropdown，避免两个弹层叠开影响比较。由 `dotnet Acceptance.dll <已登记的帧目录>` 重放，`frame_compare.py` 按整窗 RGBA 精确比较。冻结版 Desktop.Controls SHA256 为 `17ab2c42a857f3b2de03919f7aadbf9629bfced48edc7e0f4a2248ea9ef2df24`，修复版为 `85d1c119c262ce6eab2144f269fc8ccc214112ddc567f35a7b312d08a73af9c9`。

- 22 对整窗中，14 对完全一致，含默认展开、关闭、NavMenu 和全部恢复帧。两边 A/A 为 0，1 DIP 扰动检出 11,864 像素，复位为 0。
- 6 对捕获预期的边框或阴影修复。边框启用后实测 1 DIP，Own 和 Effective Global 给出相同的红色描边。独立阴影两条输入都画出相同的紫色阴影，覆盖为绿色后清除可恢复紫色。
- 两对作用域更新帧有实际边缘差异。0 宽只改边框颜色时 714 像素，每通道最大差 2，指定描边颜色像素为 0。本地阴影覆盖帧 259 像素，每通道最大差 1，指定绿色像素数量一致。清除本地阴影与直接设置 Effective Global 阴影的两张修复版图另有 235 像素，每通道最大差 1。均记录实际差异，未放宽整窗比较的容差。已查看最终边框和阴影原生帧。

## 主题 Token 资源开关与方法命名

2026-11-19，在 `fix/shadcn` 全局检查非 Mobile 代码中成组的主题 Token 消费及其订阅入口。统一 27 个开关和 58 个方法声明，同步 C#、AXAML、现有定向测试及文档引用。将开关命名为 `Use<职责>TokenResources`，以 `Update…TokenSubscription` 表达更换订阅，以 `Ensure…TokenBindings` 和 `Release…TokenBindings` 表达绑定生命周期，以 `Refresh…` 表达应用解析后的值。例子有 `UseExtraInfoSizeTokenResources`、`UpdatePopupLayoutTokenSubscription`、`ReleasePlacementTokenBindings` 和 `RefreshMotionDurations`。

75 个命名变更文件反向替换后与改名前逐字节一致。45 个 C# 文件的成员布局诊断前后相同。最终定向测试通过，桌面控件 1,217/1,217，渲染 320/320。临时验证应用和比较脚本已清理。

原生 Ant Design DaybreakBlue，690 × 500 DIP，缩放 2。圆形及仪表盘进度、分隔线、开关和实际垂直 NavMenu 子弹层按同一顺序重放，7 对整窗 RGBA 差异均为 0 像素。两边各自的重复帧和 1 DIP 扰动复位帧均为 0，扰动检出 12,390 像素，进度值切换检出 4,283 像素，打开子弹层检出 57,119 像素。

## 成组主题 Token 资源键命名

2026-11-19，在 `fix/shadcn` 统一非 Mobile 控件中传给资源查找的键组。属性和类型使用 `<职责>TokenResourceKeys`，例如 `ProgressRingTokenResourceKeys`、`IndicatorTokenResourceKeys`、`LoadingIndicatorTokenResourceKeys` 和 `PopupLayoutTokenResourceKeys`。覆盖 9 个样式属性、7 个键组类型、圆形与仪表盘进度的字号键入口，以及 Flyout 阴影键入口。同步 C# 和 AXAML。

41 个源码和 AXAML 文件反向替换标识符后逐字节一致，旧引用清零。28 个 C# 文件的成员布局诊断前后一致。最终桌面控件定向测试 464/464，Token 渲染 581/581，`git diff --check` 通过。

Ant Design DaybreakBlue 原生窗口，670 × 500 DIP、缩放 2。初轮发现 5,485 个差异像素全部位于原生标题栏，画面显示一侧窗口失活。最终两侧在每次捕获前后检查激活状态，并明确关闭 Select 后再单独打开 AutoComplete。8 对整窗 RGBA 均为 0 差异像素，两侧复位均为 0，1 DIP 扰动各检出 23,468 像素。真实滚动偏移 95 DIP，两种弹层均为 182 × 68 DIP。已查看进度环和独立弹层截图。

## 线形进度条的 Token 资源模式命名

2026-11-19，复核 `AbstractLineProgress` 的主题输入。按 A01 的消费契约，`LineInfoIconSizeSM` 是 Middle 和 Small 档的状态图标尺寸，模板宽高和信息区测量均直接使用它。选择读取组的属性统一命名为 `LineSizeTokenResourceMode`，同步两个桌面控件及三个主题。6 个文件的 14 处引用反向替换后与修改前逐字节一致，成员布局审计无新增诊断。

桌面 79/79、共享 21/21、线形和分段渲染 50/50、Shadcn 47/47。原生 Ant Design DaybreakBlue，620 × 500 DIP，缩放 2。锁屏期间取得的第一轮截图中，切换后分段填色有 448 像素的蓝通道相差 1。后续增加切换后的稳定时间及重复帧，最终逐帧验证窗口保持激活，6 对整窗均为 0 差异像素。两侧 1 DIP 扰动各检出 12,040 像素、复位为 0。档位切换时状态图标从 14 DIP 切到 12 DIP，两个版本的尺寸及状态记录一致。已查看最终切换帧。

## 普通和分段进度条的状态图标尺寸

2026-11-19，两个桌面主题按 `EffectiveSizeType` 把对应尺寸 Token 投射到同一个 `LineInfoIconSize`。信息区测量和图标宽高共用此值，删除小档尺寸的中转属性与 C# 档位选择。两档设计 Token 和原状态图标尺寸样式优先级均保留。

两个临时定向用例在旧实现失败。小档图标和信息区均为 9 DIP，普通尺寸属性仍为 19 DIP。改后两处使用同一个实际尺寸，三档切换、完成与异常状态及局部覆盖都通过。删除临时用例后，桌面 79/79、共享 21/21、线形和分段渲染 50/50、Shadcn 47/47。两个 C# 文件的成员布局无新增诊断。

原生 Ant Design DaybreakBlue，620 × 500 DIP，缩放 2。普通、分段及垂直进度条共 7 对整窗 RGBA 均为 0 差异像素。图标默认尺寸 14/12 DIP，局部覆盖到 18/21 DIP 后清除，两个版本的状态和几何记录一致。1 DIP 扰动各检出 9,423 像素，切档检出 13,028，局部覆盖检出 24,340，重复与复位均为 0。已查看局部覆盖帧。

## 尺寸附加属性收敛

### 1. ColorPicker

删除无读点的小档色谱手柄附加属性和两个 Setter。保留小档设计 Token，将实际滑块尺寸派生直接放在终端 Token 计算入口，生成器可追踪。保留正常与触摸大手柄两路面板输入，原生探针验证面板值优先于色谱节点上的同名局部 Token。桌面定向 20/20，ColorPicker 渲染 183/183。

Ant Design DaybreakBlue，1,140 × 580 DIP，缩放 2。普通、渐变、局部覆盖、两套面板触摸按下和弹层等 9 对原生整窗均为 0 差异像素，两侧默认和弹层重复帧均为 0。先前的窗体尺寸变化、弹层未稳定的 119 像素和面板窄窗裁剪保留为采样诊断。最终查看了完整的手柄验证帧。

### 2. ListView / TransferListView

两个终端在原作用域按档位写一个继承的 `ThemeItemPadding`。删除小档和大档附加属性，条目原有尺寸触发优先级及分组样式沿用。桌面 ListView、Transfer 和 SelectCandidate 定向 74/74，两个终端 Token 渲染 72/72。

Ant Design DaybreakBlue，850 × 580 DIP，缩放 2。分组、切档、局部资源覆盖、条目本地 Padding 和换数据后的复用共 8 对整窗均为 0 差异像素；1 DIP 扰动检出 26,144 像素，复位 0。已查看两宿主局部覆盖帧。

### 3. Button / SubmitButton / ResetButton

三个终端各自选择字号、内容内边距、纯图标内边距及纯图标尺寸的有效值，删除 8 个大、小档附加属性。Custom 继续读取中档值，加载和形态的共享选择器保持各自职责。桌面 Button、Form、跨平台主题资产及隔离测试 178/178，三个终端 Token 渲染 237/237。

Ant Design DaybreakBlue，850 × 580 DIP，缩放 2。8 对整窗均为 0 差异像素，覆盖三终端独立覆写、纯图标、档位与本地 Padding。加载图标的真实模板宽为局部 Token 设置的 19 DIP。1 DIP 扰动检出 16,047 像素，复位 0。已查看覆写帧。

### 4. Pagination / SimplePagination

页码尺寸、外层间距、跳页输入宽度均在宿主选档。删除页码和输入框的小档属性。跳页栏内部间距始终使用普通值，沿用一个独立的 `ThemeQuickJumperSpacing` 传递。外层间距使用有效输入，以保留原宿主查找优先级。验证外层节点有同名 99 DIP 资源时仍使用宿主 3 DIP，内部实际间距为 15 DIP。

桌面 51/51，两个终端 Token 渲染 57/57。Ant Design DaybreakBlue，850 × 580 DIP，缩放 2。7 对整窗全部 0 差异像素，1 DIP 扰动检出 12,715 像素，复位 0。已查看小档覆写帧。

### 5. TabControl / TabStrip

四个终端从原宿主选出标题字号、横向内边距或卡片内边距，删除 6 个分档属性。独立竖向内边距沿用原来源。桌面 283/283，四终端 Token 渲染 187/187。Ant Design DaybreakBlue，850 × 580 DIP，缩放 2，尺寸改造前后 9 对原生整窗全部 0 差异像素，包括横竖、四终端覆写和实际溢出弹层。

用户指出两条标签条阴影低处留白。基线同样存在。核对本地 Ant Design 的 inset 阴影，参考在边缘直至分隔线仍保持有效灰度。原外阴影因 -8 DIP 扩散量沿交叉轴收短，在分隔线前完全淡没。仅扩展绘制矩形的交叉轴，viewport Canvas 沿用原裁剪。原始两处边缘最后 4 像素由 255 变为 241，更多按钮侧仍为 255。整窗 2,484 个差异像素只在四处溢出阴影内。复位、正常重复和弹层重复帧均为 0。修复后桌面 283/283，Tab Token 和专门阴影渲染 259/259。

### 6. Select / TreeSelect / Cascader

三个终端各在原作用域选出标签高度、内容内边距、前缀缩进和空选内边距。删除 7 个分档属性。紧凑多选内边距是独立输入，小档共用同一个 Token 结果。更新现有 TreeSelect 契约用例，仍断言空选小档使用自己的 Token。

桌面 83/83，三个终端 Token 渲染 253/253。Ant Design DaybreakBlue，850 × 580 DIP，缩放 2。10 对整窗全部 0 差异像素。覆盖空选、多选换行、前缀、自定义高度、局部 Token 和真实 Select 弹层，覆写后的两个标签高度为 27 和 19 DIP。已查看多选覆写帧。

### 7. Button 状态画刷

将 9 个内部画刷属性与阴影输出归并为一个 `VariantAppearance`。Button 和 DropdownButton 原有主题选择器继续负责状态选择及最终样式优先级。审查发现先发布阴影可能在回调重入时用旧画刷覆盖新状态。临时用例用实际模板阴影通知重入，旧中间实现保留 Primary 蓝 `#ff1677ff`，合并发布后得到最后一次更新的 Danger 红 `#ffff4d4f` 与对应阴影。临时用例通过后删除，现有行为测试改用归并后的各状态值。

桌面 184/184，四终端 Token 渲染 316/316。Ant Design DaybreakBlue，850 × 580 DIP，缩放 2。通过框架鼠标命中路由采样常态、悬停、按下、禁用、Danger 和本地背景，9 对整窗均为 0 差异像素。切至 Danger 的画刷发布由 6 次降为 1 次。已查看按下帧。

## 成员位置审计修复

2026-11-19，在 `fix/shadcn` 先处理本批新增的 RadioIndicator 和 NotificationProgressBar 属性错位，以及 StepsItemLayoutPanel 公共契约前置和 ComboBox 运行时字段混入属性区。之后处理 HEAD 中已有的同类违规。

共整理 15 个 C# 文件，覆盖审计清单中的 27 项属性和 47 个声明，另移动关联的 8 个 DirectProperty 后备字段。GradientColorPickerTrack.Thumbs 和 Dialog.UsesWholeFrameMinimum 按消费位置归入内部协作 API。两阶段 Roslyn 导入、类型与成员语法指纹分别一致。独立只读复核检查了静态初始化依赖与属性登记顺序。

Ant Design DaybreakBlue 原生 NSWindow，720 × 500 DIP，缩放 2。各项重复与复位均为 0 像素，1 DIP 扰动均可检出。下表改前与改后对数包含场景基线、交互状态及采样校验帧。逐项查看了实际呈现，WindowsCaptionButton 由验收窗口内的托管标题栏场景覆盖。

| 项目 | 定向测试通过 | 零差异整窗对数 |
| --- | ---: | ---: |
| RadioButton | 43 | 5 |
| Notification 新增成员 | 59 | 5 |
| Steps | 203 | 5 |
| ComboBox | 80 | 5 |
| ColorPicker | 20 | 4 |
| DataGrid | 3 | 4 |
| ButtonSpinner | 52 | 4 |
| Dialog | 267 | 5 |
| Menu | 64 | 4 |
| Notification 存量成员 | 59 | 5 |
| Pagination | 51 | 5 |
| Select 家族 | 156 | 4 |
| Slider | 38 | 5 |
| Tour | 21 | 5 |
| WindowTitleBar | 133 | 4 |

最后按同一 Roslyn 规则复扫 3,598 个非 Mobile C# 文件，识别 539 个含 Avalonia 属性的类型声明。internal 注册字段或 CLR 属性混入公共属性区的声明从 47 个降至 0。全局自动布局诊断从 3,957 条降至 3,868 条，改动文件无新增规则诊断。既有自动诊断中自定义分区与跨 partial 的候选仍按源码个别判断。

15 个定向批次共 1,249 次测试执行通过，69 对原生整窗均为零像素差异。临时验收项目已清理，截图在项目外登记至提交时。

## Tab 附加属性收敛

在 `fix/shadcn` 分四组删除 26 项逐值中转。3 项直接绑定宿主属性，6 项绑定到具体模板部件，其余 17 项通过同一个宿主来源绑定。先用字号和一侧阴影验证资源作用域、本地值优先级、缺值回退及容器复用，再推广到其余部件。

四组各有 14 对 Ant Design 原生整窗截图，均为 0 差异像素。用于固定焦点的辅助验收窗口曾遮挡页签，移除后以单窗口重做受影响的验收。清理临时用例后，Tab 定向桌面测试 283/283，Token 与溢出阴影渲染测试 259/259。

## 提交时的媒体清理

117 个已登记的项目外证据目录已通过审计并清理，共 2,205 项媒体。Tab 收敛验收另清理 143 张临时截图。保留本记录中的文字结果和关键数值。
