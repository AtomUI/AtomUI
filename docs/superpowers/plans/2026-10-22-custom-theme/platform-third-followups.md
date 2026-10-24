# CT-25 第三轮后续执行包

用户已批准12文件快照，按批准内容创建847c441c1、推送既有feature/custom-theme并运行[第三轮验证](https://github.com/AtomUI/AtomUI/actions/runs/40068141876)。三个真实OS NativeAOT与两个Browser模式均通过；三个选测任务失败。此次提交批准已执行完毕，不扩大为本包的提交许可。无新分支、worktree或NuGet发布。

第二轮后续的补充Linux SDK10.0.402两项真实包消费已通过（478.552秒）；其只读容器的编译调试文件输出设置及Python依赖问题是宿主问题，失败记录仍保留。修正宿主后未改生成器语义；完整2通过计数已核对。容器及匿名产物卷均已删除，当前checkout只读挂载未改。

## W8 Windows 清单验证宿主

Windows已经越过BOM解析及项目helper路径选择。85项工具验证中84项通过，原有FullRegressionInventoryTests在CreateProcess直接执行.sh时以WinError193失败，尚未执行.NET选测。

现有清单测试改为显式Bash执行、原生PATH分隔符，并以UTF-8／LF写入两份临时命令stub。启动脚本前严格确认dotnet与sleep命中临时stub，防止PATH设置失败时误调用实际回归测试。这是已有清单检查，仍不构建或执行任何真实.NET测试，全量套件没有因此在本地运行。

三个临时用例直接执行该现有测试并模拟原生宿主、分号PATH与CRLF默认写入，原版3失败，修改后3通过；2,532字节临时文件已删除，净新增常驻用例0。第一次最终iterate在其他主题原型同时变动时被输入快照门禁拒绝，不能算通过；恢复并冻结源码后，显式该文件路径的tooling iterate通过48.935秒。实际Windows复跑仍待后续审查和验证。

## W9 真实渲染下的剩余控件合同

Mac Desktop为4,122通过／6失败，六项完整名字已取得：

- OverlayDialogPresenterTests.CloseAsync_During_Opening_Waits_For_Opening_To_Settle_Before_Closing_Motion
- ProgressBarSemanticPartTests.Line_Template_Visuals_Stay_Inside_Their_Gallery_Like_Layout_Bounds
- OverflowTipTests.ContentPresenter_Uses_Clipping_Ancestor_Width_When_Own_Bounds_Are_Wider
- MessageSemanticPartTests.Root_Frame_Has_No_Own_Margin_And_List_Insets_ListContent_Symmetrically
- TourPlacementTests.Same_Target_StepChange_Preserves_Flipped_Arrow_Direction
- DashedBorderClipContentTests.Clip_Is_Not_Applied_When_The_Platform_Cannot_Hit_Test_The_Rounded_Figure

Linux Desktop为4,116通过／12失败。日志尾部包含ProgressBar、Tour和Message三项，以及StepsLayoutMatrix的居中／SubHeader六个用例；另有StepsCompressionTests.Horizontal_Compressed_Items_Wrap_SubHeader_Onto_Its_Own_Line_Instead_Of_Clipping的栈名。原验证器错误尾部限16KB，尚有两项完整名字未恢复，不能把现有片段称作完整12项清单。

后续按具体模块与精确类复现并取得完整TRX，禁止自行运行本地完整Desktop或全量回归。先区分真实产品缺陷、渲染能力假设、像素取整及动效观测时序；未证实前不修改产品或放松断言。涉及pointer／overlay／clipping时记录全部输入几何，保留原尺寸、正常motion、完整resize与输入轨迹；默认视觉变化遵守单独审查门槛。

调查顺序：DashedBorder的实际能力与强制正反能力夹具；ProgressBar边界与Message居中的逐像素取整；OverflowTip的ContentPresenter实际文本与祖先裁剪；Tour同目标切换的Popup真实flip状态；Dialog opening／closing的实际顺序；Linux Steps的完整原几何与状态。每项有效反例、最小修复、临时验证清理及定向出口分别记录，再汇成本包审查快照。

本地六个精确类共149项，144通过／5失败；Dialog的时序失败本次未复现，不能据此忽略CI记录。只读探针确认Message容器宽600、队列宽73，理想左右间距263.5；Avalonia12.1.4将origin按Math.Round取264，余量263。原断言要求物理左右完全相等不成立；改为独立取整公式与宽度守恒，两者精确相等，没有像素容差。DashedBorder原负例隐含“当前渲染器不支持圆角containment”，改为与既有正能力子类配对的显式负能力子类，保持200×100及完整原操作。两个原有类共20项通过，未增加常驻用例。

Progress原逻辑标签rect为(571.9794921875,2,28.0205078125,14)，子Canvas实际为(572,2,29,14)，右缘601超出600。候选在模板视觉路径先采用Avalonia实际向上取整的标签尺寸再计算锚点，保留非模板自绘的逻辑度量，35项既有Progress语义检查通过；默认视觉和补充条件尚待验收。

Tour只读记录显示同一个target、相同RequestedPlacement、相同216×142弹出内容与VerticalFlipped=true，步骤切换后没有PositionFlipped新事件，ArrowPosition却被重置；这是状态更新错误。OverflowTip实际文本宽320.876953125，presenter宽420、祖先clip宽90，现实现只检查420，漏掉真实裁剪。其修复与生命周期验证仍在进行。

Tour候选保留同目标／同placement的实际翻转，两个既有类21项通过；第一步明暗整图0，第二步明／暗17,780／9,287像素，卡片与阴影随箭头恢复上移6 DIP。8组A/A全0、8组1 DIP检错通过，用户已明确允许默认翻转修复；27份媒体已删除且审计无残留，见[审查](tour-flip-results.md)。仍需本包清理后最终定向验证，不扩大为完整Tour家族通过。

上述三个触及的产品文件先完成单独成员布局整理：Progress35、OverflowTip44、Tour113个成员的Roslyn token全部相同，导入、类型头及其他声明亦相同；13项布局问题归零。Pinned属性登记提前只调用独立Popup.AddOwner，其余有初始化器字段相对次序不变。布局补丁独立保存，后续行为修改另列，未取得新提交授权。

Linux arm64 SDK10.0.402只读挂载当前checkout，以847c441c1的七个相关文件字节复现八个精确类，182项为170通过／12失败；所有候选字节在finally恢复。本地完整TRX取得12个名字，与CI可见的十项一致，另两项为OverflowTip与DashedBorder；CI原始日志中缺失的两个名字仍不能直接读取，这个对应关系是本地复现支持的判断。该宿主是本地Linux arm64，不冒充CI Linux x64。原容器apt的universe索引阻滞已终止并留存失败；改取本次所需main仓库后依赖准备通过，没有源码副本或新分支。

Steps七项均已核定为测试几何假设错误，未改产品Steps：Linux中Middle标题高19、行高32，理想Y=6.5按偶数取整为6；副标题高17、理想Y=7.5取8；两者中心15.5／16.5，不能要求都等于16。Small高17、行高24，Y=3.5取4；65高item在420高host中Y=177.5取178。改用独立框架取整公式精确核对位置，未增加容差。压缩失败在factor0.85的末item，真实Section宽101可容纳52+49的标题组合，说明文字宽84不能代表标题区容量；检查改读Section，保留全部原尺寸、数据及29段压缩轨迹，仍要求实际发生堆叠且不裁剪。只读探针已恢复，1,764字节临时helper已删除；Linux两类33项通过。

Dialog仅改变临时观测循环20ms→1000ms，原640×480／320×180、80ms motion、Show后立即Close的全部公开输入保持，稳定复现原CI断言closeTask.IsCompleted为true；300ms对照曾未复现，未当作红例。产品CloseCoreAsync先等showTask再启动closing，测试错误地要求MainLoop返回时必须仍处于中间态。现有用例改为在opening Task完成时同步观察opacity1／transform复位及close未完成，并在Close调用后立即检查pending；保留正常关闭和dispose出口。同一1000ms观测条件通过，临时改动已恢复；没有改Dialog产品代码、时长或超时。

CT-16页头原型另见[日期页头审查](date-viewer-header-results.md)，未混入本次CI提交。CT-24继续暂缓，其余家族与CT-25／CT-26完整出口继续开放。

本轮同一候选在Mac与本地Linux arm64各运行10个精确类，均198通过、0失败（Progress语义/行为、OverflowTip、Tour placement/语义、Dialog、Message、DashedBorder、Steps布局/压缩）；这是当前实现检查点，Progress临时验证尚未清理，不能替代清理后的最终出口。NET8 Desktop Release构建零警告、零错误。OverflowTip的临时源与媒体已清理，用户已允许默认祖先裁剪修复，见[独立审查](overflow-clip-results.md)。

独立只读代码审查已覆盖当前产品修复、AXAML优先／Style覆盖、祖先观察释放及现有测试合同，未发现新增Critical或Important问题。完整家族、原失败Range生命周期、精确任意裁剪路径和原生最终像素均单独保留边界；Progress的7像素限定合成差异仍待用户审查，不把代码审查视为视觉豁免。

## 本包清理后的出口

Progress的6份临时源、13份完成的辅助脚本／源备份及10轮586份取证专用文件（含标记）已清理，全部媒体审计clean；Tour、Overflow与日期页头的清理分别见各自审查。原Range生命周期失败临时用例继续单独保留且排除提交，净新增常驻case为0。

| 检查 | 实际结果 |
| --- | --- |
| Mac，10个精确Desktop类 | 198通过／0失败／0跳过，36.717秒 |
| 本地Linux arm64，同10类，只读checkout | 198通过／0失败／0跳过，53.205秒 |
| 当前NET8 Desktop Release | 构建通过，零警告、零错误，31.04秒 |
| 日期页头清理后DateViewer／Calendar／DatePicker | 337通过；详见独立日期审查 |
| Progress真实Mac managed／NativeAOT行为 | scale2，814／749次断言通过；不是最终显示像素对比 |
| 当前Windows选测／其他平台复跑 | 待新提交审查及Actions；不冒用旧CI运行时通过为本批通过 |

只读Linux容器、匿名编译产物卷和完成的运行helper在最终检查后清理。默认视觉修复均已有独立批准；Progress的限定7像素Headless合成重复采样差异仍待用户答复，不能填作验收通过。源码审查没有新增Critical／Important问题，不代替此视觉门槛。当前本包尚未获得Git提交授权，不自动提交或推送。

最终工具检查显式指定Windows清单测试文件的iterate路径，fresh执行通过（iteration-passed，48.2秒，无scope扩大或复用旧receipt）；当前10份触及C#文件成员布局审计问题0，`git diff --check`通过。所有当前批准的默认修复与剩余7像素待决范围均在独立结果页明列。
