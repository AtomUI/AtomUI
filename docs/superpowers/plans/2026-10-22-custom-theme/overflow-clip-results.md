# CT-25 OverflowTip 祖先裁剪审查

## 原因与行为

沿既有420×320窗口、90宽居中裁剪Panel、420宽ContentPresenter及原文字。实际文字宽320.876953125，大于可见90而小于owner420；旧版只比较owner宽度，漏掉提示。候选只读自身及视觉祖先Bounds、ClipToBounds、Clip.Bounds和坐标变换，逐级求可见文本矩形；不改变窗口、裁剪、文本、事件路由或提示placement。

实际指针分别在可见文字的左／中／右命中，确认真实视觉hit及提示打开。裁剪开关／宽度、可变Geometry、可变Transform／替换、重挂新父及旧父仍活着时的owner释放均验证。统一20项（15既有＋5临时）旧版14通过／6失败，候选20通过；最后一轮40.332／48.674秒。原有裁剪用例扩展同owner90→400→90，保持presenter Bounds不变，无新增常驻case。

祖先观察只在attach时建立，detach清空，禁用释放；替换Clip／Transform时先释放旧观察再建立新观察。一般Geometry使用Bounds，不宣称任意曲线路径逐字判断。

## 视觉与宿主

明暗实际ThemeManager，正常motion，原420×320／90／420几何，公开ShowDelay=0以直接驱动hover。每版独立main、A/A、1 DIP平移三个实例，先无指针再真实MouseMove；捕获前有界等待实际compositor提交。24帧均为完整不透明RGBA，无容差、遮罩或对齐。

8组A/A全部0，8组1 DIP全部检出631至5,400像素。无指针时明暗旧新均0；hover时旧版无提示，候选完整显示原文字，明色22,444像素变化，边界(136,0)至(420,82)，暗色17,527像素，边界(148,0)至(417,70)，仅新增提示框、文字、箭头和阴影。已实际查看旧版无指针、候选明暗hover及全图计数。Headless无原生IPopupImpl，使用编译VisualLayerManager模板提供实际Overlay，不替代原生最终显示面。

临时驱动曾错误选择stretch文本块整行高度中点，修正为实际TextLayout高度后命中文本；随后补齐测试宿主所需Overlay层。首轮视觉宿主未把Window背景投射到内容层，原文字不可见，6组1 DIP未检出，该轮作废并保留失败数值；补齐宿主的Background／Foreground模板绑定后，两版完整轨迹重录，24帧alpha均255，正对照全部通过。产品几何及代码没有因此改变。

用户已明确允许默认祖先裁剪修复，此批准不包含Git提交。4份临时源14,965字节、4份源备份／辅助脚本及两轮54份媒体已清理，媒体审计均无残留。清理后最终10个精确Desktop类在Mac与本地Linux arm64各198项全部通过，包含本控件15项既有用例；NET8 Desktop Release零警告、零错误。独立只读审查未发现本批新增Critical／Important问题；完整Tooltip家族及原生最终显示面仍未验收。
