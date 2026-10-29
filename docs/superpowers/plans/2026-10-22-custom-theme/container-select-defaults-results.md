# CT14 N3-SC：TreeSelect / Cascader 默认求值结果

2026-10-28；现有feature/custom-theme，HEAD346732602。此为TreeSelect1/Cascader12字段的本机默认求值迁移，不关闭CT14、完整家族或CT25。

## 实现与有效前证

两个Token类改用DependencyDefaultsV2、声明修订1、IControlTokenDefaults和13个静态DefaultFor函数。全部只依赖有效Global或原常量；没有Own间依赖、额外缓存/订阅、ControlTheme或交互修改。保留Tree最小弹层宽300，Cascader184/111/180三个常量及OptionPadding原Round算术。公开字段名称、类型、key、ID、slot保持。

Temporary/CustomThemeContainerSelectDefaults的真实Startup descriptor在原版两项FAIL，明确实际V1而预期V2，资源正对照1PASS，总1s。第一次未挂载ThemeContext的准备失败不计RED。原版明暗26默认值以及配置/Own/清除共104次实际值与type/key/ID/slot已保存；候选严格相等。

最终5PASS、0FAIL/Skip、9s：两个运行时V2/非零摘要、全13字段兼容、显式13Own值跨Global改变不丢失/撤回按当前Global求值、同Panel重挂及完整视觉轨迹。Global变化后撤回得到HeaderHeight39、OptionPadding17,14,17,14；三常量恢复184/111/180、Tree300。扩展重挂检查首次错误地把仍有父容器的节点放入新Panel，产生1个fixture异常；修正为重挂同一Panel，不据此修改生产代码，原失败保留。

## 原场景与像素

860×620 Window、同实例两根、24条原始选项、Light/Dark真实Window variant与对应scope算法；四尺寸默认→selected→disabled/enabled→Control Global配置→两个实际overlay弹层→关闭/清除。保留标题、数据、Tree名义三行预算、Cascader原高度预算、原裁剪和真实extent>viewport。驱动通过公开API配置/打开，外部按钮固定焦点和指针；没有证明原生raw输入、键盘、滚轮或所有命中边界。

最初12条数据不足以触发Cascader配置后的溢出：实际Max460/extent428，因此严格滚动断言FAIL，原图/数值/日志保留文字。改为24条后两后端原版各1PASS8s；先完成前证，再改产品。增加数据是为了实际触发溢出，没有删除高度限制、隐藏内容或改变生产滚动行为。

两后端各28对旧新，合计56对RGBA全部0；各后端28条根Bounds/选择/启用/打开/13字段记录相等。16对同版本selected→enabled/clear恢复均0。A/A及1DIP检错如下；没有通用容差、对齐、缩放、裁剪或更新基线。

| 阶段/后端 | 主题 | A/A | 1DIP检错像素 | 恢复 |
| --- | --- | ---: | ---: | ---: |
| before | Light | 0 | 2903 | 0 |
| before | Dark | 0 | 2896 | 0 |
| after | Light | 0 | 2903 | 0 |
| after | Dark | 0 | 2896 | 0 |
| before-direct | Light | 0 | 2915 | 0 |
| before-direct | Dark | 0 | 2908 | 0 |
| after-direct | Light | 0 | 2915 | 0 |
| after-direct | Dark | 0 | 2908 | 0 |

作者实际查看明暗两个弹层、已选和清除关键图：文字内容、原列宽、选中区域、原高度/裁剪及恢复前后一致。无本切片新增视觉差异，无需扩大此前S6或任何旧许可。此为作者另步自审，遵守单agent约束。

## 原生与构建

独立系统临时工程引用当前产品及同资源探针，导入仓库Build/Packages/NativeAOT构建合同，输出仅在临时目录。实际NSWindow、两scope算法、四配置阶段、全部13字段与原版104值比较；managed123项通过。首次缺少扩展方法命名空间是工程准备错误，未改生产代码。首次AOT将相同字重枚举600格式化为SemiBold，managed基线为DemiBold，字符串比较失败；原日志保留。驱动仅该字段改为Enum.Parse后比较精确整数，复跑managed仍123PASS（600/400实际值打印）；未修改产品或放宽像素。最终AOT实际运行123PASS，实际打印SemiBold600=基线DemiBold600、Normal400=400。NET8 Release已通过，0警告/0错误、20.42s；ILC构建无IL2026/IL3050，五条本机OpenSSL/Brotli最低OS链接警告保留，不据此外推部署系统范围。

精确命令：

```bash
CT_CONTAINER_TEXT="$PWD/.superpowers/sdd/completion-current" CT_CONTAINER_STAGE=before dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeContainerSelectDefaults.DefaultsTests
CT_CONTAINER_TEXT="$PWD/.superpowers/sdd/completion-current" CT_CONTAINER_STAGE=after-direct CT_CONTAINER_CAPTURE=direct dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~Temporary.CustomThemeContainerSelectDefaults
dotnet publish <临时目录>/DefaultsNative.csproj -c Release -f net10.0 -r osx-arm64 -p:PublishAot=true -p:SelfContained=true
dotnet build src/AtomUI.Desktop.Controls/AtomUI.Desktop.Controls.csproj --framework net8.0 --configuration Release --no-restore
python3 .agents/skills/atomui-control-optimization/scripts/check-member-layout.py --path src/AtomUI.Desktop.Controls/TreeSelect/TreeSelectToken.cs --path src/AtomUI.Desktop.Controls/Cascader/CascaderToken.cs
```

成员审计2个Token文件、0个自身Avalonia Control、0问题。正式Token/changelog同步，CSV严格仅13字段/2个public root更新，未把静态分类升级为全状态/全平台已验收；无Gallery使用形态变更。

## 已发现但本包未修改的消费者边界

Cascader的DropdownHeight在资源中180，主题先写MaxPopupHeight，而AbstractSelect随后按ItemHeight×DisplayPageSize写同一DirectProperty；本场景ControlHeight46、DisplayPageSize10时实际460。故不能把本包资源值比较当成该字段最终高度已生效，完整家族仍有高度所有权问题。Tree行数与实际非均匀/分组高度也不由本包关闭。

前序只读消费者核对另用3个临时实际根：父字号23、无关LineEdit.InputFontSize39时，三个可见过滤输入实际23、IsCustomFontSize=true，清除14；常规路径没有该LineEdit字体泄漏。Select标签字体23且禁用色跟本owner；Tree/Cascader标签仍复用Select Own字体14/禁用色#3f000000，即使根Global禁用色#ff006daa。内部TagAware.Foreground不是最终Tag文字的唯一来源；不能通过改一处Shared就宣称修复。该身份/配色边界仍按既有共享Select合同保留，本次未改产品；3PASS910ms后临时探针已删除，无媒体。

## 清理与最终验证

本包所有新测试均临时，不晋升常驻。保留保障为既有Core默认求值/生成器和原TreeSelect/Cascader契约测试；常驻净新增0。3份临时源已删除；外部原生工程422文件（1,228,223,829 bytes）、112 PNG与manifest共113文件已删除，媒体audit clean，无本包进程遗留。清理后 `dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj --framework net10.0 --no-restore --filter FullyQualifiedName~AtomUI.Desktop.Controls.Tests.TreeSelectControl.|FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Cascader.` 共77PASS、0FAIL/Skip、6s。git diff --check通过；该过滤器在shell须整体单引号包围。无本包新commit、分支/worktree、推送或发布；整任务完成后commit授权未消费。
