# 小鸟归巢：同行依据版PRD

PRD4 · updated 2026-11-01 · Current design; the isolated T21 first-level scene/wingbeat/Home implementation exists, while full campaign and economy runtime remain incomplete. 替代T16的自拟机制主案；先读[50项证据矩阵](MECHANIC_EVIDENCE.md)，再读[RULES](spec/RULES.md)。T21首关实现与普通设计决定已获授权；Core/依赖验证见T21报告，生产UI与完整v4/save6尚未完成。保留内部技术/独立视觉素材QA，不自动合并/部署。

## 1. 产品与证据边界

竖屏离线Sorting Puzzle，保留小鸟归巢题材与端部整理。所有有效玩法都能指向同行明确规则，用户新要求单列USER，存档/动画取消等列E工程协议；**不能把“合理”“原创”“现行代码已经有”当同行证据**。用户要求固定关卡，Replay/Restart/重进不随机；固定开局不意味着局内加空间被禁止。

不再执行旧双目标、睡眠冻结、完成数/指定色开箱、已知纹样蛋、任意搬空临时枝退休、单鸟篮、Undo扣星、终身3提示或风闸。部分有旧观察但本轮缺精确可定位边界，隔离待证；旧40阵列与60回放全部失效，不延续其证书。

## 2. 有出处的统一循环

| 采用范围 | 依据 / 差异 |
| --- | --- |
| 先选外露连续同种群，再选空/同种外端，有容量才移动 | Leafy Bird Sort；不足余量只移装得下部分另取Simple Games Water Sort，明确水→鸟移植 |
| 每关统一容量4/6/8、各类同数；02–40为K+2枝/两空，01为已采纳N4/K2/P3/零空教学 | Simple的4单位与两空配置、BOOPFUN的6/8层迁移；具体固定布局是我方策划，不声称复制某个原关 |
| 完整同种群自动归家，并失去该完成枝 | Bird Sort2已明确；取消旧“任意搬空就退枝”，普通搬空仍可用 |
| 问号蛋前鸟移走才孵 | Bird Sort Quest；用户要求species固定，但玩家孵前不知道，不画纹样预告；盲玩公平还需验证 |
| 免费无限回退/重开；提示依帮助技能合同；最佳操作记录、不限时 | Simple/Leaf等已有先例；Hint需合法胜利路径首步，不必全局最优，未知不称无解 |
| 每局可选免费加一条N座空枝 | WaterSort.in浏览器版每关一空容器迁移，容量沿本关N；Undo/保存/助力记录为项目E；不外推Android规则 |
| 世界地图/顺序解锁/分主题环境 | 用户明确要求；同行地图/主题世界仅作为参考，不把我方关数或门槛说成原作值 |

玩家读盘→点来源/目标→真实群飞落枝→若露蛋就孵→若完整群就归巢且完成枝退场→继续，直至全部鸟回家。没有未轮到而睡眠的规则，没有手动放飞确认。包括加枝在内的完整数值/事件顺序只由RULES定义，不在PRD重复第二算法。

## 3. 40关与世界

[世界与进度合同](MAP_PROGRESSION.md)拥有章节分配/解锁；[LEVEL_DESIGN](LEVEL_DESIGN.md)拥有每关教学/取舍，[固定附录入口](LEVEL_SIMULATIONS.md)拥有明确初态/回放。内容不由运行时AI补齐。

不同关通过固定的互挡图、连续段长、剥层、蛋揭示位置、完成失枝后的中转安排和群大小变化产生不同选择。它们共享一套操作，不承诺40种完全不同基础规则；无依据的机关不会为“多样性”硬塞回去。每世界含教学、练习、挑战、舒缓，章节数是本项目课程配置，不是竞品定值。

当前40关是新规则文档候选；所有旧路径先作废，再在独立临时模型重放。完整信息有解≠问号蛋盲玩公平≠Unity可玩≠最优证明；分别保留状态和证据。

## 4. 已证功能、未定参数和商业化

额外普通树枝已补SRC-WSWEB同类明确来源：每当前局可选免费一条N座空枝，Undo/Restart/保存/Hint/成绩分类由RULES R07闭环；容量N为本项目适配，不是竞品鸟座位原数。仅L01改为两步教学，其余39不变；当前40主路共472次无助力Transfer。Reserve growth and one-way collectors remain pending. Coin skins/help skills now have the explicit initial tuning table in ECONOMY_SHOP; those values are project decisions, not competitor measurements.

Current design adopts coin-only skins, four help skills/three slots, consumable inventory/permanent tiers, first-clear coins and voluntary ad doubling under [ECONOMY_SHOP](spec/ECONOMY_SHOP.md). Undo/Restart remain free; Hint now has free-per-attempt then inventory policy. Shuffle is player-initiated and uses authored deterministic candidate plans, not random new levels. No real-money IAP quotation/integration, energy, forced ads or pay-to-complete boards. Stars remain undefined. All economy/skill runtime is DESIGN_ONLY; optional ad SDK is not integrated.

## 5. UI与动画硬约束（USER）

静态尺寸唯一归[UI_SPEC](UI_SPEC.md)，时序/输入归[INTERACTION](spec/INTERACTION_ANIMATION.md)，模板归[ANIMATION_PROMPTS](spec/ANIMATION_PROMPTS.md)，开源选型归[ANIMATION_LIBRARIES](spec/ANIMATION_LIBRARIES.md)。

Production baseline: 能用清晰图标表达就只放图标，不叠可见同义文字；可访问名称仍保留。只用统一规则形按钮、不用异形按钮，设置/菜单简洁。声音、游戏音乐、点击音效只有静音开关，不做音量滑条或百分比；必要编号/错误说明可用文字。

所有动画优先成熟开源库、单一主补间库，不重复手写基础设施；动画位置禁止用一张固定图片冒充完成。飞行有真正振翅/轨迹/收翅落脚，入巢有进入/遮挡阶段，结算有时间序列与粒子。静态背景可以静态；库许可不等于素材版权。角色/布局遵UI_SPEC §4.1及内部技术/独立视觉QA，原件保留；T21首关单上方接收巢/错落支撑枝已采纳，完整运行效果未验收。

## 6. AI阅读与交付

先[docs路由](README.md)→证据→任务唯一权威，不把旧档案当有效需求。T21首关实现已授权，在251开发、Mac测试，遵AGENTS内部资源/审核门槛；普通决定不再等待用户重复确认。其余未采纳提案不因此转为规则或开发任务。

交付应写：有效需求和同行出处、明确迁移差异、未证/撤回项、新旧证据失效、实际验证命令、失败/未知、固定关卡附录、草稿PR SHA。最终游戏/真人/设备边界见[VALIDATION](spec/VALIDATION.md)。

## 7. Current design-exploration exception

The user’s 2026-11-01 direction permits design drafts to depart from existing UI constraints and use necessary functional text. Show actual prices, inventory, levels, purchase/upgrade/equip and reward states rather than decorative placeholder panels. Do not require a thick wooden button base to explore a design. This is a scoped exploration exception: existing originals, safe interaction, final visual selection and accepted slices still gate production implementation. A side-trunk receiving area is proposed; exact coordinates/occlusion assets and final visual choice remain pending. The existing upper-nest T21 slice is not erased or retrospectively declared unimplemented.
