# 分类整理益智：同行规则文字手册

2026-10-31联网复核。Sort Puzzle / Sorting Puzzle即分类整理益智；Water Sort倒液体，Ball Sort移球，Bird Sort移鸟，Nut Sort移螺母。主要共享“取外露同类→空/同类接收→容量规划”的循环，不等于三消或按大小排序。

**本页不用打开外链也能读懂玩法。** 商店/官网是开发者第一方陈述，未安装实测；没有写出的算法不靠常识补成事实。具体需求的采纳/撤回和出处键见[完整证据矩阵](MECHANIC_EVIDENCE.md)，当前唯一规则见[RULES](spec/RULES.md)。自拟局部例子只解释规则，非竞品原关卡或完整解题证明。旧“凭合理性补创新”研究结论已撤回。

## SRC-LEAF：Bird Sort / Leafy Games

**原游戏文字规则**：先点要移动的鸟，再点目标枝。只能取外端，相连同色要一起搬；目标必须为空或外端同色，枝有有限容量。少步得高分；卡住可回退或重开；无倒计时，提供鸟、枝和背景外观解锁。

**例子（自拟）**：外端A2挡着B，先给A2找到空枝或A端，才能露B。资料未讲余量只够一只时截断还是拒绝，不能从“整群”推定该边界。

**我方采用/差异**：采用点选、端部群和空/同类端；容量截断另由SRC-SIMPLE支持。记录最佳操作数有依据，但没有依据写Undo扣星公式。外观功能存在不证明价格/奖励数量。

定位：About→HOW TO PLAY/FEATURES。[第一方出处](https://play.google.com/store/apps/details?id=com.leaf.birdsort&hl=en)。

## SRC-SIMPLE：Simple Games / Water Sort

**原游戏文字规则**：每管4单位、每色4单位，颜色数K加两空管。点来源再目标，非空来源可把顶层连续同色尽量倒到空或同色顶的有余量目标，放不下的留源；不合法点击不执行、不惩罚。一次倒算一步，完成时各管为空或满管单色。每关由同一seed复现，先找到完整解才出题。

**帮助/终局**：免费不限次Undo回退一次倒水及步数；Hint给当前已找到解的首步，不自动倒。搜索没找到不等于证明死局。没有额外空管购买。

**我方采用/差异**：移植min(run,free)、一步计数、免费帮助、固定同题与可解验证；本项目固定策划数组而非运行时生成。4为直接容量依据；6/8来自SRC-BOOP。完成后失枝来自SRC-BIRD2，不声称此水游戏也失管。

定位：Pouring/The tubes/Moves/Questions。[开发者规则说明](https://pixapps.ai/simple-games/games/water-sort/en/)。

## SRC-QUEST：Bird Sort Quest / mobirix

**原游戏文字规则**：触鸟令其到另一枝，只能移动枝末鸟，目标有空间，空枝可接。把同种聚枝；鸟前方位置移空后，问号蛋孵化。可重开；有Undo与额外枝工具、鸟/枝/背景外观、日任务金币和排名。

**例子（自拟）**：可动B挡住?蛋，B离开后揭示下一鸟；揭示前不能假定它是当前想要的A。

**我方采用/差异**：采用前鸟移开揭蛋；蛋的隐藏身份按用户要求固定于关卡，不重开抽种。原文没有“蛋壳预告物种纹样”，旧已知色蛋撤回。加枝功能明确，但几座/几次/价格未公开，不能补数。连蛋及和完成退枝组合需独立验证；不声称安装实测已过。

定位：How to Play/Features。[第一方出处](https://play.google.com/store/apps/details?id=com.mobirix.ldbis&hl=en)。

## SRC-BIRD2：Bird Sort 2 / FALCON

**原游戏文字规则**：点鸟再点枝，同类聚集。整理好一群让其飞走后，会失去该枝，不能再拿它做下一次整理。官方还列额外枝、逐步返回、洗牌、破同色及计时冻结工具，并有时间/生命限制模式；声称关卡不用道具也可过。

**例子（自拟）**：完成A群同时减少一个可用枝，下一群必须利用剩余位置；不能把空掉的完成枝当新缓冲。

**我方采用/差异**：仅采用“完成群→飞走→该枝不可用”。不采原自拟“接过鸟后，任何清空即收枝”；普通转出至空不等于完成。容量/群大小没有在该页定数，我方4/6/8另有出处。时间/生命/洗牌不因采用失枝自动进入主线。

定位：HOW TO PLAY第2条。[第一方出处](https://play.google.com/store/apps/details?id=com.globalplay.birdsort2.color.puzzle&hl=en-US)。

## SRC-BOOP：Water Sort / BOOPFUN

**原游戏文字规则**：点瓶再点另一瓶，空瓶或同色顶、有余量方可倒，填成单色完成。有6/8层长瓶、问号隐藏色和只进不出的收集瓶；支持Undo/重开/提示，备用瓶可反复触控变高。离线不限时；官方也说明广告及补充Undo的广告方式。

**例子（自拟）**：收集瓶接受A后不能把A倒回，不同于普通周转瓶；备用容器变高改变局内可用空间。

**我方采用/差异**：6/8作为容量来源；加空间/逐次扩高是有据功能，精确初高、步长、上限、次数、费用仍待证。固定初盘不禁止局内扩展。收集容器与扩容参数未明的部分不进正式40关数值配置，不自编广告/金币救援。

定位：HOW TO PLAY/WHAT'S INSIDE。[第一方出处](https://play.google.com/store/apps/details?id=com.boopfun.watersort)。

## SRC-CUTIE：Cutie Sort / Easetouch

**官方文字明确**：点猫放另一开放架，按色聚架，整理全部完成；存在额外层、箱子、撤销、交换、扩容。文案不含双目标激活数、睡眠冻结、每组六只、累计/指定种箱公式或扩容数字。

**旧录像记录与本轮边界**：前轮视频记录过六猫、顶部目标刷新、趴睡、箱和篮；本轮没有重验可定位时间与完整边界。记录不是没有价值，但不能用它补写一套精确公式并声称联网已证。尤其睡眠是否冻结、哪次完成开哪个箱，需要对应实录与反例。

**我方决定**：保留研究事实，双目标/睡眠/精确条件箱/单鸟篮从本轮有效规则隔离。Swap仅有名称不说明交换对象；expand仅说明存在，不编格数。不能为保旧40关继续沿用无来源边界。

定位：How to Play/Key Features。[官方](https://play.google.com/store/apps/details?id=sort.casual.color.cat.puzzle.match&hl=en)；旧参考[视频1](https://www.youtube.com/watch?v=fZHsuXpTwEE)/[视频2](https://www.youtube.com/watch?v=onptn300img)。

## SRC-NUT：Nut Sort Mania / Gameware

**原游戏文字规则**：顶部同色螺母移到空杆或同顶色杆；短、中、长杆改变空间。可回退/提示/加杆，官方称无隐藏随机碰运气、全关不需道具可解，有诊断与日挑战；也有短时间挑战模式。

**我方采用/差异**：借鉴容量与无需道具验证的方向；具体4/5短枝不是该页给出的数字。当前40关采用同关统一容量，不因异长度有依据就编一套五座枝规则。智能诊断的算法/收费/预算没公开，不能保证任意状态即时正确。

定位：Features/How to play/Fair Play/Smart Assist。[开发者页面](https://www.gamewareeurope.com/Public/games?id=nutsort)。

## 其他已核系统（非全部进入首版）

| 来源 | 原游戏文字可确认内容 | 本项目处理/未知 |
| --- | --- | --- |
| SRC-GURU：[Guru Ball Sort](https://play.google.com/store/apps/details?id=ball.sort.puzzle.color.sorting.bubble.games&hl=en) | 顶球同色接收且有空间，4球瓶，Undo/加瓶/重开，离线不限时 | 容量/帮助参考；不从单球文案推全部模式永远逐球；费用未证 |
| SRC-MAGIC：[Magic Water](https://play.google.com/store/apps/details?id=com.magic.relaxing.water.sort.colorgame.puzzle&hl=en) | 遮布/冰块/锁柜/卷帘/问号水；地图推进、壁纸、动作音效；洗牌/加管和活动 | 地图可作为用户所要世界地图的同类依据；机关精确触发/价格不明，不能补创；不采用体力转盘 |
| SRC-AZURA：[Azura Bird Sort](https://play.google.com/store/apps/details?id=com.birdsort.color.sortpuzzle&hl=en) | 回退/重开/加枝，少步整理，过关金币解锁物品与背景，增加颜色/鸟数 | 成长系统存在有据，金币数/售价/解锁表待证；不编经济曲线 |
| SRC-TVC：[TVC Bird Sort](https://play.google.com/store/apps/details?id=com.birdsort.colorpuzzle&hl=en) | 鸟种与枝数随关增加 | 只支持跨关难度维度，不证明单纯加鸟即可好玩 |

## 世界地图、成长和动画要求的证据归属

用户要求世界地图和逐世界解锁，这是USER_REQUIREMENT；SRC-SIMPLE也有顺序关卡解锁，SRC-MAGIC有冒险地图，[Bluebook的Magic Sort](https://apps.apple.com/gb/app/magic-sort-water-sorting/id6481261917)介绍明确排序并建设/探索不同主题城镇。Bluebook产品不可误认成GrandGames同名游戏。四个区域01–08/09–18/19–28/29–40是本项目暂定配置，不是竞品原值；不自行加星门槛、奖励数或金币建设公式。

[Splashscreen Bird Sort](https://splashscreen.games/game/bird-sort)为父任务已核的主题路线线索，本子任务直接复取被限制，不作关键规则唯一依据。用户另要求动画开源优先、静态图不得冒充运行动画；这是交付/技术偏好，动画库许可和兼容性另文核查，不是新玩法。

## 研究到需求的使用规矩

1. 所有玩法都要有证据键和采用差异；“有某个机关”不等于知道它全部公式。无法证实的细节移出有效需求。
2. 固定40关是用户约束；关卡布局是本项目策划内容，规则来自以上同行组合。组合是否可玩仍需新规则回放与真人验收，不能拿旧60条路径背书。
3. 用户地图、图标不重复文字、规则形按钮、声音/音乐/点击仅静音开关属于USER；稳定ID、原子存档、输入锁/路径/pivot与时长属于工程/表现合同，不冒称同行玩法参数。
4. 不重画或复制同行美术、不搬原作关卡；本次只整理文档，不实施代码、运行数据或素材。
