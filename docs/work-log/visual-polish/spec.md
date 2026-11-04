# T10c — 接入正式美术 + 表现层打磨

前置：T10a 美术已验收（`art/raw/{env,fx,ui,birds}`，含 `bird_NNs.png` 睡眠鸟、crate/basket/nest/nest_done/zzz/sparkle/check/feather/ring/confetti_01-03/trophy/star/closed_eye）；T10b 表现层已验收（报告 `tasks/T10b_report.md`，末节“T10c 占位资源契约”给出 Resources 键）。Unity 编辑器未开，你独占项目。

## 必须做
1. **接入真素材**：把 art/raw 新增素材按 Unity 导入规范放进 `PerchSort/Assets/Art/…`（沿用现有 Art 目录与 import 设置脚本/约定，PPU 与 pivot 参考 `art/manifest.json`），按 T10b 契约键接入：箱体 crate、篮子 basket、目标巢 nest（激活/完成态 nest_done）、睡眠鸟用 `bird_NNs`（不再用变暗占位）、Z 用 zzz.png 气泡（飘动+淡出）、sparkle/check/feather/ring、彩纸三种随机纸片、结算页奖杯 trophy 与星 star。**不得改 art/ 原件**；缺口回报。
2. **修已知问题（我在截图验收中发现）**：
   a. 右列栖木上相邻的鸟重叠（第 40 关最明显，右列 `Side==Right` 镜像后间距/锚点不对）：相邻鸟间距必须一致、任何关卡不重叠。
   b. 鸟与栖木偏小，两列之间空白过大：在 9:16 内尽量放大（栖木宽度占比提高，鸟身适当放大），同时 10 栖木关卡仍不重叠、不挤进顶部巢区/底部篮子与按钮区。
   c. 栖木行距按可用高度均匀分布并居中；第 1 关（5 根）不要显得偏上或稀疏；篮子位置与大小合理（接入 basket 素材后与鸟比例协调）。
   d. 板条箱要盖住整根栖木及其上鸟（含短枝宽度），不要比栖木宽出屏幕边缘。
3. **观感打磨**（对照 DESIGN「表现要点」）：选中抬升有轻微弹性；落地挤压回弹；归巢时巢口 ring 光环+sparkle，check 弹出缩放；羽毛飘落有旋转；目标替换动画流畅；彩纸用 3 种纸片，带重力与旋转；结算页接奖杯。Reduce motion 仍生效。
4. 补测试：关键素材在 Resources 中可加载（缺失即失败）；布局测试——对关卡 1/10/20/30/40 断言同侧相邻鸟包围盒不相交、所有栖木在安全区内。
5. `bash scripts/run_tests.sh` 全绿；`bash scripts/screenshots.sh` 出新图（含 1/20/40 关、睡眠、归巢、结算页）。

## 约束
不改 Core、关卡数据；不提交 git；报告 `tasks/T10c_report.md`（含布局数值、截图清单、自行决定的点）。

## 验收（Claude）
我重跑测试；逐张看截图找重叠/比例/穿帮。
