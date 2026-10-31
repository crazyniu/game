# T7 — 音频/震动接线 + 应用图标/播放器设置 + 手感打磨

已验收：T5（UI/菜单/选关/设置，`SettingsStore` 契约见 `tasks/T5_report.md`）、T6（`AudioManager` 与 7 个 WAV，见 `tasks/T6_report.md`）。

## 做什么
1. **音频接线**：在 View/UI 层事件点调用 `AudioManager`：选中→Select；落位→Move；集齐飞向树冠→Collect；通关→Win；非法操作→Error；撤销→Undo；所有按钮→Click。`SettingsStore` 的音效/音乐开关变化（含启动时读取）必须同步到 `AudioManager`；设置页开关立即生效。AudioManager 在菜单与游戏场景间保持单例，不重复创建。无 BGM 资源时音乐开关仍保存但无声，不报错。
2. **震动**：封装 `Haptics`（`Handheld.Vibrate` 或平台安全的轻量实现，编辑器/不支持平台空操作），仅在 Collect 与 Win 触发，受设置开关控制；Error 不震动。
3. **应用图标与播放器设置**（Editor 脚本一键应用，幂等）：
   - 用 `art/raw/ui/icon_1024.png` 设置各平台图标（Android 自适应图标前景/背景若需要则合理拆分，iOS 使用 1024 源）。
   - 竖屏锁定（Portrait，禁用横屏旋转）；`productName` 用 `Perch Sort`；公司名与 bundle id **使用占位**：`companyName=TODO_COMPANY`、Android applicationId / iOS bundle id = `com.todo.perchsort`，并在 `docs/RELEASE_CHECKLIST.md` 明确标注“上架前必须替换，且发布后不可更改”；版本号 `0.1.0`，Android versionCode=1，iOS buildNumber=1。
   - 目标 SDK/最低系统版本按 Unity 6000.6 与当前商店要求设置（在报告里写明取值与依据；不确定处标出让我复核）。
   - 关闭不需要的权限与包（Android 不请求任何危险权限；不引入网络/广告/分析 SDK）。
   - 启动画面：关闭 Unity 默认 Logo（若许可允许）或保持默认并在报告说明。
4. **手感打磨（小范围、可验证）**：游戏内加载下一关/返回菜单的过渡淡入淡出（≤0.25 秒，可被快速点击打断不卡死）；通关与集齐的粒子/缩放庆祝（轻量，不引入新插件）；`减少动效` 设置项（关闭抖动/粒子，保留必要位移）。
5. **测试**：新增 EditMode/PlayMode 测试：设置开关→AudioManager 状态同步；Haptics 在关闭时不触发（可注入假实现）；场景切换后 AudioManager 唯一；全套保持全绿；`scripts/screenshots.sh` 保持可用并补一张设置页（含减少动效开关）。
6. 在 `docs/RELEASE_CHECKLIST.md` 写出（不执行）上架前待办：账号、bundle id、签名与密钥管理、Unity 需安装的 Android/iOS 模块、隐私政策与数据安全/隐私标签（当前：无数据收集）、年龄分级问卷要点、商店素材清单与尺寸、真机测试矩阵。

## 约束
- 不改 Core 行为；不碰 `art/` 原件；不提交 git；无联网依赖；不接广告/内购/分析。
- 不修改 WAV 资产内容；如认为音效需要调整，写进报告。

## 验收（Claude 亲自执行）
- `bash scripts/run_tests.sh` 全绿；截图脚本可用；报告 `tasks/T7_report.md`；`docs/RELEASE_CHECKLIST.md` 完整。
