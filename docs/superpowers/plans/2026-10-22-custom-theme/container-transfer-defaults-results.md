# CT14 N3-TR：Transfer 默认求值结果

本片按[Transfer迁移计划](container-transfer-defaults.md)实现三个Token的11个V2默认函数。机制及当前Mac资源路径已验证，用户已接受[34对精确限定自然差异](container-transfer-defaults-noise-review.md)，媒体清理及11条Token／3条控件目录回填已完成。本机默认求值机制切片出口完成，不代表完整Transfer家族或其他平台验收。第十七批原始14文件已保存为可校验补丁，本片没有Git授权。

## 变化与证据

ListTransferToken／TreeTransferToken各5项、TransferListViewToken1项；默认只读取有效Global或常量，没有Own→Own边。保留180／250／200、HeaderHeight=Global.ControlHeightLG、HeaderPadding原Global公式及取整、PaginationMargin=(0,Global.MarginXXS)。Own.HeaderHeight不自动改变HeaderPadding；该关系未擅自改成新依赖。没有新公开API、字段、Part、绑定、主题、订阅、缓存、布局或交互。

原Transfer精确模块44/44通过。临时7项探针在原版为3个V2 descriptor合同FAIL、4个明暗兼容PASS；迁移后7/7通过。三个Version缺口是求值机制目标，不是原版消费bug。Global／Control Global、11明确Own值包含0／零Thickness、分页宽度切换、撤回及同实例重挂均保持；明暗22行实际值、类型、identity、key枚举类型／名称／数值、dense slot、资源投影类型完全相等。临时源最初缺少ListItemData命名空间的编译错误单独保留，不计作RED。

三个internal Token为partial并实现IControlTokenDefaults，以声明Revision1接入生成默认函数。运行期EvaluatorRevision为生成摘要，非字面1。源码身份：

| 文件 | 候选SHA256 |
| --- | --- |
| `src/AtomUI.Desktop.Controls/Transfer/ListTransferToken.cs` | `204f4f368a5faa0ab044a5e10fefb754afafbb5d8d339fa5c8959dd4ff8799ef` |
| `src/AtomUI.Desktop.Controls/Transfer/TreeTransferToken.cs` | `c621d5b42179c5a981fe99f962e5ab0991732bdf577549bf9545c6f10a66d286` |
| `src/AtomUI.Desktop.Controls/Transfer/TransferListViewToken.cs` | `fe11427946099299847ed89c0a72bbd6bd044e30de3719bd58bcc484aa57adc6` |

正式四文档纠正不存在的TransferToken／TransferListBox描述，说明三个scope、header/footer内距和分页消费者。Gallery用法未改变。后续已按实际descriptor统一Semantic owner、方向限定与条目边界，并修复原文不存在的专用Style类型及错误SetterTargetType。原文两项检查均FAIL；修正后4项通过，实际编译/加载原文AXAML的8个专用Style/9个关键Setter命中两侧分区和3个条目，无Name/Loaded代码回退。4份临时文件已删除，清理后原44项再次通过；这一文档修正没有改产品API/模板。

## 实际视觉采样

后续API复核确认TreeTransfer没有公开PageSize，owner级IsPaginationEnabled wrapper为internal。下述原84帧及原生脚本的Tree分页阶段属于内部模板分支诊断，不能称为纯公开用法；用户批准的34对仍限这些原历史。原公开默认／Global／Control／Own前缀比较有效，独立[公开API往返补充](container-transfer-public-results.md)已完成60自然帧/80直接绘制；新增14对历史差异于2026-10-26单独获批，140张PNG及manifest共141文件已清理，不能把两次批准扩大到其他历史。TreeTransfer.ListWidthLG的默认机制已核对；后续共享公开属性标识补查以旧/新版明暗16阶段证实既有条件消费，归Active；没有新增树分页API。

实际AtomUI Window，1840×620／scale1／Skia，完整Light/Dark；两个owner Width760／Height320及独立分页视图240×320，保留18项、parent/child树、目标key、选中和原滚动／裁剪。default→Global→Control→Own→分页→选中→reset，同版本main/AA/DIP使用独立实例、两版本同历史，正常motion。最小650ms且覆盖实际最长Transition，再要求三连续同帧，上限4s；没有遮罩、裁剪、对齐或通用容差。

基础Avalonia Window的首轮22帧暗色资源配白色背景，实际查看后判为宿主准备失败，单独保留；正式比较两侧均改用真实产品Window重跑，不把这组当完整暗色通过。

| 正式84帧 | 数量／结果 |
| --- | --- |
| A/A | 28对，24为0，4对4–20像素 |
| 1 DIP | 28对全部检出 |
| 匹配历史新旧 | 42对，38为0，4对4–20像素；全部6默认比较为0 |
| 原实例恢复 | 12对均非零：Light47／Dark39，单通道1／2；旧版候选均出现 |
| 主要资源／几何／公开key状态 | 42对一致 |

独立诊断96帧：同样七阶段历史，四个阶段在自然采样后加两次直接Skia绘制。直接32对A/A、16对恢复、32对新旧均0；自然诊断另14对非零。正式与诊断合计34对非零，最高51像素、单通道4，已逐对登记离散坐标与上限，原始失败保留。不能用直接绘制替换正式合成器失败。

完整诊断每帧739–907个Visual。28组新旧与28组A/A的观测差异仅为两处WaveSpirit残余LastWaveSize/LastWaveOpacity，位置不与非零像素重合；不声称整棵状态完全相同，未证明所有历史合成内部原因。默认至恢复因分页懒创建子树增加，按数组序号直接比较会错配；在非零点对应的7个绘制节点，8组版本／主题／main-AA恢复的几何、字体／画刷、状态和矩阵相同。该局部观测不代替完整家族生命周期／GC验收。

已实际查看正确暗色默认、明色分页、暗色选择、完整差异图及错误基础宿主图；默认分区、文本／选择可读，明确HeaderHeight0使相应header收敛，分页及所选状态与输入一致。用户已接受逐对限定的34项自然差异，原始比较仍保留失败及真实计数，不以数值、看图或批准扩大其他场景范围。

## Mac managed与NativeAOT

两个运行各六个真实NSWindow，client800×500、scale2，分别记录Light／Dark实际主题，三个owner资源/消费者、V2 descriptor与摘要、Own及0、撤回、同实例重挂、分页宽度：各684/684通过。NativeAOT实际dynamicCode=false，托管为true。宿主只引用当前源项目，没有运行时反射扫描／伪造注册或私有PropertyStore旁路；这不是原生最终像素或真实键盘／指针验收。旧版原生未运行，原版默认对照来自实际Headless资源，不能补写原生前证。

原生工程准备时补正AtomUI命名空间与partial Application启动生成合同后，managed构建零警告／错误；AOT发布有本机Homebrew dylib面向macOS26而链接最低12的警告，记录为当前宿主边界，不声称验证macOS12。临时绘制诊断使用旧Save重载的警告只存在已删除临时代码，最终产品构建另核零警告。

## 清理、最终检查与未完成项

- 三份Temporary C#及专用目录已删除，永久测试净新增0；复用原Transfer与生成器合同提供长期保障。
- 外部原生工程422文件、1,248,520,347字节已删除；当前无原生进程残留。
- 清理后原Transfer精确模块44/44、0 Skip，通过；共享属性标识补查清理后以相同filter最终再验44/44、测试3秒。
- Desktop Controls NET8 Release通过，零warning/error，20.77秒。
- 三个完整Token文件成员审计：3文件、0个Avalonia Control、0问题，不能报告成审了3个Control。
- 媒体共202场景PNG及1差异PNG，连同manifest共204文件已通过evidence.py清理，运行目录不存在、Git媒体审计clean，无本次取证二进制进入仓库。
- 11条Token／3条控件目录已回填，保留完整家族仍开放的边界。待审第十七批原始14文件以独立校验补丁固定；本次CSV与总状态变化只是后续工作树差异，不扩大旧Git审查范围。

作者另步自审已核对11个属性的名称／类型／顺序、三份源码身份、原生进程结束与26个文档链接；`git diff --check`通过，第十七批原始14文件blob及补丁对base/index的适用性已核对，index未修改。此为作者审查，不声称独立评审。补充公开往返14对限定已获批准、其141文件清理完成；TreeTransfer.ListWidthLG已核定既有条件消费；实际其他平台与原生最终显示面、完整N2/N3/N4及CT14仍开放。不能沿用第十七批或任何历史Git批准提交本片。
