# Pagination 输入接线与限定视觉审查结果

用户已批准[两项接线及两项兼容保留短设计](pagination-input-wiring-design.md)，并于2026-10-26明确回复“允许修复并接受十对表内限定差异”。本次视觉批准仅覆盖下述修复与N01–N10，不含Git、其他Host或完整家族验收。基线93ef4ba8d；当前产品仅改PaginationToken.cs、QuickJumperBarTheme.axaml、SimplePaginationTheme.axaml。14项V2默认及两处小号宽度／简洁输入背景接线分别取证，不能混为原消费全有问题。

## 有效验证与实际边界

- 原Pagination57项通过；两个原版明暗反例共六处未更新，资源已为Mini90／Purple，实际Width／MinWidth仍50、绘制帧仍原色；同节点撤回稳定。接线后两项均通过：默认44、Own90生效、背景Purple生效并恢复。原始反例不包括初期观测目标和序列化准备错误。
- V2声明合同原版1 FAIL、2兼容PASS；迁移后三项通过。明暗28默认及168条资源／类型／identity／key／slot记录新旧相同，没有Own→Own边。
- 保护检查明暗各一项通过：Large／Middle／Custom读取常规75、Small读取Mini90；disabled背景、只读隐藏、透明色、0、撤回、同输入引用、焦点／草稿／光标保持；真实框架KeyPress／KeyRelease触发Enter后页码为9。
- 外层ControlTheme跨两次/template/曾导致真实加载失败，已修正为QuickJumpEdit实例内AXAML Styles的一跳选择器；未改LineEdit隐式主题、StyleKey或加入代码绑定，不以准备失败作为功能RED。
- Mac真实Light／Dark NSWindow、scale2：managed206／NativeAOT207项通过，AOT动态代码禁用；140条资源记录完全相同，含四尺寸、disabled／只读、透明／0、重挂与恢复。实际原生最终像素／原生输入未由此证明。
- 临时四份C#已删除，常驻净增0；两个原生进程结束，419文件／1,380,865,854字节外部工程已清理。AOT本地dylib面向macOS26的链接警告保留，不声称macOS12已验。

## 正式自然帧

Mac arm64 Headless／Skia、scale1、1200×420，真实应用主题事务和产品Window，正常motion保留；四行分别为普通Middle、普通Small、Simple可编辑Middle、Simple可编辑Small。公开数据Total500／CurrentPage6、默认PageSize10，不重建A→B→A主实例；main／A/A／1DIP是独立Window，DIP只将外部容器左距增加1。

两版保留相同完整历史：default→Global(ControlHeight36／SM20／LG48、MarginXS13／XXS8)→Control Global(LG64／MarginXS15)→14项Own(常规宽75／Mini90／输入Blue等)→disabled→reenable和reset。每阶段等待至少650ms及实际最长Transition、连续三同帧，不关闭动效，不缩放／对齐／裁剪／遮罩或设通用容差。

纯机制72帧：24A/A均0、24组1DIP检出、36组新旧均0；12恢复中6为0，其余6是下表单像素。最终接线36帧：12A/A中11为0、12组1DIP检出；6恢复中3为0，其余3为同一单像素。历史4像素A/A单列下表，原始比较失败及计数全部保留。

## 已批准的预期视觉变化

以下为main完整整图新旧变化像素；全部变化落在实际分页行内，无行外变化。普通Middle行全部为0；默认／Global／Control／disabled／reset时Simple Middle行也为0。Own仅额外改变两个Simple启用输入的背景。Small宽度与其后标签／导航位置变化属于已批准设计的布局结果。

| 阶段 | Light | Dark |
| --- | ---: | ---: |
| default | 772 | 778 |
| Global | 763 | 763 |
| Control Global | 793 | 793 |
| Own | 4743 | 4750 |
| disabled | 1385 | 1390 |
| reset | 772 | 778 |

已实际查看原机制明色默认／暗色disabled、最终明色默认、暗色Own及默认完整差异图。默认页码／箭头／标签保持，小号输入宽44；明确Blue背景只作用于Simple启用输入，disabled仍用原背景。Own测试中Small条目高度18、饱和色为输入保留探针，不作为推荐密度或配色，也未借此修复其他布局。

## 十对限定自然差异（已批准）

仅适用上述Host、源版本及各自完整历史；整图物理像素左上为原点。仅接受逐对所列点与RGBA，不以包围框扩大范围，不扩展到其他配置／平台／原生显示面。

| ID | before → after | 离散点／数量 | 精确RGBA变化／最大单通道 |
| --- | --- | --- | --- |
| N01 | old-mechanism-dark-main-default → reset | (65,174)，1 | (20,20,20,255)→(19,19,19,255)，1 |
| N02 | old-mechanism-dark-aa-default → reset | (65,174)，1 | 同N01 |
| N03 | old-mechanism-dark-dip-default → reset | (66,174)，1 | 同N01 |
| N04 | v2-mechanism-dark-main-default → reset | (65,174)，1 | 同N01 |
| N05 | v2-mechanism-dark-aa-default → reset | (65,174)，1 | 同N01 |
| N06 | v2-mechanism-dark-dip-default → reset | (66,174)，1 | 同N01 |
| N07 | wired-dark-main-default → reset | (65,174)，1 | 同N01 |
| N08 | wired-dark-aa-default → reset | (65,174)，1 | 同N01 |
| N09 | wired-dark-dip-default → reset | (66,174)，1 | 同N01 |
| N10 | wired-light-main-disabled → wired-light-aa-disabled | (95,135)、(114,135)、(95,142)、(114,142)，4 | 每点(199,143,199,255)→(215,175,215,255)，32 |

暗色单像素在原始及接线版同一Simple Middle输入圆角，独立纯V2／接线诊断各366节点的公开背景／描边／圆角／厚度／几何／矩阵／裁剪等输入恢复完全相同；两组自然诊断仍各1像素，两个直接Skia恢复均0。明色四点在Small条目边缘；独立诊断的6自然A/A、6直接Skia A/A均0，366节点公开输入含字体／前景一致，未稳定重现历史四点。

诊断另插入直接绘制，属于独立历史，不改写正式失败，也尚未证明每个历史帧的内部缓存原因。以上限定不是通用容差。批准后158张PNG及manifest共159文件已清理，媒体审计clean；原始比较失败及真实计数保留于文本证据。JSON清单的N07–N10编号曾按采样顺序排列，登记时已对齐本表；十对文件、坐标与数值集合完全不变，两个原始比较JSON的SHA保持，未扩大批准范围。

## 清理后检查与未完成项

临时源码／原生工程清理后，`dotnet test tests/AtomUI.Desktop.Controls.Tests/AtomUI.Desktop.Controls.Tests.csproj -c Release --framework net10.0 --no-restore --filter FullyQualifiedName~Pagination`重新构建并通过原57项、0 Skip、测试1秒。Desktop NET8 Release构建37.10秒、零警告／错误；完整PaginationToken文件成员审计1文件／0Control／0问题。CSV仅回填14Token／2Control，其中ItemLinkBg／InputOutlineOffset兼容保留。批准登记及媒体删除后产品三文件SHA保持，沿用相同输入的上述聚焦证据，不重复执行没有新变更的模块。

本机默认机制及两项输入接线切片已完成上述验证、限定视觉审查与清理。完整家族、其余输入／状态／Global、其他Host、原生最终像素及Git提交不由此批准。作者另步审查独立记录，不声称外部评审；具体提交范围另交用户审查。
