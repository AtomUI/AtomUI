# Gallery 版本角标和 Upload 视觉修复

在 `bugfix/shadcn` 使用 macOS 原生 Gallery。原始场景使用 Badge、Segmented、BorderBeam 的版本角标，以及 Upload 的 Default Files、Pictures Wall 和 Pictures with list style。保持原有示例内容、图片、状态、动效与滚动容器。

| 验收项 | 原生脚本结果 |
| --- | --- |
| 示例卡片版本角标 | 三页 × 四种外观全部使用当前主题主色与对应文字色。Shadcn Luma 浅色 `#171717`、深色 `#E5E5E5`，Ant Design 浅色 `#1677FF`、深色 `#1668DC` |
| 角标运行时更新与局部定制 | 保持 Segmented 示例打开，将 Ant Design 主色改为 `#722ED1`，角标随即更新。显式角标色 `#C03523` 生效 |
| Default Files | 三种文件状态的左图标从贴边变为 8 DIP 内距，进度条和文件名对齐 |
| Pictures Wall | Luma 外框 26 DIP，缩略图圆角按内缩计算为 17 DIP。图片尺寸与中央内容像素一致，露在圆角外的角部完成裁剪。Ant Design 按自己的外框与内缩值计算 |
| Pictures with list style | 原布局的行外框和视口左右边界重合，同尺寸 2× 对照未复现整段掉线。修复后四种外观的三类行外框均实测左右各有 2 物理像素边线与 2 物理像素绘制余量。裁剪有效，删除按钮命中，实际滚轮令 Gallery 外层向下滚动 50 DIP |

脚本以工作树构建的程序集启动 AtomUI 窗口，通过窗口编号获取原生截图。已查看四种外观、图片墙前后与图片列表对照。版本角标的同状态复拍与 1 DIP 扰动复位均为零像素差，能够识别扰动。

临时脚本和截图已清理，取证文件审计通过。清理后在 `net10.0` 上运行 Upload 控件 132 项、GalleryBase 的 ShowCaseItem 11 项和 Gallery 的 Badge / Upload 示例 8 项，合计 151 项通过。`git diff --check` 通过。

## Gallery 分隔线、示例外框和 Upload 缩放复查

使用当前工作树的程序集，启动 1300 × 900 DIP 的真实 Gallery 页面。临时驱动通过路由命令打开 Badge、Transfer、Upload，通过窗口编号获取 macOS 2× 原生帧，并以 Avalonia Headless + Skia 对同一 Upload 示例进行整数及分数缩放渲染。

| 验收项 | 脚本和视觉结果 |
| --- | --- |
| Badge 普通示例与带版本角标示例 | 修复前两种卡片的描边厚度均为 0，浅色顶部采样为 `#FFFFFF`。修复后顶部与左右边均为 1 DIP 主题描边。Shadcn Luma 浅、深色依次为 `#E5E5E5`、`#232323`，Ant Design 浅、深色为 `#D9D9D9`、`#424242`。已核对原生前后截图及角标附近圆角。 |
| Transfer 吸顶标签页 | 修复前已提升的标签宿主从物理像素 `(560,80)` 起绘制，盖住同坐标的两条分隔线，红框位置的采样全为白色。修复后宿主起点为 `(562,82)`，横、竖线分别占用 1 DIP，交界连续，且均不参与指针命中。Shadcn Luma 浅、深色与 Ant Design 浅、深色均通过。 |
| Upload 图片列表左右边缘 | Luma、Nova、Vega、Maia、Lyra、Mira、Sera、Rhea 的浅、深色通过 16 组 macOS 原生 2× 场景，以及 80 组 Avalonia Headless + Skia 的 1×、1.25×、1.5×、1.75×、2× 场景。三种状态的两侧到视口均至少留有 1 个物理像素。成功、失败行的左右直边在 1× 及三个分数缩放下均为 1 个物理像素，在 2× 下均为 2 个，圆弧采样无缺段。Skia 诊断帧切换祖先裁剪后，两条实线的直段像素采样无差异。原生和 Skia 均检测到 1 DIP 横向扰动，复位差异为零。 |
| Upload 原交互 | 三种文件状态的删除按钮均能命中。仍使用裁剪视口并禁用横向滚动，原生路由滚轮使 Gallery 外层滚动 50 DIP。复查未发现红色失败行的右侧截断，无需修改 Upload 的渲染或布局。 |

已检查原生和 Skia 截图，临时取证已审计并清理。定向回归并入[本轮清理后的模块验证](2026-11-19-select-focus-tags-switch.md)。
