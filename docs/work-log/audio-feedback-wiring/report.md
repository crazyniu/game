# T7 — 音频、震动、发布配置与动效接线

日期：2026-10-31；目录 `/Users/apple/Documents/projects/game`；Unity 6000.6.7f1。实现与本机验收脚本已交付，**EditMode 101/101、PlayMode 11/11 全绿，11 张 1080×1920 截图成功**。手机平台构建、平台专属图标槽、真机听感/触感及最终视觉验收仍待完成，详见末节。没有 Git 提交、签名或上传。

## 完成内容

### 音效与设置

`BoardView.Feedback` 在动画事件点发出 `Sfx`，由 `GameController` 调用现有 AudioManager；Core 完全不参与或改变。

| 事件 | 时点 / 音效 |
| --- | --- |
| 选中 | 选中非空栖木时 Select |
| 落位 | 单鸟飞行到目标槽后 Move |
| 集齐 | 四鸟飞向树冠开始时 Collect |
| 通关 | 最后一组收集结束后 Win；默认庆祝结束再显示结果卡 |
| 非法操作 | 空栖木初选或 Core 拒绝的目标操作 Error；不震动 |
| 撤销 | 有效撤销时 Undo |
| 所有按钮 | UiTheme 的共同按钮入口播放 Click，包含菜单、设置、选关、暂停、结果、确认按钮 |

启动先从 SettingsStore 读取并应用音效/音乐/震动/减少动效，再开放 UI。`SettingsStore.Changed` 每次保存同步 AudioManager 的状态、Haptics.Enabled 与 BoardView.ReducedMotion；场景销毁时解除订阅。首个 GameController 创建 AudioManager，后续菜单/游戏页面或 Game 场景重新加载均复用 DontDestroyOnLoad 实例；重复实例仍由既有 Awake 逻辑销毁。

没有 BGM 资产，所以接线不调用 PlayMusic；音乐开关照常保存并即时同步 MusicEnabled，不加载缺失资源或产生缺失 BGM 警告。已有 AudioManager 音源池、限频与音量不变，七个 WAV 未修改。

### 震动与动效

- 新增纯 C# `Haptics`：默认设备回调仅在非 Editor 的 Android/iOS 编译 `Handheld.Vibrate()`，其他平台为空操作；构造函数可注入假回调。关闭后不调用设备、不排队，只有 Collect / Win 分支调用 Pulse，Error 不调用。
- 新增 `ScreenTransition`：下一关/加载关卡、游戏或暂停返回菜单，淡出 0.10 秒 + 淡入 0.10 秒，以 unscaled/realtime 计时。新导航取消旧协程和未执行的加载回调，页面即时导航也会取消残留遮罩；切换过程中棋盘禁用，完成后恢复当前页面输入。没有 timeScale 锁，也不会积压多次加载。
- 默认模式 Collect / Win 生成 12 个短命 Sprite 粒子，0.45 秒自动销毁；使用 Unity 内置白纹理，不引入插件/美术资源。收集保留缩小与飞行，通关鸟群增加轻微缩放呼吸。
- 设置页新增 `Reduce motion`，默认 OFF；开启关闭非法操作抖动、落位压缩、飞行弧线、收集缩放与通关群鸟/粒子庆祝，保留选中、落位、归巢与撤销的必要位移以及状态淡出。轻量页面透明度过渡保留。
- `SettingsData` 保持 version 1，新增可选 bool `reducedMotion=false`；旧文件没有该字段时默认全动效，现有三个开关不变。

## Editor 一键发布配置

菜单 **Perch Sort → Apply Release Settings**；命令行 `bash scripts/apply_release_settings.sh`。`SceneBuilder.Build` 也会应用，因此测试/截图场景重建一致。所有 ProjectSettings 修改通过 `ReleaseSettings.cs` 的 Editor API 完成，没有直接改 YAML。

| 配置 | 实际持久化值 |
| --- | --- |
| 身份 | productName `Perch Sort`；companyName `TODO_COMPANY`；Android / iOS `com.todo.perchsort` |
| 版本 | `0.1.0`；Android versionCode `1`；iOS buildNumber `1` |
| 屏幕 | Portrait；横屏两方向及倒置旋转关闭；iOS requiresFullScreen=true |
| Android | min SDK 26 / Android 8.0；target SDK 36 / Android 16；ARM64；IL2CPP |
| iOS | 最低 15.0；IL2CPP；提交工具链要求 Xcode 26+ / iOS 26 SDK+，未在本机归档 |
| 权限 | forceInternetPermission=false、forceSDCardPermission=false；无相机/麦克风/定位使用描述及相关调用；震动为正常权限，最终 Manifest 待打包核对 |
| 采集 | Analytics enabled / initializeOnStartup=false，CrashReporting=false，EngineDiagnostics=false，enableCrashReportAPI=false |
| 启动 | Unity Logo 与 Splash 均关闭，实际保存值为 0 |

**公司名和 bundle id 仅为占位，上架前必须替换，且发布后不可更改。** 正式发布前应更新配置脚本，避免后续场景重建写回占位；身份变化也会影响 Unity 默认本地存档路径。

最低版本依据 [Unity 6000.6 系统要求](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements)：Android 8/API 26、iOS 15；其 Android 工具链为 SDK 36、NDK r27c、OpenJDK 17。目标 SDK 36 依据 [Google Play 官方要求](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en)，2026-08-31 起普通新应用和更新须 target API 36。Apple 当前要求见 [Upcoming Requirements](https://developer.apple.com/news/upcoming-requirements/)：2026-04-28 起 Xcode 26+ / iOS 26 SDK+；部署最低 15.0 与构建 SDK 是不同概念。Unity 6 允许 Personal 关闭 Splash，依据 [Unity 官方声明](https://unity.com/blog/unity-is-canceling-the-runtime-fee)。上述规则核对于本报告日期，提交时必须复核。

### 图标与本机模块边界

- 原始 `art/raw/ui/icon_1024.png` 只读，字节相同副本保存为 `Assets/Art/Icons/icon_1024.png`，作为默认 1024 图标；不覆盖已有 `Assets/Art/ui` 或 art 原件。
- 原图是已合成的完整插画，Android adaptive 使用透明前景 + 原图背景，避免复制两只鸟或伪造抠图；透明层为 `Assets/Art/Icons/adaptive_foreground.png`。脚本按平台支持的 icon kinds/slots 分配，iOS 全部应用图标使用 1024 源。
- **本机仅安装 MacStandaloneSupport、WebGLSupport，没有 Android / iOS Build Support**；GetSupportedIconKinds 返回空，因此本机已验证默认图标，尚未填充/验证手机平台专属槽。日志明确提示安装模块后重跑；没有伪造已成功配置的平台槽。需协调者/发布负责人安装模块、运行脚本，并检查导出资产目录与 Android 圆形/圆角/厂商蒙版裁切。
- 图标生成是固定字节输入；重复场景重建后所有 ProjectSettings 与 Icons 文件 SHA256 不变，见 `build/t7-scope-verification.json`。

## 包清理与范围

协调者在本 Dispatch 的 ask 回复中明确授权修改 Packages，要求先备份与保留真实依赖；已先备份 `build/manifest.backup.json`，再移除以下 9 个未使用直接依赖，Unity 自动更新 packages-lock：

```
com.unity.collab-proxy
com.unity.learn.iet-framework
com.unity.modules.unityanalytics
com.unity.modules.unitywebrequest
com.unity.modules.unitywebrequestassetbundle
com.unity.modules.unitywebrequestaudio
com.unity.modules.unitywebrequesttexture
com.unity.modules.unitywebrequestwww
com.unity.modules.video
```

video 模块会传递依赖 UnityWebRequest，项目没有视频功能，故一并移除。最终 manifest / lock 无上述网络、分析、协作包；没有新增广告、内购、分析、联网 SDK。C# 运行时 Scripts 中无网络请求/危险权限调用。保留 `com.unity.ugui`（含 TMP）、Input System、URP、Test Framework、2D 工具链、IDE、timeline/visualscripting 及其余引擎基础模块，避免对已有模板资源进行无关清理；完整保留清单就是最终 manifest，相对备份仅删除这 9 项、未升级/新增其他依赖。

修改文件：

- `Assets/Scripts/UI/{GameController,GameHud,SettingsStore,Strings,UiTheme}.cs`、新增 `ScreenTransition.cs`，UI asmdef 添加 Audio 引用。
- `Assets/Scripts/View/BoardView.cs`、新增 `CelebrationBurst.cs`，View asmdef 添加 Audio 引用。
- 新增 `Assets/Scripts/Audio/Haptics.cs`；现有 AudioManager/WAV 不改。
- 新增 `Assets/Editor/ReleaseSettings.cs`；`SceneBuilder.cs` 接入配置；`PerchCaptureSequence.cs` 等待页面过渡并新增截图。
- 新增 `Assets/Tests/EditMode/FeedbackTests.cs`；扩展 `PersistenceTests.cs`、`GameplaySmokeTests.cs` 及测试 asmdef 引用。
- `ProjectSettings/{ProjectSettings,UnityConnectSettings}.asset` 为 Editor API 生成；`Assets/Art/Icons/**` 为获准派生资源；`Assets/Scenes/Game.unity` 为既有 SceneBuilder 验证过程重建输出。
- `Packages/{manifest,packages-lock}.json`，`scripts/{run_tests,screenshots,apply_release_settings}.sh`，`docs/RELEASE_CHECKLIST.md`，本报告；Unity 自动生成新文件 `.meta`。

运行前记录了 Core、整个 art/、七个 WAV 的 1181 个受保护文件 SHA256；交付核对 **零变化**，未改 Core 行为、关卡数据、美术原件或 WAV，没有 git commit。

## 验证证据

实际执行：

```sh
bash scripts/run_tests.sh
bash scripts/screenshots.sh
bash -n scripts/screenshots.sh scripts/run_tests.sh scripts/apply_release_settings.sh
```

结果：**EditMode 101/101，PlayMode 11/11，0 失败；截图脚本退出 0，11 张 PNG 均为 1080×1920。** 新增 2 个 EditMode、3 个 PlayMode 用例；已有测试只补等待页面切换，不删断言或降低要求。测试入口最小数更新为 101 / 11。

- EditMode：可注入假设备的震动开关/不排队；旧 Settings v1 缺 reducedMotion 字段的兼容。原设置往返测试加入 reducedMotion 保存断言。
- PlayMode：实际设置按钮立即关闭音效/音乐/震动并开启减少动效，音源立即 mute，重新加载 Game 读取设置并保持同一 AudioManager，额外重复实例销毁；音乐无资源时没有意外日志。
- PlayMode：快速 load→menu 和 load20→load10，仅最新导航生效，页面/关卡/棋盘输入恢复，能够继续选择鸟。
- PlayMode：开启减少动效真实求解通关，按移动数核对 Select / Move、按物种数核对 Collect，最后 Win；无庆祝粒子，状态与画面同步，撤销发出 Undo；空位操作发出 Error。
- 原 1/10/20/30/40 通关、撤销、非法操作、重开恢复、死局、菜单/解锁/暂停/设置/重置烟测全部保留通过。
- 配置幂等：截图重建再次调用 ReleaseSettings，前后全部 ProjectSettings 与 Icons 哈希一致。受保护哈希、幂等结果在 `build/t7-scope-verification.json`。

新增 `build/screens/11-settings-reduced-motion.png` 显示 Reduce motion ON；原 `08-settings.png` 同样含新行，OFF。原 01–10 的关卡/菜单/选关/暂停/死局/胜利截图仍可生成。已查看设置页与胜利页，文字和按钮完整，过渡没有留下遮罩；最终视觉验收交由协调者。

证据：`build/test-results.xml`、`build/playmode-results.xml`、`build/{scene-build,editmode-tests,playmode-tests,screenshots}.log`、`build/screens/*.png`。编译首轮曾因只读的 AnalyticsSettings.deviceStatsEnabledInBuild 赋值失败，已删除错误赋值；后续整套全绿。日志中的 Editor Licensing access-token 提示没有阻止退出 0，不是游戏的联网行为；测试断言无意外运行时日志。新测试的 FindObjectsByType 使用了兼容重载，有 Unity 6.6 弃用警告但无编译错误。

## 尚待验收 / 不作过度声明

1. 未构建/安装 Android/iOS，平台模块及专属图标槽尚待安装后验证；最终包权限、16 KB page-size、iOS Archive/签名、SDK 组合与窗口/安全区必须以真机包为准。
2. Editor 空操作与假回调证明震动开关逻辑，不能证明手机物理震感。声音事件/开关测试不替代真人听音；本轮未调整 WAV 或宣布音效听感通过。
3. 0.20 秒为页面过渡代码设计时长；低帧率设备上显示时长受帧调度影响，快速点击与取消路径已由 PlayMode 验证，手机仍需确认。
4. `docs/RELEASE_CHECKLIST.md` 已写完整账号/身份、密钥签名、模块/SDK、隐私政策与无收集标签、年龄问卷、素材尺寸、真机矩阵及发布闸门；这些事项只列出，没有创建账号、处理密钥或上传商店。
