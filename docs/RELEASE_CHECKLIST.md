# Perch Sort — 发布前清单

核对日期：2026-10-31；Unity 6000.6.7f1。以下均为上架前待办，不代表已经创建账号、签名、上传或发布。当前游戏离线运行，无账号、广告、内购、分析与数据收集；进度与设置仅保存在设备本地。

## 身份、账号与签名

- [ ] 注册并验证 Google Play Console / Apple Developer Program 主体、联系人与双重验证，确认团队角色、地区、协议、税务及欧盟 trader 状态等适用要求；个人新账号的测试/生产访问门槛以控制台要求为准。
- [ ] **公司名 `TODO_COMPANY` 与 Android applicationId / iOS bundle id `com.todo.perchsort` 全是占位；上架前必须替换，且发布后不可更改。** 确定真实主体和唯一反向域名标识后，同时更新 `Assets/Editor/ReleaseSettings.cs` 与商店记录。不要用占位标识上传。
- [ ] 当前 `productName=Perch Sort`、版本 `0.1.0`、Android versionCode `1`、iOS buildNumber `1`；每次上传递增构建号。公司名/产品名影响 Unity 本地存档目录，正式身份冻结后再做升级存档验证。
- [ ] Android 创建上传密钥、启用 Play App Signing，记录证书指纹和恢复负责人；密钥、alias 密码放密码库/CI secret，离线加密备份，禁止放仓库或日志。
- [ ] iOS 在正确 Team 下创建 App ID、签名证书和 provisioning profile，验证 capabilities 与实际用途一致；证书私钥、恢复资料和 App Store Connect API key 由受限密码库保管。

## 工具链与构建配置

- [ ] Unity Hub 为 **6000.6.7f1** 安装 Android Build Support（SDK/NDK Tools 与 OpenJDK）和 iOS Build Support。本机 T7 验证时仅有 MacStandaloneSupport / WebGLSupport，未构建 Android/iOS。
- [ ] Android 使用最低 API **26 / Android 8.0**、目标 API **36 / Android 16**、ARM64、IL2CPP；Unity 6000.6 官方工具链为 SDK 36、NDK r27c、OpenJDK 17。Google Play 自 2026-08-31 对普通新应用/更新要求目标 API 36。发布时再次核对控制台及官方要求。[Unity 系统要求](https://docs.unity.com/en-us/engine/6000.6/manual/get-started/install-and-upgrade/getting-started-installing-unity/system-requirements)、[Google Play 目标 API](https://support.google.com/googleplay/android-developer/answer/11926878?hl=en)
- [ ] iOS 最低部署版本 **15.0**，使用 **Xcode 26+ / iOS 26 SDK+** 构建提交；最低系统版本不是构建 SDK 版本。本机模块未安装，此组合的实际归档与签名仍待复核。[Apple 提交要求](https://developer.apple.com/news/upcoming-requirements/)
- [ ] 在 Unity 执行 **Perch Sort → Apply Release Settings**（命令行 `bash scripts/apply_release_settings.sh`）；幂等配置竖屏、标识、版本、图标、权限及遥测。`run_tests.sh` 的场景重建也会应用该开发占位配置；正式发布前先更新脚本内身份/版本。
- [ ] Android/iOS 模块安装后重跑配置，确认平台图标槽全部填入。默认图标为原始 `art/raw/ui/icon_1024.png` 的字节相同副本；Android adaptive 保留完整平面插画作背景、透明前景，不伪造抠图。必须在圆形、圆角矩形和厂商蒙版下检查鸟冠、尾羽与脚部裁切；若需调整，另产派生资源，不覆盖原件。
- [ ] 确认 Portrait 锁定、关闭横屏及倒置旋转；iPad 当前 requiresFullScreen=true，真机复核系统窗口行为与安全区。Unity 默认启动 Logo / Splash 已关闭（Unity 6 Personal 也允许）；验证真机冷启动到首帧无长时间空白。[Unity 官方说明](https://unity.com/blog/unity-is-canceling-the-runtime-fee)
- [ ] 生成 Release AAB / Xcode Archive；检查 ARM64、16 KB page-size 兼容性与最终 merged AndroidManifest。无相机、麦克风、定位、存储等危险权限；`VIBRATE` 为正常权限，游戏仅 Collect / Win 调用。不应有 INTERNET、AD_ID、广告/支付/统计 SDK；不得把 Editor 能通过当作最终包权限证明。
- [ ] 最终包审查 iOS Info.plist、entitlements、隐私清单和 required-reason API 声明（包括 Unity 自身生成内容），只声明实际使用项；禁用 Development Build、Script Debugging 和网络诊断。

## 隐私、分级和审核说明

- [ ] 发布可公开访问的隐私政策网页（正文、有效日期、真实主体及联系邮箱），说明不收集/分享/追踪数据，进度设置仅在本地、重置进度与卸载的影响，若设备系统备份存在则说明其边界。游戏无账号，不添加虚假的账号删除流程。
- [ ] Google Play Data safety 按最终构建填写“无数据收集、无分享”；无广告、无内购、无需登录。即使不收集数据仍填写表单并提供隐私政策；引擎或 SDK 后续变化必须重新审查。[Google Data safety](https://support.google.com/googleplay/android-developer/answer/10787469?hl=en)
- [ ] App Store Connect App Privacy 按实际填写 “No, we do not collect data from this app”，不追踪、不申请 ATT；确保最终包和第三方依赖吻合，不能只凭产品意图填写。[Apple App Privacy](https://developer.apple.com/help/app-store-connect/manage-app-information/manage-app-privacy/)
- [ ] Google IARC / Apple 新版年龄问卷：如实回答卡通鸟类益智、无暴力血腥、无性内容、赌博/模拟赌博、药物、恐怖、聊天/UGC、网页自由访问、付费随机物品；不要自行保证评级结果。
- [ ] 明确目标年龄及是否主动面向儿童；如选儿童/家庭类别，另核对相应商店政策及素材/外链限制，不因画风可爱自动勾选。审核备注说明离线玩法、40 关、可撤销/重开、无账号无需审核凭据。

## 商店素材

| 平台 / 素材 | 计划尺寸与要求 | 待办 |
| --- | --- | --- |
| Google Play 图标 | 512×512 PNG，≤1024 KB；区别于设备 launcher 图标 | [ ] 由 1024 源另导出，检查小尺寸辨识 |
| Google Play feature graphic | 1024×500，JPEG 或 24-bit PNG，无 alpha | [ ] 制作真实玩法宣传图 |
| Google Play 手机截图 | 至少 2 张，建议 4–8 张 1080×1920，PNG/JPEG；单边 320–3840，长边不超短边两倍 | [ ] 真实 Release 手机运行重拍主菜单、棋盘、收集、通关、设置 |
| Google Play 平板截图 | 支持平板时补 7/10 英寸实际布局截图 | [ ] 不拉伸手机画面 |
| Apple App Icon | 1024×1024、无透明、无预烘焙圆角 | [ ] 模块生成资产目录并在 Archive 验证 |
| Apple iPhone 大屏截图 | 可选 1260×2736 / 1290×2796 / 1320×2868 竖屏，按当前 Connect 槽位；每组 1–10 张 | [ ] 真实匹配设备尺寸拍摄 |
| Apple iPad 13 英寸截图 | 2064×2752 或 2048×2732 竖屏；支持 iPad 时必需 | [ ] 真机/模拟器验证布局后拍摄 |
| 双平台文案与链接 | 标题、简短说明/副标题、完整说明、关键词/分类、版权、支持邮箱/URL、隐私 URL、版本说明；按各字段当前字数限制 | [ ] 英文与计划发行语言逐项审校 |
| 可选预览视频 | 另按商店当前视频规格制作；非本轮交付 | [ ] 不包含不存在的功能 |

尺寸依据：[Google Play 素材](https://support.google.com/googleplay/android-developer/answer/9866151?hl=en-GB&rd=1)、[Apple 截图规格](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/)。`build/screens/*.png` 是 Editor Play Mode 的 1080×1920 验收图，**不是已验证的所有设备商店截图**；第 11 张明确显示 Reduce motion 开启。

## 真机与发布闸门

| 测试组 | 至少覆盖 | 通过标准 |
| --- | --- | --- |
| Android 最低端 | API 26、ARM64、低内存设备 | 冷启动/连续游戏/存档正常，无危险权限弹窗 |
| Android 当前系统 | API 35/36，至少两种厂商，含 16 KB 页面设备 | edge-to-edge/刘海/手势返回、图标蒙版、旋转锁定正确 |
| iPhone | 最低 iOS 15 可用设备 + 当前 iOS 的刘海/Dynamic Island 设备 | 启动、前后台/来电中断、静音开关、设置重启保存、触控正常 |
| iPad / Android 平板 | 4:3 与宽屏、安全区/系统窗口 | 竖屏、棋盘和文字无裁切、按钮可触达 |
| 音频与震动 | 手机外放 + 耳机；支持/无震动硬件 | 7 音效事件对应，静音立即停止，菜单/场景切换无重叠管理器；无 BGM 时无错误；Collect/Win 才震动、Error 不震动 |
| 动效与输入 | 默认/Reduce motion 两档，快速连续点下一关与返回菜单 | 淡出淡入设计总时长 0.20 s，无遗留遮罩/输入锁；减动效无抖动/粒子且能通关撤销 |
| 存档与离线 | 飞行模式、杀进程重开、上一版本升级、重置取消/确认 | 不依赖网络；进度/开关保存；重置不删设置；正式身份目录下升级不丢档 |
| 性能与稳定性 | 连续 20 分钟，关 1/20/40，反复前后台/通关 | 无异常、持续对象/音源增长、明显帧卡或音频疲劳 |

- [ ] 运行 `bash scripts/run_tests.sh` 全绿并归档 XML、日志。
- [ ] 运行 `bash scripts/screenshots.sh`，逐张验收 11 张真实 Play Mode 图；真人听音和真机触感单独签字。
- [ ] 真机签名包完成上述矩阵，检查离线启动和运行网络行为、权限、签名、身份与版本，不把源配置或 Editor 测试代替包验收。
- [ ] 由账号所有者确认最终占位清零、素材、政策表单及签名；再进入内部测试 / TestFlight，按控制台实际要求逐步放量。本任务没有执行上传或发布。
