# CT15 Avatar / AvatarGroup 工程结果（2026-10-31）

状态：3metadata/11Own当前切片工程与限定验证完成，**完整Avatar/CT15未关闭，仅48对限定视觉已批准**。继续[Avatar计划](avatar-consumers.md)及[Tag依赖](tag-consumers.md)，不重复既定批准家族。137metadata/274Own分母与NeedsDecision8保持；CT12/13/14、CT25/FW1、官方依赖边界不变，无分支/worktree/agent/提交。

## 真实修改

Controls拥有AbstractAvatar/Avatar/Token/Theme，Desktop仅拥有Group。11Own全Active迁为V2R1，四算法44默认/type/key/slot和原文字getter逐条相同。三个TextFontSize名称保持原Icon尺寸角色，默认文字仍原继承14。

Gap、Circle实际半宽、连续Custom宽高/Icon参数进入ControlTheme编译绑定/有限类型化converter；Square owner Global与明确FontFamily/FontSize进入真实Avatar；Group边框用自身Global。图片controller、优先级Image>Text>Icon、generation/lease/borrowed规则未改。Native与声明式root Style保留原合同，PART_TextPresenter保持原生TextBlock和DownOnly Viewbox/完整Text。

Group的成员/fold typed relay按membership持有释放，SizeType先于Size投射，修复初始显式Custom被preset覆盖。真实overflow热Own有效RED为snapshot47但真实32，局部fold Content根复用现有ThemeContextResourceBridge，跟随Context变化且退役时移除/dispose；不改共享Flyout/Scope，不给所有Avatar增加bridge。不引入Local值恢复引擎或新Semantic Part API。

## 有效反证与运行

| 验证 | 修改前 | 修改后已执行 |
| --- | --- | --- |
| 首轮10 | 5FAIL/5PASS：Custom连续尺寸、Native Circle、Global字体、成员退役、旧Group回收 | 10PASS |
| 初始Group.Size72 | 真实child32 | 72；运行中72→96及NaN恢复原Large也通过 |
| Group自身Global BorderThickness3 | 实际1 | 3；Native5/clear回3/配置clear回1 |
| 打开overflow热Own | context与Group相同、snapshot47/spacing19但真实32 | 真实47，清空回32，无重开 |
| 借用图片 | 复用现有管线，不声称新修复前证 | 48→80后仍同一IImage、clip40，清Source回完整Text，重绑Loaded，不dispose调用方图 |
| 资源退出 | 移除child时旧Group未回收 | 保留移除child不保留Group；保留退役fold内容不保留Group/Provider |
| 四算法默认 | 44默认/type/key/slot及文字14 | 各批逐条相同 |
| 最终选择模块 | 18临时+13Group+1Dialog内Group | 32PASS/0Skip，5s；root Style通过 |
| Controls独立Avatar | 既有模块 | 7PASS，348ms；最终清理后重验已执行 |

驱动编译错误（binding元素/身份参数/命名空间/internal上下文入口）如实保留，不计产品RED。初轮浅宿主中暗scope对比不足，整批保存后修正承载面，并在所有版本重走完整相同历史；不是只延迟或重建一侧消差。

## 四算法真实Skia与AOT

环境：macOS26.6.2/build25G83、arm64、.NET SDK10.0.300、官方Avalonia12.1.4；基线HEAD4560091deea4e56c14e89a70e2f418e72a8afd64加当前既有未提交改动。对照仅回放8个Avatar原源及新增converter原不存在状态，finally逐字恢复全部候选；其余未提交文件保留。

900×700/Desktop Window/scale1；四算法内容scope，以配方ColorBgContainer着色承载面；真实Skia与UseRegionDirtyRectClipping，正常motion，真实DispatcherTimer pump。静态采样最少700ms且12连续等原始像素帧，5s上限。主实例按Own三档/0/clear、Global、Native/clear、Custom/Group连续尺寸、Square、RTL/fold、重挂/换模板、图片/完整Unicode长文0→160→恢复执行；独立新进程校准与独立overflow打开/热Own/清空/关闭/退役。

候选managed主76+overflow20+校准4检查均0FAIL。旧源主76/28FAIL、overflow20/4FAIL、校准4/0FAIL，失败均为上述有效缺陷。旧新136主+overflow帧85对精确0、48对明确有意变化见[限定表](avatar-consumers-visual-review.md)，3对closed微差未通过：Light80/max3、Compact53/max12、DarkCompact44/max13。两版各36主业务恢复全0；独立A/A/扰动恢复均0，1DIP变化1607/1645/1362/1401像素、max235/210/235/210。AOT发布成功，5条既有Homebrew库macOS26-v-target12 ld警告，无IL警告，AOT各100项检查完成，跨运行和popup恢复数值另列在文末；未通过微差继续保留。

精确重放入口（会话临时源，结束后删除）：`python3 /private/tmp/atomui-ct15-avatar-u3af0qjf/replay.py candidate`、`candidate-popup`、`old`、`aot`；构建和采样均以`.artifacts/verification/run.lock`的fcntl非阻塞独占锁守卫，冲突即拒绝。旧源动作不随版本前缀变化。媒体不进入仓库，原失败批与最终批分开记录，同坐标完整RGBA，无通用容差或mask/crop/align。

已查看修正前后普通/暗/Compact的Global、Native、真实overflow和完整长文Custom关键帧：真实Circle随宽、Square/边框/字体定制进入原目标、原文字/图像/裁剪/排列均保留。数值全绿不替代视觉查看；3个closed微差仍未通过。完整OS窗口/chrome/输入、Browser、Popup过程时相和未执行自然极值保留。

## 待完成出口

- [x] AOT完整主/overflow/校准、跨运行与恢复数值；非零单列保留。
- [x] root Style补验、5源成员审查/纯移动指纹、作者另步自审更新。
- [x] 18临时用例/3源和媒体/外部驱动清理，最终精确Controls7及Desktop14、NET8、CSV/总表同步。
- [ ] 仅48对有意变化的限定视觉批准，其他非零与完整Avatar/CT15保留。


最终NativeAOT主76/overflow20/校准4均0FAIL，与managed136对中134精确0；Light closed80/max3、Dark closed49/max2不在批准内。三版各36主业务恢复全0。补充popup往返单列：每版open-default→open-restore普通明12/max8、普通暗12/max7非零，两Compact为0；三版default→closed依次为旧[92/max8,61/max7,53/max12,0]、候选[12/max8,61/max7,0,44/max13]、AOT[92/max8,12/max7,0,44/max13]（顺序Light/Dark/Compact/DarkCompact）。这些恢复微差全部未通过，未设新容差；同版本已出现的值不自动豁免。三版各4配方独立A/A与1DIP恢复全0，检错数字与max完全相同。


root Style补验通过：声明式Width/Height64、Circle CornerRadius7、Menlo19/Navy在Custom72→96时仍优先，无取节点赋值回退。最终临时18+既有14所选32PASS/0Skip（5s）；五C#源/两Control成员0问题，AbstractAvatar最终与整理后/纯整理前成员指纹完全相同。最后一批44默认及实际文字getter与原批精确相同。Avatar临时3源19202字节；取证目录1843文件（含前失败批、浅宿主批和最终全部媒体/树值/检查及manifest）。审计clean，清理及最终精确模块正在执行。

清理落实：Avatar Temporary 3源/19202字节已删；外部driver 411文件、整个原会话551文件与current marker已删。候选8原源及新增converter在清理前逐字核对完全相同；不删除其他会话或Tag进行中临时源。原1843取证文件cleanup返回removed=true/audit clean。最终验收在新的短期日志目录执行，读数登记后亦删除。


## 清理后最终限定验收

Avatar的18临时/3源19202字节、1843取证、411文件外部driver及551文件原会话全部清理后，精确Controls Avatar 7PASS/0Skip（412ms）、Desktop AvatarGroup 13与Dialog内Group 1共14PASS/0Skip（1s）。NET8 Release覆盖Desktop及所引用Controls，0 warning/0 error（00:00:24.37）。未自行跑全量。最终短期日志目录4文件在读数写入本报告后删除；无Avatar临时源、会话/媒体或本次进程残留，Tag进行中临时验证单独归属下一包。

本包18临时+既有14+Controls7共39项匹配验证；managed/AOT各100检查。5源/2Control成员0问题，AbstractAvatar纯移动及初始化指纹保持。四正式文档、CSV 3meta/11Own、总表及作者另步自审同步。仅48对有意变化已获限定视觉批准，所有closed/往返微差、真实OS/Browser/时相及完整Avatar/CT15仍未通过或未验证。无提交授权。


限定批准登记：2026-10-31，用户明确回复“批准表内48对，其余未通过与完整CT15保留”（request_user_input_async / call_7b292d7ef454420fb8fc0603ac3e3f4c / item 0）。仅本表48对有意变化获批准；closed/往返微差、OS/Browser/Popup过程时相及完整Avatar/CT15继续保留，不授权提交。
