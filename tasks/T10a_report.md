# T10a v2 美术交付报告

状态：**已完成素材制作与本地文件检查，待 Claude 视觉验收 / 游戏接入**。未提交 Git，未修改 `PerchSort/`；除本指定报告外，写入范围仅为 `art/` 与 `docs/LICENSES.md`。

## 已交付

- 15 项要求的基础素材：crate 1024×320；basket 512×384；nest / nest_done 512×512；trophy 1024×1024；closed_eye 128×128；zzz / sparkle / check / feather / ring / confetti_01..03 / star 各 256×256。
- 10 张 `art/raw/birds/bird_01s.png`～`bird_10s.png`，各 1024×1024；脚底 y=870，沿用对应原鸟的脚部 pivot。
- `art/manifest.json` 已追加尺寸、RGBA、alpha 外框、锚点、用途、源图和 batch 字段；原条目保留。木箱文字区、篮子站位、巢图标中心另有坐标。
- `art/contact_v2.png`（深浅底并排），`art/contact_v2_light.png`、`art/contact_v2_dark.png`（独立底色），以及 `art/qa/sleep_v2.png`（睡眠鸟与原鸟对照）。每项都含真实 64px 缩图。
- 来源说明已追加到 `docs/LICENSES.md`；完整制作 / 使用约定见 `art/ASSETS.md`；25 张母版在 `art/source/v2/`；28 次内置 image_gen 调用的完整提示词与编辑目标见 `art/prompts/generation_v2.json`。

## 睡眠方案与修订

直接把透明闭眼线叠在原鸟的大块深色眼球上，无法覆盖原眼和白色高光，明显穿帮。因此按任务第 6 项允许的替代方案，为 10 种鸟各自用原图生成闭眼版；保留独立 closed_eye 资产，但现有鸟接入推荐切换 `bird_XXs`，再由代码压扁、变暗并显示 Z 气泡。生成编辑保留了物种轮廓与识别花纹，局部羽毛 / 腮红并非逐像素不变；实际对比已展示，未声称是纯眼区像素补丁。

篮子、空巢、奖杯初稿有白色贴纸边，已通过内置工具分别修订并弃用初稿。完成巢以最终空巢为目标独立编辑，加入恰好 3 枚小蛋并保持同一巢形。除明确要求的 Z 睡眠符号外，生产素材没有文字。

## 验证与结果

执行 `art/.venv/bin/python art/tools/build_v2_assets.py` 成功；实际输出保存在 `art/qa/check_v2.txt`。25 项全部满足指定尺寸、RGBA、透明背景与四周 8px 安全边距，光环中心 alpha=0，10 张睡眠鸟脚底线检查通过；没有额外编写游戏测试或运行无关测试。

直接查看 `contact_v2.png` 与 `qa/sleep_v2.png`：深浅底均未见明显白边或棋盘格残留，64px 下主体可辨，四角闪光 / 五角金星、空巢 / 三蛋完成巢有清楚区别；睡眠鸟无原睁眼残留。木箱在 64px 下轮廓可辨，小锁与通气槽需按实际整根栖木尺寸显示。

## 接入提示 / 剩余工作

本任务未触碰游戏项目，未做 Unity 运行验收；Claude 仍需确认视觉风格与实际渲染的遮挡、缩放和睡眠动画。木箱须作为前景覆盖整根栖木及上方鸟，中心文字由 UI 叠加；篮子的实际鸟脚和前沿遮挡以 manifest 中建议坐标开始调试。睡眠版与原图已按相同 alpha 外框对齐，脚部 pivot 可复用，但切换动画仍需在运行时确认。

重新导出 v2 使用 `art/.venv/bin/python art/tools/build_v2_assets.py`；如先运行初版 `build_assets.py`，需随后运行 v2 脚本恢复追加的 manifest 条目。没有新增 `btn_*`。
