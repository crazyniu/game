# 动画开源方案、许可与兼容门槛

2026-10-31 · 只调查/设计，**未安装任何库、未写运行代码、未验证本项目Unity6000.6.7f1或移动IL2CPP**。本页是选型/许可证唯一入口，动画时长归INTERACTION，玩法归RULES。用户要求能用开源库优先用、单一主补间库、所有动画位置不能用固定图片冒充完成；这些是USER约束，不是竞品规则。

## 1. 推荐与比较

| 方案 | 第一方已核事实 | 本项目决定/限制 |
| --- | --- | --- |
| [LitMotion](https://github.com/annulusgames/LitMotion) | MIT；官方要求Unity≥2021.3、Burst≥1.6、Collections≥1.5.1、Mathematics≥1.0；支持Transform/UI/TMP补间、Punch/Shake、LSequence | **唯一主补间库首选候选**，待获授权后锁版本与兼容试验；没有安装/接入。源码可读不等于本项目每帧无分配或所有平台已通过 |
| [LeanTween](https://github.com/dentedpixel/LeanTween) | [源码头部MIT](https://raw.githubusercontent.com/dentedpixel/LeanTween/master/Assets/LeanTween/Framework/LeanTween.cs)，缓动方程另含BSD通知，常用补间/路径能力；具体版本/目标平台需未来核对 | 仅当首选路径方案实测不足时评审替代，不与LitMotion重复承担主补间 |
| [DOTween](https://dotween.demigiant.com/license.php) | 自定义许可，允许使用、对修改版再分发有限制；不是MIT | 不随口标“MIT开源”；不作为默认首选。[下载说明](https://dotween.demigiant.com/download)有Unity6000.6相关meta workaround记录，渠道/版本需核对，不能用泛版本兼容声明替代测试 |
| [Rive Unity](https://github.com/rive-app/rive-unity) | MIT代码、技术预览状态及独立.riv资源管线 | 当前不为本项目新增整套管线；许可不包括任意第三方.riv内容 |
| Unity Animator / Particle System / [Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html) | Unity原生工具/官方包，不是独立MIT补间库 | 分别承担角色clip、真实粒子和官方路径工具；不能伪称所有Unity引擎代码MIT开源 |

本表为当前第一方声明与父任务独立核查的转述，不是法律意见或已完成依赖审计。未来选定版本必须保存该版本许可证/NOTICE、来源、tag/commit、依赖版本和目标平台记录；库许可不替代角色/音效/粒子纹理的资产许可。

## 2. 能直接复用什么，不能假设什么

[LitMotion动作控制](https://annulusgames.github.io/LitMotion/articles/en/motion-control.html)提供MotionHandle、TryCancel、AddTo(GameObject)和CompositeMotionHandle，用于存在性检查、对象销毁绑定和批量取消；重复调用已结束句柄不能当作永远有效。需要暂停/恢复的调度由库提供的Scheduler/PlaybackSpeed等能力承接，UI在暂停页继续播放时须确认采用IgnoreTimeScale类时钟而非手写第二套时间框架。

[官方迁移说明](https://annulusgames.github.io/LitMotion/articles/en/migrate-from-dotween.html)明确不具备与DOTween全部功能一一对应的路径、AppendCallback或SetSpeedBased；路径建议用Unity Splines。不得因为API名像就编造调用，禁止另手写补间/样条/调度框架。确需少量游戏事件编排，可消费已计算事件列表并调用库Sequence/取消接口，不把逻辑转移到动画回调。

## 3. 动画库与素材分别验收

| 动作 | 库/工具职责 | 必需的真实动作证据 |
| --- | --- | --- |
| 起飞/跨枝/落枝 | 主补间库控制位置/旋转/缩放及错峰；Animator或验收序列帧控制振翅/收翅 | 翅膀形态确实周期变化，脚底落位/收翅；单张fly图片平移旋转不算真实振翅 |
| 进入鸟巢 | 库编排接近/入口缩小/落脚/遮挡与接收 | 鸟从巢前到内部的过程、前后层遮挡和逐只接收，不是静图覆盖/瞬间删鸟 |
| 完成枝退场/问号孵化 | 库与已批准帧/clip表现已提交事件 | 枝退场随时间变化，蛋壳→裂壳→鸟有阶段；不能据动画自定触发 |
| 结算 | 主库面板/角色时间序列，Particle System发射与寿命 | 粒子实际生成/运动/消亡、面板进场和角色动作的运行录屏；一张庆祝海报不通过 |
| 地图/按钮/弹窗 | 同一主库的淡入/过渡/按压 | 点击反馈/锁态与世界开放顺序有真实交互；不得用图片假按钮 |

当前只读项目事实：10种×stand/blink/sleep/fly共40张静态姿态贴图，UIAssetBuilder加载四态，BirdView.SetPose切Sprite；BoardView.FlyRun使用单fly姿态配位移/旋转/缩放。它证明资源已接入，**不证明有多帧振翅/骨骼clip，也不满足最新真实振翅要求**。不重画替换现有角色；未来有授权后补足动作资源/clip并单独验收。静态背景和原始帧可作为素材，最终动画整体必须随时间变化；减少动画也至少保留短过渡的状态序列，不是一张图占据整个动作。

## 4. 将来实施前与交付验收

- 明确安装/代码/资源权限，遵251开发/Mac测试；此文不构成安装命令授权。
- 锁主库与依赖版本；验证Unity6000.6.7f1、iOS/Android IL2CPP、AOT/裁剪、设备性能及包冲突，不以版本下限代替实测。
- 检查暂停/后台/IgnoreTimeScale、连点、Undo/Restart/Next、场景销毁、池回收再用、批量取消；旧viewGeneration句柄不能写新对象。对象池回收不等于GameObject销毁，单靠AddTo销毁绑定不足，需要每次租用生命周期取消。
- 动作完成/取消/回调竞争仍只消费一次已提交事件，不能重复Home/Retire/解锁/发奖；动画失败不回滚已提交规则或永久锁输入，按稳定快照同步。
- 每个动作录可播放真实运行视频和时间采样，截图只补布局；检查真实振翅、入巢遮挡、粒子寿命，禁止静图当动画完成。

当前状态：调查完成；最终锁版本、安装、代码/移动兼容、素材clip及运行验收全部NOT_RUN。
