# Gallery 示例背景与 PopupConfirm 外沿

## 本轮范围

- 逐页核对 Gallery Components 的示例容器、语义样式和代码设置的背景及相应前景。用既有主题资源复用同一套浅深语义，重放实际示例。保留颜色、图片、显式主题和状态外观示例的演示用途。
- Native PopupConfirm 从 Gallery 的 Basic usage、Delete 按钮及 Top placement 重放。比较实际主体、阴影、箭头和宿主边界，针对修正的模块做回归。

## 暗色配色依据

官方默认暗色用 OKLCH 明度 0.145、0.205、0.269 分别表达页面背景、卡片与弹层、弱强调与交互底色。0.922 的默认主色用于高强调操作并搭配深色前景。边框和输入边线使用低透明度的白色。Gallery 的承载面、普通状态和正文按相应角色配对。示例中保留的状态色使用深底与可读的同色系前景。 [shadcn 官方主题规范](https://ui.shadcn.com/docs/theming)

主题令牌直接替换示例的底色和前景声明。须保留既有亮色配方的示例在现有样式资源里按外观提供配色。Card 去掉缓存页面底色的 ViewModel 字段，直接读取主题令牌。NavMenu 的强制深色行在 Shadcn 暗色 Token 算法里使用普通、悬停、选中对应的色对。

## PopupConfirm

Retina 比例为 2。Nova 的描边主体宽 255.5 DIP，阴影框原为 256 DIP。Vega 对应为 267.5 与 268 DIP。截图中描边之外纯白的一个物理像素来自内容页透过间隙。原生 PopupRoot 的独立捕获在该列 alpha 为 0，后一列重新达到 10。

阴影框改用带箭头控件的实际主体排列边界。原 Nova 与 Vega 的透明间隙分别命中 156 与 188 行，修正后均为 0。宿主尺寸、主体尺寸、弹出方向及阴影值的对照一致。右边缘同状态、颜色扰动、恢复的变化像素分别为 0／312／0 和 0／376／0。Popup、Tooltip、DatePicker 定向控件测试 38 项通过。

## Showcase 逐页核对

| 分类 | 已查路由 | 修改示例 |
| --- | ---: | --- |
| Data Display | 23 | Badge、Card、Carousel、Collapse、DataGrid、Descriptions、Empty、Expander、GroupBox、InfoFlyout、List、Segmented、Statistic、Tag、Tour、TreeView |
| Data Entry 与 Feedback | 29 | AutoComplete、Transfer、Upload、Message、Modal、Notification、PopupConfirm、Result |
| General、Layout、Navigation、Other 及入口页 | 27 | Button、FlexPanel、Grid、Masonry、Space、Splitter、Breadcrumb、ButtonSpinner、ComboBox、Menu、Pagination、Steps、TabControl、BorderBeam |

79 个 Gallery 路由在 18 个暗色配方（八种 Shadcn、八种 Shadcn Compact、Ant Design 与其 Compact）下均完成实际路由和可见示例扫描，共 1,422 个路由组合和 8,041 个视口检查。Palette 色卡、显式主题预览、图像、日程色带和高强调操作按其演示或状态角色核对。

Nova 暗色的 Empty 原生复拍显示两块底板从 `#F5F5F5`、`#E6F7FF` 改为 `#262626`、`#111A2C`，正文用 `#FAFAFA`。实测前景对比度从 2.98:1、2.95:1 提升到 14.50:1、16.66:1。两块底板位置与大小一致。同状态、1 DIP 滚动扰动、复位的差分按原区域取样为 0／33890／0。NavMenu 强制暗色例的选中行从 `#E5E5E5` 改为 `#3E4E5D`，普通行和悬停各有独立色对。

末次配色调整对 Breadcrumb、ButtonSpinner、ComboBox、Menu、TabControl 做了 18 × 5 路由定向重放。TabControl 的示例墨条通过生成的 `TabControlIndicatorStyle` 读取主题色，厚度由已有令牌声明。移除了原 Initialized 回调，实际样式测试验证该语义部件生效。

Message、Notification 在实际窗口反馈层打开的成功和错误样式分别从 `#F6FFED`、`#FFF2F0` 改为 `#172819`、`#311C1E`，对应文字 `#A4E493`、`#FF9497`。样式挂载到反馈管理器时，其两套外观资源随样式进入目标作用域。18 个暗色配置通过 72 个弹层检查，2 个浅色对照通过 8 个。

全屏农历日期格是面积约 17,000 DIP² 的选中面。暗色下从选中主色改读控件的选中项令牌对，宿主的实际主题外观通过编译绑定传到日期格。Nova 原生从 `#E5E5E5` 改为 `#444444` 并配 `#FAFAFA`，Ant Design 原生从 `#1668DC` 改为 `#15325B` 并配 `#D8FFFFFF`。18 个暗色配置及两种浅色对照下，紧凑日期格的颜色与尺寸均对照一致。原生截图在对应选中底色上分别计数 67,300 与 65,401 个物理像素。

Gallery 合批构建与最终 Desktop.Controls 增量构建通过。清理临时驱动、媒体与日志后，Gallery 115 项、Desktop 控件 81 项、渲染 30 项、主题 164 项定向测试通过，`git diff --check` 通过。
