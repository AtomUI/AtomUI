# CT15 Spin 循环限定视觉审查（工程/清理/最终门禁完成，308对已获限定批准）

仅下表308对独立的初始/循环/延时到期与重新可见变化。表按“原整帧RGBA相同且候选整帧RGBA相同”合并等价行，每行列明全部算法/场景键及实际对数；原新同坐标完整帧比较，不通过裁剪/遮罩/对齐/容差合并。另268个second为同时间相同图像复核，不新增产品状态。

场景：M0内置四点、M1原生27×31 Orange glyph、M2同型仅模板glyph；D0默认1200ms、D1原生500ms、D2 Own2000ms、D3原生0（Compositor沿原1ms下限）；composite为原Spin+Tip/输入/100高视口900extent。p为旋转周期P倍数，透明度稳态2P；delay1/2-cycle为两点原延时到期后的下一完整透明度循环。ready是在冻结phase0下等待原生内容真实过渡完成的就绪图，不能据此说正常循环静止。

| 对数 | 算法:完整场景键 | 非零像素 | max | bbox |
| ---: | --- | ---: | ---: | --- |
| 24 | L:M0D0-p0-first；L:M0D0-visible；L:M0D1-p0-first；L:M0D1-visible；L:M0D2-p0-first；L:M0D2-visible；L:M0D3-p0-first；L:M0D3-visible；L:M0D0-ready；L:M0D1-ready；L:M0D2-ready；L:M0D3-ready；C:M0D0-p0-first；C:M0D0-visible；C:M0D1-p0-first；C:M0D1-visible；C:M0D2-p0-first；C:M0D2-visible；C:M0D3-p0-first；C:M0D3-visible；C:M0D0-ready；C:M0D1-ready；C:M0D2-ready；C:M0D3-ready | 145 | 69 | [54, 27, 67, 40] |
| 8 | L:M0D0-p1-first；L:M0D1-p1-first；L:M0D2-p1-first；L:M0D3-p1-first；C:M0D0-p1-first；C:M0D1-p1-first；C:M0D2-p1-first；C:M0D3-p1-first | 580 | 164 | [0, 0, 67, 67] |
| 4 | L:M0D0-p2-first；L:M0D0-p4-first；C:M0D0-p2-first；C:M0D0-p4-first | 580 | 122 | [0, 0, 67, 67] |
| 8 | L:M0D0-p3-first；L:M0D1-p3-first；L:M0D2-p3-first；L:M0D3-p3-first；C:M0D0-p3-first；C:M0D1-p3-first；C:M0D2-p3-first；C:M0D3-p3-first | 580 | 164 | [0, 0, 67, 67] |
| 2 | L:M0D0-delay1-cycle-first；C:M0D0-delay1-cycle-first | 148 | 6 | [3, 13, 17, 27] |
| 2 | L:M0D0-delay2-cycle-first；C:M0D0-delay2-cycle-first | 152 | 55 | [40, 50, 54, 64] |
| 12 | L:M0D1-p2-first；L:M0D1-p4-first；L:M0D2-p2-first；L:M0D2-p4-first；L:M0D3-p2-first；L:M0D3-p4-first；C:M0D1-p2-first；C:M0D1-p4-first；C:M0D2-p2-first；C:M0D2-p4-first；C:M0D3-p2-first；C:M0D3-p4-first | 580 | 122 | [0, 0, 67, 67] |
| 4 | L:M0D1-delay1-cycle-first；L:M0D2-delay1-cycle-first；C:M0D1-delay1-cycle-first；C:M0D2-delay1-cycle-first | 148 | 6 | [3, 13, 17, 27] |
| 4 | L:M0D1-delay2-cycle-first；L:M0D2-delay2-cycle-first；C:M0D1-delay2-cycle-first；C:M0D2-delay2-cycle-first | 152 | 55 | [40, 50, 54, 64] |
| 2 | L:M0D3-delay1-cycle-first；C:M0D3-delay1-cycle-first | 148 | 6 | [3, 13, 17, 27] |
| 2 | L:M0D3-delay2-cycle-first；C:M0D3-delay2-cycle-first | 151 | 54 | [40, 50, 54, 64] |
| 64 | L:M1D0-p1-first；L:M1D0-p2-first；L:M1D0-p3-first；L:M1D0-p4-first；L:M1D1-p1-first；L:M1D1-p2-first；L:M1D1-p3-first；L:M1D1-p4-first；L:M1D2-p1-first；L:M1D2-p2-first；L:M1D2-p3-first；L:M1D2-p4-first；L:M1D3-p1-first；L:M1D3-p2-first；L:M1D3-p3-first；L:M1D3-p4-first；L:M2D0-p1-first；L:M2D0-p2-first；L:M2D0-p3-first；L:M2D0-p4-first；L:M2D1-p1-first；L:M2D1-p2-first；L:M2D1-p3-first；L:M2D1-p4-first；L:M2D2-p1-first；L:M2D2-p2-first；L:M2D2-p3-first；L:M2D2-p4-first；L:M2D3-p1-first；L:M2D3-p2-first；L:M2D3-p3-first；L:M2D3-p4-first；C:M1D0-p1-first；C:M1D0-p2-first；C:M1D0-p3-first；C:M1D0-p4-first；C:M1D1-p1-first；C:M1D1-p2-first；C:M1D1-p3-first；C:M1D1-p4-first；C:M1D2-p1-first；C:M1D2-p2-first；C:M1D2-p3-first；C:M1D2-p4-first；C:M1D3-p1-first；C:M1D3-p2-first；C:M1D3-p3-first；C:M1D3-p4-first；C:M2D0-p1-first；C:M2D0-p2-first；C:M2D0-p3-first；C:M2D0-p4-first；C:M2D1-p1-first；C:M2D1-p2-first；C:M2D1-p3-first；C:M2D1-p4-first；C:M2D2-p1-first；C:M2D2-p2-first；C:M2D2-p3-first；C:M2D2-p4-first；C:M2D3-p1-first；C:M2D3-p2-first；C:M2D3-p3-first；C:M2D3-p4-first | 391 | 255 | [20, 20, 47, 47] |
| 2 | L:composite-p0-first；L:composite-ready | 47 | 69 | [255, 139, 262, 146] |
| 1 | L:composite-p1-first | 202 | 233 | [240, 131, 262, 153] |
| 2 | L:composite-p2-first；L:composite-p4-first | 202 | 233 | [240, 131, 262, 153] |
| 1 | L:composite-p3-first | 202 | 233 | [240, 131, 262, 153] |
| 1 | L:composite-delay1-cycle-first | 48 | 6 | [240, 135, 248, 143] |
| 1 | L:composite-delay2-cycle-first | 52 | 55 | [251, 144, 259, 152] |
| 1 | L:composite-visible | 47 | 69 | [255, 139, 262, 146] |
| 24 | D:M0D0-p0-first；D:M0D0-visible；D:M0D1-p0-first；D:M0D1-visible；D:M0D2-p0-first；D:M0D2-visible；D:M0D3-p0-first；D:M0D3-visible；D:M0D0-ready；D:M0D1-ready；D:M0D2-ready；D:M0D3-ready；DC:M0D0-p0-first；DC:M0D0-visible；DC:M0D1-p0-first；DC:M0D1-visible；DC:M0D2-p0-first；DC:M0D2-visible；DC:M0D3-p0-first；DC:M0D3-visible；DC:M0D0-ready；DC:M0D1-ready；DC:M0D2-ready；DC:M0D3-ready | 145 | 60 | [54, 27, 67, 40] |
| 8 | D:M0D0-p1-first；D:M0D1-p1-first；D:M0D2-p1-first；D:M0D3-p1-first；DC:M0D0-p1-first；DC:M0D1-p1-first；DC:M0D2-p1-first；DC:M0D3-p1-first | 580 | 140 | [0, 0, 67, 67] |
| 4 | D:M0D0-p2-first；D:M0D0-p4-first；DC:M0D0-p2-first；DC:M0D0-p4-first | 580 | 106 | [0, 0, 67, 67] |
| 8 | D:M0D0-p3-first；D:M0D1-p3-first；D:M0D2-p3-first；D:M0D3-p3-first；DC:M0D0-p3-first；DC:M0D1-p3-first；DC:M0D2-p3-first；DC:M0D3-p3-first | 580 | 140 | [0, 0, 67, 67] |
| 2 | D:M0D0-delay1-cycle-first；DC:M0D0-delay1-cycle-first | 148 | 5 | [3, 13, 17, 27] |
| 2 | D:M0D0-delay2-cycle-first；DC:M0D0-delay2-cycle-first | 152 | 46 | [40, 50, 54, 64] |
| 12 | D:M0D1-p2-first；D:M0D1-p4-first；D:M0D2-p2-first；D:M0D2-p4-first；D:M0D3-p2-first；D:M0D3-p4-first；DC:M0D1-p2-first；DC:M0D1-p4-first；DC:M0D2-p2-first；DC:M0D2-p4-first；DC:M0D3-p2-first；DC:M0D3-p4-first | 580 | 106 | [0, 0, 67, 67] |
| 4 | D:M0D1-delay1-cycle-first；D:M0D2-delay1-cycle-first；DC:M0D1-delay1-cycle-first；DC:M0D2-delay1-cycle-first | 148 | 5 | [3, 13, 17, 27] |
| 4 | D:M0D1-delay2-cycle-first；D:M0D2-delay2-cycle-first；DC:M0D1-delay2-cycle-first；DC:M0D2-delay2-cycle-first | 152 | 46 | [40, 50, 54, 64] |
| 2 | D:M0D3-delay1-cycle-first；DC:M0D3-delay1-cycle-first | 148 | 5 | [3, 13, 17, 27] |
| 2 | D:M0D3-delay2-cycle-first；DC:M0D3-delay2-cycle-first | 151 | 45 | [40, 50, 54, 64] |
| 64 | D:M1D0-p1-first；D:M1D0-p2-first；D:M1D0-p3-first；D:M1D0-p4-first；D:M1D1-p1-first；D:M1D1-p2-first；D:M1D1-p3-first；D:M1D1-p4-first；D:M1D2-p1-first；D:M1D2-p2-first；D:M1D2-p3-first；D:M1D2-p4-first；D:M1D3-p1-first；D:M1D3-p2-first；D:M1D3-p3-first；D:M1D3-p4-first；D:M2D0-p1-first；D:M2D0-p2-first；D:M2D0-p3-first；D:M2D0-p4-first；D:M2D1-p1-first；D:M2D1-p2-first；D:M2D1-p3-first；D:M2D1-p4-first；D:M2D2-p1-first；D:M2D2-p2-first；D:M2D2-p3-first；D:M2D2-p4-first；D:M2D3-p1-first；D:M2D3-p2-first；D:M2D3-p3-first；D:M2D3-p4-first；DC:M1D0-p1-first；DC:M1D0-p2-first；DC:M1D0-p3-first；DC:M1D0-p4-first；DC:M1D1-p1-first；DC:M1D1-p2-first；DC:M1D1-p3-first；DC:M1D1-p4-first；DC:M1D2-p1-first；DC:M1D2-p2-first；DC:M1D2-p3-first；DC:M1D2-p4-first；DC:M1D3-p1-first；DC:M1D3-p2-first；DC:M1D3-p3-first；DC:M1D3-p4-first；DC:M2D0-p1-first；DC:M2D0-p2-first；DC:M2D0-p3-first；DC:M2D0-p4-first；DC:M2D1-p1-first；DC:M2D1-p2-first；DC:M2D1-p3-first；DC:M2D1-p4-first；DC:M2D2-p1-first；DC:M2D2-p2-first；DC:M2D2-p3-first；DC:M2D2-p4-first；DC:M2D3-p1-first；DC:M2D3-p2-first；DC:M2D3-p3-first；DC:M2D3-p4-first | 391 | 235 | [20, 20, 47, 47] |
| 2 | D:composite-p0-first；D:composite-ready | 47 | 60 | [255, 139, 262, 146] |
| 1 | D:composite-p1-first | 202 | 200 | [240, 131, 262, 153] |
| 2 | D:composite-p2-first；D:composite-p4-first | 202 | 200 | [240, 131, 262, 153] |
| 1 | D:composite-p3-first | 202 | 200 | [240, 131, 262, 153] |
| 1 | D:composite-delay1-cycle-first | 48 | 5 | [240, 135, 248, 143] |
| 1 | D:composite-delay2-cycle-first | 52 | 46 | [251, 144, 259, 152] |
| 1 | D:composite-visible | 47 | 60 | [255, 139, 262, 146] |
| 3 | C:composite-p0-first；C:composite-visible；C:composite-ready | 36 | 69 | [254, 139, 260, 145] |
| 1 | C:composite-p1-first | 156 | 190 | [240, 132, 260, 152] |
| 2 | C:composite-p2-first；C:composite-p4-first | 156 | 177 | [240, 132, 260, 152] |
| 1 | C:composite-p3-first | 156 | 190 | [240, 132, 260, 152] |
| 1 | C:composite-delay1-cycle-first | 38 | 6 | [240, 135, 247, 142] |
| 1 | C:composite-delay2-cycle-first | 40 | 55 | [250, 144, 257, 151] |
| 2 | DC:composite-p0-first；DC:composite-ready | 36 | 60 | [254, 139, 260, 145] |
| 1 | DC:composite-p1-first | 156 | 163 | [240, 132, 260, 152] |
| 2 | DC:composite-p2-first；DC:composite-p4-first | 156 | 153 | [240, 132, 260, 152] |
| 1 | DC:composite-p3-first | 156 | 163 | [240, 132, 260, 152] |
| 1 | DC:composite-delay1-cycle-first | 38 | 5 | [240, 135, 247, 142] |
| 1 | DC:composite-delay2-cycle-first | 40 | 46 | [250, 144, 257, 151] |
| 1 | DC:composite-visible | 36 | 60 | [254, 139, 260, 145] |

有意修正：旋转整周期从上一0.75P角度回到0；四点初值全部回到声明0.3，各点在完整Alternate周期和原index×P/3错峰边界恢复0.3。原Base/自定义glyph/模板、67/13实际尺寸、原生27×31/Orange优先、曲线/时间与Root样式保持。首key的最小正规正位置低于一切可表示非零TimeSpan-tick进度，非零正常曲线完整帧未变，未关闭动效或改变像素容差。

最终三版各2372帧：2184双帧受控相位+52就绪+104显隐+16真实clock+16校准。候选/AOT各3004检查PASS，旧各组751中95失败（四算法共380），全部完整无缺帧。2356受控/就绪/显隐/校准cross全0；16正常时相cross非零保留。旧新1780零/592非零，其中576有意=308独立+268同帧重复，另16正常时相非零保留。A/A和1DIP复位各4零，扰动各3010/2987/3022/2999 max255/199/255/199检出。

原12FAIL、次正规模型7PASS/1FAIL及首1160就绪不足帧全部保留账本。原46静态与P3-C96对批准不扩大；平台/GC/其余恢复/极值与完整Spin/CT15仍保留。当前尚待临时清理与清理后精确模块/NET8，不提前请求批准，不授权提交。


已查看四算法完整图面与首批TextBox差异区域，初值/端点/延时与组合原内容分开判断。28临时1源10258字节已删除；9375取证文件（原12FAIL/两个模型/候选临时/1160就绪不足帧/最终7116主帧/四拼图/TSV/manifest）经官方audit/cleanup删除，findings=[]、removed=true。外部driver412文件/publish15文件、原469会话共1082498989字节已删除；源码1源冻结逐字一致。清理后精确模块/NET8在独立四文件会话执行，不保留驱动、时钟访问器或媒体。


清理后真实重建精确Spin：SpinSemanticPartTests/SpinIndicatorCustomSizeTests/RecurringVisualWorkTests.Spin_共16PASS/0FAIL/0Skip、581ms（命令31.29s）；Desktop NET8 Release覆盖Common零警告零错误21.04s（命令21.27s）。均先取得共享输出非阻塞独占锁，最终四文件门禁会话清理。成员1源/1控件0、正式文档相对链接无断链、git diff --check通过；无全量/提交/新分支/worktree/子agent。仅308对独立起点/端点变化待限定批准，268个second为相同图像复核，16正常clock非零/原FAIL/其他恢复/平台/GC/完整Spin与CT15保留。


限定批准登记：2026-11-01，用户明确回复“批准表内308对，其余边界保留”（request_user_input_async / call_122c5e1eef6b4d7ca6e0c98bd9c0c55b / item 0）。仅Spin循环表内308独立起点/端点/错峰/重新可见变化获批准，51行整帧等价合并及268同帧second复核不扩大产品状态。16真实clock非零、原12FAIL/模型及就绪坏批、其他恢复/平台/GC与完整CT15保留；原46静态/P3-C96批准不扩大，不授权提交。
