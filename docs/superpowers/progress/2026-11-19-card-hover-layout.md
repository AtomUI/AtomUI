# Card 悬停布局与外框过渡

`bugfix/shadcn`，macOS 原生 Gallery 的 Card → Customized content。原录屏中，封面底边在指针进出时反复跳动 3 个物理像素。复现使用原示例的 240 DIP 卡片、封面和标题，保留动效和页面滚动结构。

[Shadcn Luma Card](https://ui.shadcn.com/r/styles/base-luma/card.json) 的默认描边环和阴影属于绘制样式，不占布局空间。本地 Ant Design 参照 `/Users/chinboy/Projects/ReferenceProjects/ant-design/components/card/style/index.ts` 在可悬停态过渡边框颜色和阴影。AtomUI 原实现把 `BorderThickness` 由 1 DIP 立即设为 0，封面随之变宽，按比例增高并移动标题。只在原实现上固定边框厚度的对照中，位移消失，原阴影仍能完成过渡。

| 验收项 | 修复前 | 修复后 |
| --- | --- | --- |
| 封面和标题 | 封面宽 238 → 240 DIP，封面底部与标题下移约 1.5 DIP | 封面宽恒为 238 DIP，封面和标题坐标固定 |
| 外框过渡 | 移入时边框先消失，阴影随后出现 | 外边框画笔渐变到卡面背景，阴影同步淡入，约 100 ms 完成 |
| 指针移动 | 移入和移出会改变边框厚度与布局 | 封面、正文、左右边缘及底边的命中稳定，快速进出后恢复原像素 |

脚本以 Avalonia 路由输入驱动原生窗口，通过窗口编号截图。比较覆盖 Shadcn Luma 浅色、深色和 Ant Design 浅色。两版复拍与 1 DIP 扰动复位的卡片区域像素差均为零。已查看前后截图，修复后三种外观的内容采样区域在悬停前后逐像素一致，外框继续显示悬停反馈。

临时驱动和取证帧已清理，工作区取证文件审计及 `git diff --check` 通过。清理后在 `net10.0` 上分别按 `FullyQualifiedName~AtomUI.Desktop.Controls.Tests.Card.` 和 `FullyQualifiedName~AtomUIGallery.Tests.ShowCases.CardShowCasePageTests` 运行专项测试，分别通过 25 项和 1 项。
