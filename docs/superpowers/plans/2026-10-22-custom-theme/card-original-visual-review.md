# CT15 Card 原恢复历史限定调查归属表（已获有限调查归属批准）

本表仅准备申请40条具体路径交CT25/FW2调查：原完整历史8恢复阶段×4算法=32、属性不变时Frame重录4条、对应独立图元重录4条。仅表列40路径已获有限调查归属批准，原话与请求ID见末尾；真实AOT、清理与最终Card门禁均已完成，现仅申请下述有限裁决，不提前改变归属。非零仍严格FAIL，不增容差、不改官方依赖/FW1，不关闭Card或完整CT15；原32对视觉与2默认纠正批准不扩大。

| 配方 | 原8恢复阶段各自像素/max | 属性不变的真实Frame重录 | 无AtomUI图元重录 | 随后完整背景重绘 |
| --- | ---: | ---: | ---: | ---: |
| Default | 312/2 | 312/2 | 312/2 | 0 |
| Compact | 312/2 | 312/2 | 312/2 | 0 |
| Dark | 110/1 | 110/1 | 110/1 | 0 |
| Dark+Compact | 110/1 | 110/1 | 110/1 | 0 |

原8恢复阶段逐项为local-restore、padding-restore、own-restore、reserved-restore、global-restore、parts-restore、variant-restore、rtl-restore。表中第一列每一个阶段均单独统计，不因画面相同扩大为更多状态；对应真正记录在[原历史核对](card-original-restore-history.md)。A/A和原整层1DIP恢复全0。

保留1260×1380、12张380×300、三尺寸×普通/Meta/Grid/Tabs、两原动作、标题/内容/cover和完整22静态阶段。原主批900性质检查通过不等于像素通过。只读副场景不改属性，先完整背景重绘0，再Frame.InvalidateVisual精确复现312/110，再背景重绘0；InvalidateVisual仅用于因果诊断，不进入产品。

独立工程只引用官方Avalonia.Headless/Skia12.1.4并断言无AtomUI运行程序集。原Frame几何/颜色/描边/四角全部只读导出；普通简单圆角与Grid三卡Top8/Bottom0复杂分支明确保留。临时复制确定的本地几何生成算法并以官方DrawingContext/StreamGeometry API绘制，未引用或替换产品依赖；模型不含业务内容，不声称完整默认图片相同。比较的是完整窗口前后差分，其像素集合及每个有符号通道差与真实Card四组全部相同，无裁剪/缩放/对齐/遮罩。

纯模型managed/AOT36帧cross全部0，严格同值Frame重录4FAIL，比较命令返回2；完整背景重绘4组0。基础只用RoundedRect API的模型也独立产生357/357/120/120，同类但不与原312/110混报。

排除项：原旧新global-restore4630/4389/4589/4347等完整对比、其他52重复阶段/原失败、持续loading跨时相、平台/所有极值/完整家族均保留。本表只申请调查归属，不批准非零正确或通用绘制容差。清理后Card20PASS/0Skip/4.7581s，复用相同最终源码NET8零警告零错误17.41s；910取证audit clean后删除，790会话2173296792字节及marker已删除，无仓库临时测试/生产变更。最终helper记录后清理。


当前managed/AOT原主批各900性质检查、88cross全0；真实重录副批各88性质/36cross全0；独立图元36cross全0，合计160完整可比帧均0。真实AOT46410656字节SHA256 1d3a232a3aa7fbc8e4b717408d8188edf0f3848899f1f511311fe9cfebff0a81，纯AOT7425624字节SHA256 418ee783600e90a74acd4dabb0f3f332e159a48c18070d5cab8d12920d2ed012；均零IL/CS warning、各五既有Homebrew linker warning。


## Card 40路径限定调查批准登记

用户明确回复“仅批准表列40路径归CT25/FW2调查，其余保留”（request_user_input_async / call_c8333768d41d4ba692fea52d54ee7dea / item 0）。仅card-original-visual-review.md表列原32恢复、4同值Frame重录及4独立图元路径交CT25/FW2继续调查；非零仍FAIL，不增容差、不打产品重绘补丁、不改FW1/官方依赖。原旧新Global非零、其他边界与完整CT15保留，不授权提交。
