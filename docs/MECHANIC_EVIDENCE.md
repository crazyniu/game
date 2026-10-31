# 玩法与系统证据矩阵（有效需求的证据闸门）

核查：2026-10-31。本次为联网第一方文字核查；未安装这些产品、未重播完整录像或测量动画。当前规则以[RULES](spec/RULES.md)为准；本页拥有出处/证据资格，不另定义算法。所有旧关卡/旧代码都不能反过来证明同行做法。

## 1. 证据与决定的含义

- `P`：开发者维护商店/官网明确文字。只证明其陈述，非独立实测。定位采用页面标题+小节+关键词，链接不是离线阅读前提。
- `O`：前轮已观察记录，本轮未重验、缺可定位操作时间；不能补全未测边界。
- `E`：本项目工程协议（ID、存档、输入锁等），不声称来自竞品玩法；用于可靠实现已选规则。
- `USER`：用户明确约束，如固定初盘、只文档、鸟主题。
- `可采用`：有明确玩法依据，最终采用版本和组合由RULES拥有；不等于允许本任务实现。
- `隔离待证`：功能存在或旧观察不足以定义精确规则，退出当前可执行需求；禁止AI凭名字补公式/费用。
- `撤回`：本轮未找到依据的原自拟玩法/政策，不再作为有效需求或未来默认候选。
- 关卡具体数组/参数是我方固定策划内容，不是声称复制同行关卡；其使用的规则必须逐项有来源。适配题材/单位与工程参数必须明示，不能伪称竞品原值。

## 2. 第一方来源登记

| 来源ID | 产品、可访问出处与定位 | 本轮明确读到的内容 | 未公开的边界 |
| --- | --- | --- | --- |
| SRC-LEAF | [Leafy Bird Sort](https://play.google.com/store/apps/details?id=com.leaf.birdsort&hl=en)，About→HOW TO PLAY | 先点鸟再点枝；只取外侧；同色相连一起搬；空枝或外端同色接收且受容量约束；少步得高分；Undo/重开；外观解锁；不限时 | 批量遇余量不足如何截断、所有容量、评分公式 |
| SRC-QUEST | [mobirix Bird Sort Quest](https://play.google.com/store/apps/details?id=com.mobirix.ldbis&hl=en)，How to Play/Features | 端部移动、目标须有空间、空枝可接；前鸟移开后问号蛋孵化；Undo/加额外枝；鸟/枝/背景外观；日任务金币、排行榜 | 蛋身份如何决定、连蛋细节、额外枝容量/次数/费用、Undo额度 |
| SRC-BOOP | [BOOPFUN Water Sort](https://play.google.com/store/apps/details?id=com.boopfun.watersort)，HOW TO PLAY/WHAT'S INSIDE | 空/同色顶且有余量可倒；6或8层容器；隐藏色；只进不出的收集瓶；备用瓶可逐次加高；Undo/重开/提示；离线不限时 | 每次加高几格、初始高度、上限、成本；收集瓶颜色绑定/满后反馈 |
| SRC-BIRD2 | [Bird Sort 2 / FALCON](https://play.google.com/store/apps/details?id=com.globalplay.birdsort2.color.puzzle&hl=en-US)，HOW TO PLAY | 完成鸟群后飞走并失去该枝；额外枝、撤回、洗牌、破同色规则、暂停计时等道具；宣称无需道具可过 | 鸟数/容量、退场动画和触发精确帧、时间/生命活动参数 |
| SRC-CUTIE | [Cutie Sort / Easetouch](https://play.google.com/store/apps/details?id=sort.casual.color.cat.puzzle.match&hl=en)，How to Play/Key Features | 同色整理架子，额外层、箱、undo/swap/expand，全部归类完成；无计时/运气定位 | 双目标激活/睡眠冻结、每组六只、完成数/指定色箱公式、扩容数字、交换对象 |
| SRC-SIMPLE | [Simple Games Water Sort](https://pixapps.ai/simple-games/games/water-sort/en/)，Pouring/Moves, Undo and Hint | 连续顶层同色尽量搬至余量、余下留源；一次倒计一步；免费不限次Undo/Hint；同关同seed同盘；先证可解；搜索未找到不冒报无解 | 属水排序，不是鸟产品；4层/两空管是该产品配置，不能说所有同行相同 |
| SRC-NUT | [Gameware Nut Sort Mania](https://www.gamewareeurope.com/Public/games?id=nutsort)，Features/How to play/Fair Play/Smart Assist | 长短中杆、顶色至空/同色杆；Undo/Hint/加杆；声称不靠道具可解、无隐藏随机碰运气；解局诊断、日挑战 | 各长度精确数字、智能诊断预算、帮助费用 |
| SRC-GURU | [Guru Ball Sort](https://play.google.com/store/apps/details?id=ball.sort.puzzle.color.sorting.bubble.games&hl=en)，HOW TO PLAY/KEY FEATURES | 每瓶4球，顶部球、同色接收且有空间，Undo/加瓶/重开，离线不限时 | 实际群搬算法、额度/商业化细节 |
| SRC-MAGIC | [Gaming Laboratory Magic Water](https://play.google.com/store/apps/details?id=com.magic.relaxing.water.sort.colorgame.puzzle&hl=en)，Challenges/Events | 遮布/冰块/锁柜/卷帘/问号水、Undo/Shuffle/加管；地图/壁纸；活动/体力；分动作声音 | 解冰、钥匙、卷帘/标签的准确触发均未给出 |
| SRC-AZURA | [Azura Bird Sort - Color Puzzle](https://play.google.com/store/apps/details?id=com.birdsort.color.sortpuzzle&hl=en)，HOW TO PLAY/GAME FEATURES | 触鸟再触枝、少步、回退/重开/加枝；过关金币解锁物品和背景；鸟色/数量递增 | 金币数量、价格、解锁表、广告费用 |
| SRC-TVC | [TVC Bird Sort - Color Birds Game](https://play.google.com/store/apps/details?id=com.birdsort.colorpuzzle&hl=en)，About | 同类填枝，鸟种和枝数增加带来难度变化 | 精确增长曲线及每关配置 |
| SRC-BLUEBOOK | [Magic Sort / Bluebook](https://apps.apple.com/gb/app/magic-sort-water-sorting/id6481261917)，产品介绍 | 排序液体、建设/探索不同主题城镇 | 不是GrandGames同名产品；不证明我方四区数量/世界通关解锁条件 |
| SRC-SPLASH | [Splashscreen Bird Sort](https://splashscreen.games/game/bird-sort)，父任务已核路线/主题地图 | 本轮子任务复取受URL限制，仅作补充线索 | 不作当前关键规则唯一证据，不能说本轮子任务已复核 |
| SRC-WSWEB | [WaterSort.in浏览器版](https://www.watersort.in/play)，Objective/Controls/Android区别 | 网页自身说明：普通容器四单位、每关可加一个空容器、重开重建同题；公开免费浏览器产品 | 未操作实测；不推定Galaxy Android相同规则；Undo加枝/计步/存档未公开，属我方E集成；免费帮助是项目范围配置 |
| SRC-OBS-CUTIE | [前轮Cutie视频1](https://www.youtube.com/watch?v=fZHsuXpTwEE)、[视频2](https://www.youtube.com/watch?v=onptn300img) | O：此前记录有6只/目标刷新/睡眠/箱/篮可见行为 | 本轮未获取可定位时间和完整边界复测，不能作为本轮精确合同已核证明 |

## 3. 全量盘点：基础与特殊玩法

| 项ID / 原有项 | 明确同行规则与证据 | 本项目采用范围或修订决定 | 差异/缺口及禁止推断 |
| --- | --- | --- | --- |
| M01 鸟主题归类 | SRC-LEAF/SRC-QUEST/P同类鸟整理和飞行 | 可采用；小鸟归巢主题，USER | 鸟巢美术表达不自动导出目标队列 |
| M02 点来源再目标 | SRC-LEAF/P先触鸟后触枝 | 可采用相同操作 | 取消选中/非法反馈为E，不增额外规则 |
| M03 外端run联动 | SRC-LEAF/P外侧同色相连同搬 | 可采用 | 不能抽内部、跨异种抽同类 |
| M04 空/同色端接收 | SRC-LEAF/P空枝或外端同色 | 可采用 | 不引破同色道具作为普通规则 |
| M05 容量不足部分搬移 | SRC-SIMPLE/P连续顶层仅搬装得下部分 | 可采用时标明水→鸟移植 | SRC-LEAF未明确该边界，不能归因Leaf |
| M06 一操作计一步 | SRC-SIMPLE/P一次倒无论数量计1 | 可采用等价群搬操作计数 | 动画每只起飞不独立计步 |
| M07 非法动作零效果 | SRC-SIMPLE/P不符条件不执行不处罚 | 可采用 | 选中回落属于E；不扣资源 |
| M08 固定初盘/重开相同 | USER固定；SRC-SIMPLE/P同关seed复现 | 必须固定资源，不运行时随机填充 | 我们不用其生成器；固定不等于禁止局内加空间 |
| M09 六只满组归巢 | SRC-BOOP/P六层、SRC-OBS-CUTIE/O六猫；SRC-QUEST/P同鸟飞走 | 六作为容量/每种数量的题材适配可声明；不能声称SRC-QUEST每组就是6 | 精确归巢/完成合同须明示采用源组合，不捏造原作统一数字 |
| M10 双激活目标队列 | SRC-OBS-CUTIE/O观察；SRC-CUTIE/P未说明算法 | 隔离待证，退出当前有效规则 | 不保留“前二未收集”“可先第二”等自补边界 |
| M11 非目标睡眠/冻结 | SRC-OBS-CUTIE/O可见睡眠记录 | 隔离待证，不能作为当前精确规则 | 闭眼装饰与规则冻结必须区分；不可凭动画推断 |
| M12 队列刷新自动连收 | SRC-OBS-CUTIE/O | 隔离待证；随目标队列一起移出 | 不保留固定下标扫描作为未经证实的玩法 |
| M13 计数箱RevealAfter | SRC-CUTIE/P仅有箱；SRC-OBS-CUTIE/O满组关联 | 隔离待证，退出当前有效条件 | 无法证明累计/本轮、睡眠计数、阈值1/2 |
| M14 指定种箱RevealSpecies | SRC-OBS-CUTIE/O有色箱相关记录 | 隔离待证 | 无法证明睡眠或归巢何时触发、双箱批次公式 |
| M15 额外层/箱存在 | SRC-CUTIE/P明确存在 | 研究保留，机制细节待证后再采用 | “有箱”不是任意锁箱条件授权 |
| M16 问号蛋揭示 | SRC-QUEST/P前方鸟移开后?蛋孵化 | 可采用隐藏物种的前鸟揭示；固定内容由USER约束 | 不随机重抽；确切连续蛋/遮盖交互需合同审查或隔离 |
| M17 已知色/纹样蛋 | SRC-QUEST/P是问号蛋，不是预告种类 | 撤回壳纹提前公开物种玩法 | 不能把减少盲猜的偏好写成同行事实 |
| M18 原临时枝任意清空退役 | 无已核同行支持 | 撤回Unused/Armed/Retired规则 | BirdSort2不能证明首次占用后清空即退 |
| M19 完成群后该枝消失 | SRC-BIRD2/P先完成群飞走再失枝 | 可采用完成枝退场 | 不等于部分转出/最后一鸟转出时退场 |
| M20 普通长短枝 | SRC-NUT/P异长度杆，SRC-GURU/P4、SRC-BOOP/P6/8容量 | 不同关统一4/6/8容量可采用；同一关异长度短枝不进入本轮有效40关 | 旧五座无精确来源；不把我方5座说成已核同行值 |
| M21 单鸟篮0–2、容量1 | SRC-OBS-CUTIE/O有篮记录 | 隔离精确数量/容量，移出当前合同 | S05官方未定义篮，不凭图形推出每格1只 |
| M22 额外普通枝 | SRC-QUEST/SRC-BIRD2/SRC-AZURA/P有加枝，SRC-WSWEB/P明确每关一空容器 | 可采用R07：免费一条本关N座普通空枝；当前路径最多一条 | 水→鸟普通容量迁移，N是项目配置非来源鸟座位数；Undo/保存/成绩分类为E，不外推Android收费或算法 |
| M23 备用瓶逐次加高 | SRC-BOOP/P反复触控备用瓶使其变高 | 功能保留在有来源待参数化区 | 不编初始1座、每次+1、最多6、金币/广告成本 |
| M24 只进不出收集容器 | SRC-BOOP/P只可倒入不可倒出 | 有来源候选；未核细节不进正式40关 | 非普通满组锁；种类绑定/完成清空未知 |
| M25 隐藏色层 | SRC-BOOP/SRC-MAGIC/P问号/隐藏 | 可参考信息揭示，不等于纹样蛋 | 提示提前看种/窥视道具没有依据 |
| M26 Swap交换 | SRC-CUTIE/P有swap名称 | 隔离，不能只凭名称实现任意交换 | 交换两鸟/两整枝/不限颜色均未知 |
| M27 Shuffle洗牌 | SRC-BIRD2/SRC-MAGIC/P有洗牌 | 未采用；与固定关卡用户要求区分初盘和局内 | 有依据不强制全采；不加进正式策略 |
| M28 破同色、计时冻结/生命 | SRC-BIRD2/P有这些工具和限制 | 未采用；当前只取排序/完成退枝方向 | 不创造我们自己的工具效果/资源表 |

## 4. 全量盘点：辅助、成长、表现与工程

| 项ID / 原有项 | 同行依据 | 本项目决定 | 差异/缺口 |
| --- | --- | --- | --- |
| M29 Undo/Restart | SRC-LEAF/SRC-QUEST/P支持；SRC-SIMPLE/P免费无限Undo | 可采用免费回退/重开 | 不将永久Hint额度、Undo扣星偷偷绑定 |
| M30 提示 | SRC-SIMPLE/P免费不限次，显示当前可解路径首步；SRC-NUT/P解局诊断 | 可采用求解路径首步，不自动执行 | 找到可解路径不必最优；预算超限是未知 |
| M31 每关终身3次提示 | 无已核依据；旧代码不是同行证据 | 撤回固定3次政策 | 不用旧UI角标证明同行如此 |
| M32 Undo+Restart扣三星 | SRC-LEAF/P只说少步高分 | 撤回0次3星/1–3两星/≥4一星公式 | Undo次数与操作效率不是同一指标 |
| M33 少步成绩 | SRC-LEAF/SRC-AZURA/P少步得高分/少步排序 | 可采用记录最佳操作数 | 不编星阈值、最优比率或奖励倍率 |
| M34 鸟/枝/背景外观 | SRC-LEAF/SRC-QUEST/P外观解锁/切换 | 有来源功能保留在范围评审 | 不默认无限解锁或每关奖励具体数量 |
| M35 金币过关成长 | SRC-AZURA/P通关金币解锁物品/背景 | 有来源、待完整经济证据；非凭空创造新鱼币 | 金币数量/售价/掉落曲线未公开，退出可执行参数 |
| M36 日任务/排名/活动 | SRC-QUEST/P有日任务金币/竞争，SRC-NUT/P日挑战 | 有依据但非当前固定40关核心 | 不编重置时区、奖励表、网络同步合同 |
| M37 无广告/IAP/体力 | 项目范围约束；SRC-SIMPLE/P无付费帮助，SRC-QUEST/SRC-BIRD2等实有广告/IAP | 可选择不采商业化，必须明示是我方范围 | 有同行广告不要求照搬；不能声称全部竞品无广告 |
| M38 不限时、无步数失败 | SRC-LEAF/SRC-QUEST/P无时间限制；SRC-BIRD2/P无步数上限但有时间/生命模式 | 可采用低压力主线 | 同类不同模式不能互相覆盖 |
| M39 动态增加鸟种/枝数 | SRC-AZURA/SRC-TVC/P随进度增加 | 可用于跨关固定策划；默认K+2枝/两空来自SRC-SIMPLE，移植到6/8座关是我方明确组合配置 | 不运行时按表现随机调盘；具体40数组是本项目自编内容 |
| M40 前5教学/每5舒缓/最多2特殊类 | 旧设计策划策略非同行已核公式 | 仅作为我方内容组织/验收目标，非玩法规则 | 不声称同行统一每5关舒缓或实测有效 |
| M41 主动放飞+风闸 | 无本轮排序同行依据 | 撤回有效/可选需求 | 不保留模糊未来入口让AI顺手实现 |
| M42 三同清除 | Goods邻近三消；本轮EXO正文复取失败 | 不采用，移出核心 | 不将鸟排序混三消；失败链接不作关键证据 |
| M43 声音/群飞/庆祝 | SRC-LEAF/SRC-MAGIC/P有视听反馈，SRC-BIRD2/P群飞 | 可保留表现方向 | 毫秒/pivot/路径是工程调参，非竞品测量；不可改变完成条件 |
| M44 按钮尺寸/减动效/安全区 | 项目现UI+平台规范，属于E | 保留已定UI与可达性合同 | 不把这些包装成新增玩法；不因改文档重画UI |
| M45 存档/Undo整事务/输入锁/稳定ID/hash | E工程可靠性要求 | 保留并随已选规则更新 | 原子存档与并发序号无须伪造同行来源；它们不改变游戏目标 |
| M46 求解证书/Unknown | SRC-SIMPLE/P先可解才出题、找不到不报已无解；E严谨验证 | 保留证明状态/回放/未测边界 | 独立模型、最优证明和Unity实机是不同证据 |
| M48 世界地图/顺序解锁 | USER要世界地图/世界顺序解锁；SRC-MAGIC/P过关推进冒险地图，SRC-BLUEBOOK/P不同主题城镇，SRC-SIMPLE/P顺序过关 | 可采用地图导航与顺序解锁，不设星门槛；区域01–08/09–18/19–28/29–40是我方暂定编排 | 不能声称同行使用这四区或相同数量；不引未证奖励经济 |
| M49 图标/形状/设置 | USER图标不重复可见文字、规则形按钮、音乐/声音/点击仅静音开关 | 按用户UI硬约束整理，非新玩法 | 无滑条、百分比；工程尺寸归UI_SPEC，不归竞品规则 |
| M50 动画开源优先/不可静图冒运行 | USER要求，属于交付/技术偏好；非同行玩法参数 | 新动画须真实可播放，优先核许可明确的开源方案 | 不在本任务执行动画/生成素材；库兼容/许可证据由动画文档单独拥有 |
| M47 固定40关/各关不同 | USER固定/多样要求；SRC-AZURA/SRC-TVC逐级变化 | 按已证规则重做内容与回放 | 不为保40旧数组保留无依据机制，不混入运行JSON |

## 5. 原需求退出与验收门槛

1. 撤回M17/M18/M31/M32/M41；目标队列、睡眠、精确条件箱及单鸟篮M10–14/M21隔离。旧60条回放曾在旧规则下通过，不再证明修订规则，受影响旧报告/附录必须标历史失效。
2. 普通加枝已由SRC-WSWEB及RULES R07闭环；加高、收集容器、成长等“功能有来源、具体参数未证”必须分开：固定初盘不禁止局内增加空间，也不能凭一行商店介绍编价格/次数/格数。只有参数/触发足够明确后才能进入可执行合同。
3. 每个有效玩法字段须能回指本矩阵条目和可访问第一方原文。工程协议标E、用户约束标USER，允许可靠实施；不得把任何工程便利变成新的通关/惩罚机制。
4. 本页不是给AI补空白的许可。若RULES仍出现无出处算法，先停相关规则，其他已证模块可继续；本任务仍只改文档，不执行游戏实现/素材制作。

## 6. 本轮规则选择回执

RULES v4采用：SRC-LEAF端部同类接收；SRC-SIMPLE的min(run,free)、一次移动一步、免费不限次Undo/Hint、默认K+2/两空和无惩罚；SRC-BIRD2完成群飞走失枝；SRC-QUEST问号蛋前鸟移开孵化。每关统一N取4/6/8是把SRC-SIMPLE四单位与SRC-BOOP六/八层移植为栖位容量，按同类整理完成同一物种；不是声称某个竞品同时使用这些全部规则。蛋固定数据遵USER，对玩家隐藏。事务先锁run/Count、Hatch后不追飞是E实现协议，不改变来源允许搬移量。未采用双目标/睡眠/条件箱/篮/同关短枝/原临时枝。普通加枝采用SRC-WSWEB同类机制并由R07完整定义；逐次加高/收集容器仍待证，不能把未定值写进固定40关。

本版正式配置禁止相邻egg：其原作精确孵化边界尚未核实，不以E协议名义加入自拟玩法。已根据此限制移除L21的相邻蛋并重新回放；旧相邻蛋见证不复用。所有世界仅为不同森林，林型只视觉，不增加天气/环境规则。

## 7. 普通加枝补证回执

本轮追加直接读取SRC-WSWEB的浏览器玩法文字，不声称已安装或操作验证；网站明确浏览器版与Android为不同产品，品牌归属不用于额外背书。M22现为有据机制的明确迁移：新增一个普通空容器→一条本关N座普通枝；免费、Undo事务、保存和成绩分组分别标项目范围/E。合理的容量适配和可靠性协议无需冒称竞品原值，也不应因商店未公开源码而把整个功能永久挂起。其余M23/M24/M35未获对应精确依据，仍不激活。
