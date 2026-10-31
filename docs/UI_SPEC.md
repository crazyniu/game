# 小鸟归巢：UI单值规范（v1.3整理版）

2026-10-31。**本次只整理文档，未重画UI、修改代码或做本轮视觉验收。** 本页拥有静态尺寸/位置/外观/缩放/触控目标；动画时间与输入语义唯一归[INTERACTION](spec/INTERACTION_ANIMATION.md)，可复制任务模板见[ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md)。旧叠加版本已[归档](archive/UI_SPEC_HISTORY.md)，不再与本页并列。

依据：main与PR原UI_SPEC blob均为`c42308e5fa3d6ed29816c2c79136186371268e34`，完整读取v1.3（含末条0.72遮罩修正）；补空白时只读现代码。标签：**既定**=原规范最终条款；**代码事实**=源码中当前值，非运行验收；**DESIGNED**=新增验收目标，未实测。不得以“整理”为由改变已验收切图或让旧v1/v1.1值复活。

## 1. 单位与坐标（UI-U）

- UI-U01：参考设计画布1080×1920；全部布局数字单位为**design px**。中心原点，X向右、Y向上；Ø表示直径。它不是PNG源像素、设备物理像素、iOS pt或Android dp。
- UI-U02：容器/图标框与实际PNG非透明范围分开。完整鸟显示框为136×136（含透明区域），不是可见鸟身宽。
- UI-U03：未写“局部”的位置相对安全区设计根中心；局部坐标必须叠加表中父容器坐标。禁止拿嵌套局部坐标当屏幕坐标。

## 2. 单值尺寸与位置（UI-D）

本表是尺寸唯一入口；其他文件/提示词引用UI-D键，不另填竞争数值。除独立命中目标外均是视觉Rect。

| 键/元素 | 尺寸、中心、资源/限制 | 状态/出处 |
| --- | --- | --- |
| UI-D01 菜单树冠 | 900×750，(0,80) | 代码事实：BuildMenu |
| UI-D02 菜单Logo | 860×358，(0,420) | 代码事实 |
| UI-D03 Play | Ø260绿，(0,-330)，icon_play | 既定尺寸+代码位置 |
| UI-D04 Levels | Ø150橙，(0,-565)，icon_levels | 既定尺寸+代码位置 |
| UI-D05 菜单设置 | Ø150木底，(420,850)，icon_settings | 代码事实 |
| UI-D06 菜单进度牌 | 480×90，(0,-780)，字号32 | 代码事实 |
| UI-D07 HUD暂停 | Ø130，(-420,850)，icon_pause | 代码事实 |
| UI-D08 HUD关卡牌 | 240×100，(0,850)，数字56，无Level文字 | 既定+代码事实 |
| UI-D09 HUD Undo/Hint/Restart | 各Ø130，X=-262/0/+262，Y=-800；无Undo时留原槽不重排 | 既定类型+代码坐标；隐藏行为按INTERACTION |
| UI-D10 Hint角标 | 60×70，相对Hint(48,45)，数字框52×52/字号32 | 代码事实 |
| UI-D11 关卡页返回 | Ø150，绝对(-396,820)，父header(0,820) | 代码事实 |
| UI-D12 关卡页星数牌 | 185×85，绝对(355,820) | 代码事实 |
| UI-D13 滚动视口 | 804×1440，(0,-40)，底边-760，底部渐隐120 | 既定+代码事实 |
| UI-D14 关卡格 | 150×170，4列；列中心距186/行中心距200，即净距36/30；无章节条 | 既定+LevelGrid代码；不能因“方块”改成150×150 |
| UI-D15 关卡数字 | 已解锁64深棕#5A3A22无描边；锁定白字深描边 | 既定 |
| UI-D16 设置面板 | 560×460，(0,0)，无标题；只一个声音开关 | 既定 |
| UI-D17 设置关闭 | Ø150红，局部(215,165)，icon_close | 代码事实；允许现有角徽式外越面板，仍须在安全区 |
| UI-D18 设置声音 | Ø200木底，(0,0)，sound/sound_off | 既定+代码事实 |
| UI-D19 暂停面板 | 760×760，(0,0)，无标题 | 代码事实 |
| UI-D20 暂停关闭 | Ø150红，局部(315,315) | 代码事实 |
| UI-D21 暂停继续 | Ø220绿，局部(0,130)，icon_play | 代码事实 |
| UI-D22 暂停重开/主页/声音 | Ø150木底，局部X=-220/0/+220、Y=-170 | 代码事实 |
| UI-D23 胜利面板 | 860×1120，(0,0) | 既定 |
| UI-D24 胜利横幅 | 922×180，(0,520)，无文字、不可点 | 既定 |
| UI-D25 左/中/右星槽 | 150/180/150；(-190,540)/(0,560)/(190,540) | 既定；亮星数由INTERACTION评级决定 |
| UI-D26 奖杯鸟父层 | 680×300，(0,180) | 代码事实 |
| UI-D27 奖杯 | 250×290，父层局部(145,0)，绝对(145,180) | 代码事实 |
| UI-D28 结算鸟 | 完整图框340×340，局部(-145,-12.1328125)，绝对(-145,167.8671875) | 代码公式-135+(870/1024-0.5)×340；不是可见羽毛框 |
| UI-D29 Next | Ø220绿，(0,-220)，icon_next；末关不显示 | 既定 |
| UI-D30 Replay/Home | Ø150橙；Replay左(-150,-420)，Home右(150,-420) | 既定+代码明确左右 |
| UI-D31 死局面板 | 760×640，(0,0) | 既定 |
| UI-D32 死局图示 | 红圆底Ø160，(0,130)，内问号96×96；不可点 | 既定+代码事实 |
| UI-D33 死局Undo/Restart | Ø200绿/橙，(-150,-130)/(150,-130)；无历史只显示Restart并居中(0,-130) | 既定+代码事实 |
| UI-D34 引导气泡 | Y=420，宽≤760；不得盖任何鸟/巢/枝 | 既定；密集关存在布局风险，未再实机核验 |
| UI-D35 Skip | Ø130木底、icon_next；位置沿现引导布局，不擅填新坐标 | 既定尺寸 |
| UI-D36 棋盘/枝行 | Y∈[-620,620]，两列同行对齐；行距min(260,1240/行数) | 既定+代码事实 |
| UI-D37 目标鸟巢 | 水平居中Y=700；激活180，等待110/透明度70% | 既定；与救援箱/笼不同 |
| UI-D38 棋盘鸟 | 完整显示框136×136，槽宽80，脚底依manifest/pivot落枝 | 已有T15b尺寸基线，不能冒充可见身宽 |

### 2.1 图标框、净距与触控不是同一数值

UI-D39：UiTheme.Round中图标框=按钮直径×0.6，居中、preserveAspect；因此容器内边距各约0.2D。PNG本身的透明留白另算，不能为填满图标框裁掉脚/冠/尾。图标不接收射线，当前按钮Image的视觉Rect同时充当命中Rect。

UI-D40：HUD相邻中心距262、视觉净距132；菜单Play/Levels中心距235、视觉净距30；胜利Replay/Home净距150；死局双钮净距100。旧文“HUD150间距”的歧义由当前代码坐标消解；这里是视觉净距，不是扩展命中区后的净距。

## 3. 缩放、安全区与设备命中目标（UI-S）

**代码事实**：SceneBuilder配置CanvasScaler=ScaleWithScreenSize，referenceResolution取UI-U01，ScreenMatchMode=Expand，Canvas=ScreenSpaceCamera、planeDistance=5、sortingOrder=100。Expand不使用MatchWidthOrHeight，不虚构match=0.5。

SafeAreaLayout当前计算：视口宽`Vw=min(W,9H/16)`并居中；将视口与Screen.safeArea求交得到SafeRect，`s=min(SafeRect.width/1080,SafeRect.height/1920)`。HUD localScale=s/Canvas.scaleFactor，最终design px映射为s个渲染像素；根中心对齐安全交集中心。背景独立铺满视口。targetTexture截图使用整个输出作安全区，不代表真实刘海/手势通过。

| 计算样例（无安全区扣除） | 视口尺寸 | s | HUD直径的物理像素预测 | 验证状态 |
| --- | --- | --- | --- | --- |
| 1080×1920 | 1080×1920 | 1 | UI-D09×1 | 只按源码公式推算 |
| 1080×2340 | 1080×2340 | 1 | UI-D09×1，额外高度不拉伸内容 | 同上 |
| 1440×1920 | 1080×1920，左右各180留边 | 1 | UI-D09×1 | 同上，不宣称设备实测 |
| 720×1280 | 720×1280 | 2/3 | UI-D09×2/3 | 同上 |
| 实际刘海/手势设备 | 视口∩真实safeArea | 按公式记录 | 同时记录物理/逻辑比例 | PENDING设备验收 |

**UI-S01 / DESIGNED验收目标**：iOS触控区至少44×44pt；Android至少48×48dp。以实际设备像素/pt或density换算：`hitDesignWidth≥targetLogical×physicalPixelsPerLogicalUnit/s`，高度同理。不能把视觉Ø130design px直接说成44pt/48dp。允许在未来获授权后扩大透明命中Rect，但不得放大定版图像、相互覆盖、盖角标/邻钮或侵入手势区。当前未独立扩边，不能宣称已经达标。来源：[Apple Buttons](https://developer.apple.com/design/human-interface-guidelines/buttons)、[Android触控目标](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views)。

## 4. 外观和状态（UI-V）

- UI-V01：原木/奶油/树叶绿/橙色，与现有手绘鸟和栖木统一。按钮只圆形，关卡格圆角矩形；禁用/隐藏/加载/返回行为由INTERACTION拥有。
- UI-V02：Fredoka+Noto Sans CJK回退。白色亮字用深棕#5A3A22描边，奶油说明正文深棕无描边；普通文字≥32design px，关卡数字按UI-D15。按钮无文字；引导最多6英文词/10汉字。不恢复已删版本号、语言/音量/振动/减动画开关入口。
- UI-V03：遮罩最终唯一颜色/透明度为**Linear空间黑#000、alpha0.72**，不是旧0.5。暂停/设置/死局HUD保留原色仅被遮罩压暗，不加第二层禁用灰；通关HUD退场按INTERACTION时间合同。
- UI-V04：主菜单打开设置时保留菜单在遮罩下；任何模态必须盖住全部可交互画面，棋盘不能穿透弹窗层。
- UI-V05：当前目标是**顶部接收鸟巢TargetNest**。用户称“入笼/进笼”在动作说明中解释为回家进入接收容器，不授权把现鸟巢画成锁笼。救援障碍箱/未来笼式遮盖是RescueCover，盖住来源枝、按条件揭示，不能作为归巢目标。

## 5. 差异、未决与验收

只读发现：HUD Undo现代码只设interactable，而新版交互合同无历史隐藏且留槽；PanelMotion入场即interactable不等于主体稳定后开放结果按钮；ShowPause忙碌直接返回与新版暂存请求不同；系统减动画读取未接入。只记录，不在本任务修改。

UI-D34固定气泡位置与“不遮棋盘”可能在密集关冲突；须记录实际重叠并评审，不能私自挪定版UI或缩鸟。设备命中区/安全区、长屏/平板及现代码与合同差异均PENDING，不用概念图冒充运行验收。验收截图/录屏与输入测试见[VALIDATION](spec/VALIDATION.md)。

出处代码（只读）：SceneBuilder.Build、SafeAreaLayout.Apply/ViewportFor、GameHudPages各Build、SettingsPanel.Build、UiTheme.Round、LevelGrid.Build、GameHud.Refresh；角色资产manifest见art/raw/birds_front。实际接口/字段未实现部分见[CODE_MAP](spec/CODE_MAP.md)。
