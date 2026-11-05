# CT13 NU2-A：NumericUpDown 内嵌加减图标 owner

承接已授权输入消费者任务，主agent/现有目录，无新分支、worktree或Git操作。只在NumericUpDownSpinnerTheme.axaml核定四处Shared.IconSize，保持原14默认；上下箭头继续ButtonSpinnerOwn.HandleIconSize（默认7），不机械合并两种图形职责。

图标宽高是NumericUpDown模板显式赋予内部加减按钮的组合几何；其颜色仍属于公开IconButton的既有identity。OwnerTokenResource会先命中目标自身公开IconButton，不能仅因名字方便就把它用作Numeric父几何入口；若RED证明断链，使用已有NumericUpDownTokenResource明确父维度。

- [x] NU2A-R：真实NumericUpDown Spinner、Numeric.IconSize=22、IconButton.IconSize=41/IconSizeSM=35、ColorIcon独立颜色；证明实际宽高及可见Icon尺寸未达22，公开子按钮颜色与独立IconButton作正对照。Input上下箭头7保留。
- [x] NU2A-B：原明暗四尺寸/两种模式，原数据/高度/方形按钮/边框保持；原默认、scope配置、0/清除、真实点击增减/键盘；AA/1DIP/恢复，记录完整轨迹与实际变换，媒体在仓库外。
- [x] NU2A-I：仅四个IconWidth/Height入口改Numeric有效Global，实际消费者/0/热更新/identity隔离和原布局验证。没有新Own字段、子控件identity、图标颜色重写或代码绑定。
- [x] NU2A-V：本机managed/AOT与NET8、精确新旧像素/几何；新Glyph尺寸变化单独审查。两个硬编码分隔边的LineWidth归后续几何包，不混入本次简单尺寸入口。
- [x] NU2A-C：临时源/工程/媒体清理，清理后精确NumericUpDown及实际影响模块、正式文档/结果、作者另步自审。完整家族/CT25仍开放。
