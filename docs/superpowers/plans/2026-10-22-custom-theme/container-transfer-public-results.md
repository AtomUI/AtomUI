# Transfer 公开 API 往返补充与限定差异（限定已批准）

这是[默认求值结果](container-transfer-defaults-results.md)的补充。此前[34对限定差异](container-transfer-defaults-noise-review.md)已获批准并完成204文件清理；它包含TreeTransfer internal分页分支，不能当作公开分页能力。本次独立纯公开API历史不沿用此前批准，原始比较失败保留。

## 合同、脚本与结果

Mac arm64 Headless/Skia、实际AtomUI Window、scale1、1840×620，en-US及默认字体。两个owner Width760／Height320，独立TransferListView Width240／Height320。List数据18项、目标item-1、选中item-2；树parent/child、目标child。两个版本／主题／main-AA-DIP各独立Window；DIP只移动外部测试Panel左距1DIP。

驱动只使用公开ThemeConfigProvider.Config、ListTransfer.PageSize与公开布局/数据属性。TreeTransfer未设置internal状态；反射仅只读确认其没有公开PageSize/IsPaginationEnabled wrapper。default→Global→Control→Own与List公开分页→reset，正常motion、最小650ms及实际最长Transition后连续三同帧，上限4s。两个版本保留相同数据、视图引用、选择、边界与完整操作历史。

- Global：ControlHeightLG56、PaddingSM24、MarginXXS7。
- Control：List的ControlHeightLG56、Tree62，算法Disabled。
- Own：ListWidth210／ListWidthLG250／ListHeight220／HeaderHeight48／HeaderPadding8,4；Tree为230／270／210／44／10,3，Tree的LG仅验证资源配置，不驱动分页；独立分页视图PaginationMargin0。List.PageSize=3，仅List显示分页；reset改回0及默认配置。
- 捕获入口为Temporary/CT14TransferPublicPath内单一脚本，候选／旧版各30自然帧，通过；媒体之外的状态与几何30对相等。原源仅三个Token回放为V1，finally恢复候选SHA；没有分支/worktree或反射写入。
- 正式60帧：A/A19/20为0、20组1DIP全检出、匹配历史新旧29/30为0（所有默认为0）；12组恢复各Light38／Dark33像素非零。仅旧版Light-main Own的一处4像素同时造成AA与新旧比较失败。
- 独立直接Skia80帧：40组A/A、16组恢复、40组新旧全0。只读局部绘制输入20组匹配历史新旧一致；恢复时懒创建节点使完整数组不相等，不能宣称整棵状态一样；在非零区对应的2个CheckboxIndicator／PixelAlignedBorder绘制节点，8组版本/主题/main-AA恢复相同。尚未证明每个历史合成内部原因。
- 已实际查看新暗色Own画面：List公开分页为6页，Tree保持非分页，分区/标题/选中与公开输入相符；没有改模板、隐藏滚动条、关闭motion、裁剪、对齐或通用容差。

三份产品源码与此前资源/NativeAOT验证的SHA保持；此次仅补充证据，没有新增产品修复。临时C#已删除，永久用例净新增0；清理后原Transfer44项、0Skip，通过。临时采样存在xUnit1051取消令牌提示，未写入产品，最终已删除。用户2026-10-26明确回复“接受补充表内14对限定差异”，仅覆盖下表PUB01–PUB14与Q01–Q05所列范围；原始失败计数未改。140张仓库外PNG及manifest共141文件已由evidence.py清理，运行目录不存在，Git媒体审计clean。

## 已批准的14对限定

仅适用以上Host、完整历史、源版本与本表离散点；不包含Native显示面、其他配置/历史、Git或完整家族验收。两侧alpha均255且不变。每对仅接受所列离散坐标、像素数量和RGBA单通道绝对差上限；比较器继续保留真实失败和计数。

| ID | 类别 | before → after | 坐标集 | 像素上限 | 单通道上限 |
| --- | --- | --- | --- | ---: | ---: |
| PUB01 | AA | `public-old-light-main-own.png` → `public-old-light-aa-own.png` | Q01 | 4 | 4 |
| PUB02 | restore | `public-old-light-main-default.png` → `public-old-light-main-reset.png` | Q02 | 38 | 1 |
| PUB03 | restore | `public-old-light-aa-default.png` → `public-old-light-aa-reset.png` | Q02 | 38 | 1 |
| PUB04 | restore | `public-old-light-dip-default.png` → `public-old-light-dip-reset.png` | Q03 | 38 | 1 |
| PUB05 | restore | `public-old-dark-main-default.png` → `public-old-dark-main-reset.png` | Q04 | 33 | 2 |
| PUB06 | restore | `public-old-dark-aa-default.png` → `public-old-dark-aa-reset.png` | Q04 | 33 | 2 |
| PUB07 | restore | `public-old-dark-dip-default.png` → `public-old-dark-dip-reset.png` | Q05 | 33 | 2 |
| PUB08 | restore | `public-new-light-main-default.png` → `public-new-light-main-reset.png` | Q02 | 38 | 1 |
| PUB09 | restore | `public-new-light-aa-default.png` → `public-new-light-aa-reset.png` | Q02 | 38 | 1 |
| PUB10 | restore | `public-new-light-dip-default.png` → `public-new-light-dip-reset.png` | Q03 | 38 | 1 |
| PUB11 | restore | `public-new-dark-main-default.png` → `public-new-dark-main-reset.png` | Q04 | 33 | 2 |
| PUB12 | restore | `public-new-dark-aa-default.png` → `public-new-dark-aa-reset.png` | Q04 | 33 | 2 |
| PUB13 | restore | `public-new-dark-dip-default.png` → `public-new-dark-dip-reset.png` | Q05 | 33 | 2 |
| PUB14 | oldNew | `public-old-light-main-own.png` → `public-new-light-main-own.png` | Q01 | 4 | 4 |

## 离散坐标集

整图物理像素原点左上，以下是点全集，不以包围框扩大范围。

### Q01（4点）

y=260: x=26；y=261: x=27；y=272: x=27；y=273: x=26。

### Q02（38点）

y=251: x=15,17,24,26；y=252: x=14,15,16,25,27；y=253: x=13,14,28；y=254: x=13,14,27,28；y=255: x=13,28；y=262: x=13,28；y=263: x=13,14,27,28；y=264: x=13,14,28；y=265: x=14,15,16,25,27；y=266: x=15,16,17,24,25,26。

### Q03（38点）

y=251: x=16,18,25,27；y=252: x=15,16,17,26,28；y=253: x=14,15,29；y=254: x=14,15,28,29；y=255: x=14,29；y=262: x=14,29；y=263: x=14,15,28,29；y=264: x=14,15,29；y=265: x=15,16,17,26,28；y=266: x=16,17,18,25,26,27。

### Q04（33点）

y=251: x=16,17,24,25；y=252: x=14,15,16,25,26,27；y=253: x=14,27；y=254: x=13,14,27；y=255: x=13,28；y=262: x=13,28；y=263: x=13,14,27；y=264: x=14,27；y=265: x=14,15,16,25,26,27；y=266: x=17,24,25。

### Q05（33点）

y=251: x=17,18,25,26；y=252: x=15,16,17,26,27,28；y=253: x=15,28；y=254: x=14,15,28；y=255: x=14,29；y=262: x=14,29；y=263: x=14,15,28；y=264: x=15,28；y=265: x=15,16,17,26,27,28；y=266: x=18,25,26。

## 当前出口

公开使用往返的14对独立限定已获批准，141文件已清理。此批准不含Git、其他Host、原生最终像素或完整家族验收。TreeTransfer.ListWidthLG已按下述既有共享属性路径归为Active条件消费者；不新增分页API。完整Transfer状态/Part/输入/生命周期、其他Host与原生最终像素仍开放。

## 共享属性标识补查

此补查与以上14对视觉历史分开，不扩大其批准。TransferListView.IsPaginationEnabledProperty 由 AbstractTransfer 原属性 AddOwner 得到；本地 Avalonia 源码的 StyledProperty<T>.AddOwner 注册新owner后返回同一对象。只用公开的该标识对 TreeTransfer 调用 SetValue／ClearValue，可以进入既有大宽度选择器；没有反射写入或新增产品代码。TreeTransfer 仍没有公开 PageSize／IsPaginationEnabled wrapper，源 TransferTreeView 的 IsSupportPagination 仍为 false。

临时属性／几何探针在旧版Token与V2各明暗2项通过，两版16个阶段的资源消费和两个分区实际宽度相同：default180、Own未启用210、共享标识启用270、Own更新290、标识撤回210、再启用290、配置撤回250、全重置180。由此将 ListWidthLG 从NeedsDecision核定为Active条件消费者，默认未分页树不消费；不将其解释为新公开树分页产品合同，也不增加主题编辑器分页开关。

首份探针使用不存在的IsRegisteredOn方法，属于编译准备失败；改为框架公开Registry查询后执行有效对照。临时C#已删除，净增常驻0，无新增媒体；候选三Token源码SHA保持。清理后的Transfer命名空间34项、0Skip通过，测试2秒；随后按此前同一 `FullyQualifiedName~Transfer` 范围核对，含命名空间外相关用例共44项通过、0Skip、测试3秒。不以属性检查推定原生最终像素或完整家族通过。
