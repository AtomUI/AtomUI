# CT15 Alert 消费结果

2026-10-30，既定feature/custom-theme及指定目录，主agent实现/另步作者自审。没有新分支/worktree、子agent、提交、全量回归或依赖替换；其他家族未提交改动保持。本包仅Alert的1 metadata/10 Own，Card及完整CT15继续。[计划](alert-consumers.md)、[限定视觉](alert-consumers-visual-review.md)。

## 实现与原合同

- T23的CloseIconSize、MessageWithDescriptionMargin接入OptionalOverride，仅规范化配置明确提供Own时激活。资源默认仍为FontSizeIcon+2和bottom UniformlyMarginXS；未配置的实际关闭图标继续有效Alert Global IconSizeSM、标题Margin仍0。标准资源14与实际12保持不等价，明确37/0及清除有效；标题覆盖Label和Marquee，仅在有描述时应用。专用Title/Close Style优先。
- 21处原Shared资源输入选择实际Alert owner，覆盖四状态背景/边框/图标、根圆角/描边、间距、标题字号/行高和兼容关闭glyph默认。Info glyph继续ColorPrimary，不改为ColorInfo。Close自身独立IconButton的颜色/输入合同保持。
- 根FontFamily/FontSize/Foreground复用Core有限继承源与3输入存在位选择，C#只读桥暴露真实InheritanceParent。无配置保留原生父字体/前景，本地值优先；标题继续FontSize/FontSizeLG角色，根本地FontSize控制正文。没有SizeType、新字体旗标、C#样式绑定或新helper。
- 图标颜色原本也会被模板属性的Local资源绑定压住。生成Style的Tag命中而FillBrush不生效，证明是优先级而非route问题。四个Owner资源绑定显式使用Template优先级，保留原模板和节点，使专用AlertIconStyle.FillBrush实际生效。
- 10 Own迁DependencyDefaultsV2 Revision1，所有默认只读原有效Global/常量，无Own依赖边。四算法40个默认/type/key/slot、实际默认close12与title margin0逐字相同；整个生成TokenResourceConst源SHA也相同。源码公共字段、事件、Part、伪类、theme key及初始化注册顺序保持。

CloseRequest仍只通知、不自动隐藏/移除；ExtraAction/Description/短标题Marquee/图标/关闭可见性保留静态节点。正式overview/implementation/token/semantic-part/changelog同步，Gallery既有六类专用Style及自动API/Token表形态不改。

## 有效运行验证

临时源在Desktop Temporary/CT15Alert，只有加载AXAML的code-behind。有效RED **15项中9 FAIL/6 PASS，3.4592s**，编译错误不计产品RED；provider编译失败另捕获并断言为空。两个Own、四Type局部Global、字体与Style退出恢复失败，已有8 Own及四算法默认作正对照。

第一版候选的四项框体断言误把algorithm Disabled的LineWidth直接覆盖当成BorderThickness重新派生，已改为直接配置实际BorderThickness；不修改算法语义，也不把夹具误判记为产品失败。增强实际FillBrush断言后，候选**23 PASS/1 FAIL**；明确Template优先级后**24 PASS，3.6951s**。完整旧源码渲染亦独立检出32项图标Style颜色失败，原版也有该缺陷。

扩展后**25临时+9既有=34 PASS，6.0037s**：

- 全部10 Own实际目标、全零/清除、root本地Padding优先；四算法40默认及10个有效Control Global默认输入，原key/slot保持。
- 实际glyph37×37，带/不带描述的自然高度均增长且清除恢复同实例；无描述普通配方Own场景48→53、Compact44→53，有描述85→98。固定高度场景也保留。
- 当前parent字体动态更新、同Alert两provider往返、六生成专用Style和关键Setter实际命中/优先级。
- 真实鼠标close/action与Enter、disabled不响应、IsClosable切换，重模板后的旧按钮退订和新按钮事件保持；旧节点仅用于读取或事件订阅断言，没有找节点写样式。
- 长Marquee正常Dispatcher循环下持续移动，热字体/文字更新后继续，清除恢复原字号/内容与实例。
- 长寿命provider保留时，移除Alert/title/description/close四个WeakReference释放。

## 完整渲染与AOT

真实Skia、UseHeadlessDrawing=false、scale1、1280×840；四算法各8根、四Type×有/无描述、Label/短Marquee、原消息/详情/Action/关闭保留。固定Height180矩阵19阶段，另独立自然高度7阶段；局部原生尺寸540×160与恢复亦保留。所有A→B→A保持实例，采样正常推进Dispatcher、至少700ms+12稳定图像/属性帧，不关闭motion。

三产品源只在原目录、持有checkout OS锁回放旧代码，finally恢复候选并核对文本相同；同一驱动和相同完整操作历史。旧固定矩阵**756检查/272失败**、自然矩阵64检查/48失败，均非零退出；候选两矩阵**756+64=820检查全部通过**。额外候选独立重复固定矩阵756通过。

当前Mac osx-arm64 NativeAOT真实启动，同两矩阵**820检查通过**。JIT/AOT固定76对中75对0，另1对Light Compact disabled为288像素/max2；自然高度28对全部0。因此合计**103/104为0，1对仍未通过**，不写成整组视觉通过。发布无IL警告，五条已知Homebrew dylib最低OS linker警告保留；仍是Headless/Skia，不替代NSWindow系统最终面或其他Host。

旧新固定76对：20对为明确Own/0、局部Global/字体和图标Style生效的有意变化；另4对禁用/恢复微差仍未通过。自然28对只有8对Own/Global有意变化，余20对0；合计104对中72对0、28对有意改变、4对未通过。详见限定表，不继承CT12或其他包批准。

同版本A/A、1DIP正对照和恢复、全部本地值/Own/Global/Part/RTL恢复已执行。固定矩阵disabled恢复到最初默认，候选普通Light为0，Light Compact281、Dark185、Dark Compact199像素仍非零；单独保留，不用之后RTL恢复为0替代该原始失败。所有新旧的初始默认、已有原生定制、RTL及真实输入对照均为0；Part覆盖Own的尺寸/Margin与几何保持，图标颜色的有意变化另列。

已查看固定Own、自然Own与暗色Part等完整画面。原有局部Dark继承黑字和任意固定高度/自设行高下的裁剪不在本包自动修复范围。Part字体37例子配行高44，保持原始文字及字段，不以缩短数据/隐藏节点通过。

## 未通过微差的范围

| 比较 | 场景 | 像素/max通道 |
| --- | --- | --- |
| 旧新 | Light Compact disabled | 288/2 |
| 旧新 | Dark disabled | 228/1 |
| 旧新 | Dark disabled恢复 | 185/1 |
| 旧新 | Dark Compact disabled | 228/1 |
| JIT/AOT | Light Compact disabled | 288/2 |
| 相同候选产品独立重复 | Dark disabled | 228/1 |
| 相同候选产品独立重复 | Dark Compact disabled | 228/1 |

放大图显示差异集中Action按钮四角细边，未发现Alert正文/图标几何改变。相同候选重复证明该现象可在无产品代码变化的轨迹中出现，但未因此认定完整根因或把原始FAIL改成PASS；不挑选重复轮替换原图，不设通用容差，不更改已关闭CT12结论。CT25/FW1既定原始问题仍保留，本次观察不自动归入已获批准边界。

## 清理与交付门槛

25项临时验证所在3文件/24507 bytes已删除；434外部工程文件/1085093835 bytes及795取证文件已删除，evidence cleanup审计clean，永久测试净增0。清理后实际重建，精确AlertSemanticPartTests **9 PASS/0 FAIL/0 Skip，720ms**；Desktop NET8 Release（覆盖Common/Core）**0 warning/0 error，22.95s**。成员审计2文件/1控件/0问题，静态注册/字段初始化顺序未调整，未发现新helper/旗标/临时代码残留。

```bash
dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Feedback.AlertSemanticPartTests
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj -c Release -f net8.0 --no-restore
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/Alert/Alert.cs --path src/AtomUI.Desktop.Controls/Alert/AlertToken.cs
git diff --check
```

作者依照审查清单另步检查三产品源增量、默认兼容、属性优先级、有效owner、无反射/AOT路径、0/清除、CloseRequest/Marquee/重挂及资源获取释放。仅作者自审，遵循禁用子agent要求，非独立评审。没有新增Critical/Important代码问题或搁置代码minor；未通过像素不作为minor豁免。

Ruling：两个历史字段采用明确Own激活以保留原默认；自动施加资源默认会改变旧glyph和留白，因此不采用。Ruling：保持标题角色字号和独立IconButton职责；误将root字号/全部Global复制到子Control会破坏原尺寸/状态。Ruling：沿既定目录/分支无委派/提交，28对有意视觉独立审查，未通过微差与完整CT15继续，不能按别包批准放行。工程验证和有意变化不等于完整家族/CT15验收；仅[限定表](alert-consumers-visual-review.md)的28对有意变化申请独立审查，其他未通过明确保留。

最终产品输入SHA256：

- `src/AtomUI.Desktop.Controls/Alert/Alert.cs`：`c4d8e07c3220057746d4c59208fe941ae7e1331bd38efeffd4bb4565873e9e52`
- `src/AtomUI.Desktop.Controls/Alert/AlertToken.cs`：`122cfc04f67c4aac2c6a61aabe8bb9989f77e3fcc01172e3e6cb8ae8f4ce46fa`
- `src/AtomUI.Desktop.Controls/Alert/Themes/AlertTheme.axaml`：`59774d9ab134107dbcbb7e98d0aa0c11dede287da1bd9fd9646baa456945bd25`

## 2026-10-30 限定批准登记

2026-10-30，用户明确回复“批准 Alert 表内28对，其余未通过与完整CT15保留”（request_user_input_async / call_8790c4384212492cba2b1512c1e8b335 / item 0）。仅表内28对有意Own/Global/Style变化已获限定批准；额外4旧新/1AOT微差、P3未通过及完整CT15保留，不授权Git提交。
