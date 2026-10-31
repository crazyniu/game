# 同行依据版数据、稳定事务与存档

schema4 / rules4 / save6（设计目标，未实施）· 2026-10-31。玩法权威RULES，出处MECHANIC_EVIDENCE，世界归MAP_PROGRESSION。本页定义E工程可靠性协议；它们不冒充竞品源码，也不增加奖励/通关条件。

## 1. 身份与静态格式

| 字段 | 类型/约束 |
| --- | --- |
| schemaVersion / rulesVersion | 整数4 / 4；旧schema3/rules3不可按缺省4读 |
| campaignId | `bird-home-peer-v4`，与旧`perch-sort-v2`/T16`bird-home-v3`隔离 |
| catalogRevision | 根目录元数据，正整数递增；不入单关内容hash |
| levelId / worldId | 1..40 / A,B,C,D，精确映射由MAP_PROGRESSION拥有 |
| flockSize | N∈{4,6,8}，同关所有枝容量及每种总数=N，无单枝capacity覆盖 |
| shelves | 固定数组P=K+2，K=3..8、P≤10，ID严格S1..SP；初态两空，无已完成群/露端蛋/相邻蛋 |
| shelves[].side | `left|right`，只影响显示；S奇数left、偶数right，行=floor((编号-1)/2) |
| shelves[].tokens | 墙→露端；Token数组长度≤N；含蛋，每种0..K-1恰N |
| Token.id | 关内唯一ASCII稳定ID，正式候选按`Lnn-Sj-kk`从初始展开序号分配；孵化/移动/Undo不改ID |
| Token.species / form | 0..K-1整数 / `bird|egg`；egg身份固定但显示层不得提前公开 |
| contentHash / proof | 导出元数据，不入自身hash；无最优证明不填solverMinMoves，使用null和明确proofStatus |

标识符匹配`[A-Za-z0-9][A-Za-z0-9._-]{0,63}`。按固定数组下标，不按S10/S2词典排序。关卡初始枝均Available，退役是运行态，不写初始Retired。没有targets/Asleep/CompletedCount/crate/baskets/temporary字段；旧字段、同关额外capacity、未知字段全部拒绝，不能静默忽略后当合法新关。

基础40不含ExtraBranch/ReserveGrow动作或伪造数值字段。它们有功能证据但仍待参数，今后激活须新schema/配置+可回放事务，不能把当前schema没有字段解释成产品永远禁止扩容。所有初态固定，不能在加载时随机补空数组或调用AI生成。

## 2. 规范hash与新测试向量

先校验完整required字段、类型、ID/数量/世界归属，再取固定hash根键`schemaVersion,rulesVersion,campaignId,levelId,worldId,flockSize,shelves`；每枝恰`id,side,tokens`，每token恰`id,species,form`。本版不允许省略这些必填项，不保留旧temporary/crate/baskets默认规则。元数据catalogRevision/contentHash/proofStatus/solverMinMoves只在明确导出元数据区，不参与逻辑正文哈希。

对象键ASCII升序，无空白/换行/BOM，UTF-8；整数十进制无前导零/负零/小数/指数；枚举/ID只ASCII，布尔/null按JSON小写；数组保留固定顺序。对规范字节SHA-256为小写64hex。物种/枝置换去重属于离线内容审查，绝不排序运行数组来哈希。

以下为**H04序列化测试夹具**，不是正式L01，使用局部levelId=1；K3/N4/P5、各类4只、两空、无初始完成群；不带最优证书，不可据此发布。旧H01 1320字节向量只留T16档案，不适用于本schema。

```json
{
  "schemaVersion": 4,
  "rulesVersion": 4,
  "campaignId": "bird-home-peer-v4",
  "levelId": 1,
  "worldId": "A",
  "flockSize": 4,
  "shelves": [
    {
      "id": "S1",
      "side": "left",
      "tokens": [
        {
          "id": "H04-S1-01",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-02",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-03",
          "species": 0,
          "form": "bird"
        },
        {
          "id": "H04-S1-04",
          "species": 1,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S2",
      "side": "right",
      "tokens": [
        {
          "id": "H04-S2-01",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-02",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-03",
          "species": 1,
          "form": "bird"
        },
        {
          "id": "H04-S2-04",
          "species": 2,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S3",
      "side": "left",
      "tokens": [
        {
          "id": "H04-S3-01",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-02",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-03",
          "species": 2,
          "form": "bird"
        },
        {
          "id": "H04-S3-04",
          "species": 0,
          "form": "bird"
        }
      ]
    },
    {
      "id": "S4",
      "side": "right",
      "tokens": []
    },
    {
      "id": "S5",
      "side": "left",
      "tokens": []
    }
  ]
}
```

规范字节（代码块末尾换行不计）：

```text
{"campaignId":"bird-home-peer-v4","flockSize":4,"levelId":1,"rulesVersion":4,"schemaVersion":4,"shelves":[{"id":"S1","side":"left","tokens":[{"form":"bird","id":"H04-S1-01","species":0},{"form":"bird","id":"H04-S1-02","species":0},{"form":"bird","id":"H04-S1-03","species":0},{"form":"bird","id":"H04-S1-04","species":1}]},{"id":"S2","side":"right","tokens":[{"form":"bird","id":"H04-S2-01","species":1},{"form":"bird","id":"H04-S2-02","species":1},{"form":"bird","id":"H04-S2-03","species":1},{"form":"bird","id":"H04-S2-04","species":2}]},{"id":"S3","side":"left","tokens":[{"form":"bird","id":"H04-S3-01","species":2},{"form":"bird","id":"H04-S3-02","species":2},{"form":"bird","id":"H04-S3-03","species":2},{"form":"bird","id":"H04-S3-04","species":0}]},{"id":"S4","side":"right","tokens":[]},{"id":"S5","side":"left","tokens":[]}],"worldId":"A"}
```

字节数`850`；SHA-256 `9118d0cd992312c9fe385e100b4ece328efbb39d48f28752286dffa78cb3ec79`。交换同种不同token IDs的数组位置改变hash；删除form必填字段拒绝，不做猜测归一化。本轮只临时脚本重算，非项目序列化实现测试。

## 3. 运行快照和状态键

StableSnapshot包含：身份与contentHash、按原枝顺序的token数组、每枝Available/Retired、已Home种集合、操作数、Undo快照栈、已提交actionSequence、进度引用。Retired必须空且与对应Home事件一致；每种remaining+N×isHome=N。无Asleep/队列/箱/临时三态。Clone必须深拷贝所有可变数组，不共享鸟/壳对象。

状态键用于精确回放/提示：rules/schema/campaign/level/hash、token ID/形态/species与顺序、枝态、Home集合；提示身份再绑定actionSequence与viewGeneration。不能只比较物种而合并蛋形态/退役差异。操作数影响成绩但不改变合法动作，可从搜索等价键单独管理；若实现忽略token ID作求解压缩，必须给可恢复映射，不能改变精确回放键。

## 4. 稳定事务、原子提交和SaveError

Transfer输入→从旧StableSnapshot构造独立候选→RULES完整稳定化→生成有序事件/候选Undo/操作数→序列化候选→原子确认成功→替换内存权威快照→Presenting。**逻辑完整先提交，之后才演出**；动画不负责Home/Retire/解锁。

存档使用同目录临时文件，完成写入与校验后原子替换主档，维护上一兼容成功备份；需在目标平台验证真实原子边界。禁止逐字段覆盖主档。无法确认替换结果先读回核验身份/序号/候选hash，不能盲重做动作。

`candidateSequence=lastCommittedSequence+1`。未确认成功前不增加已提交序号。写失败进入SaveError，保留**同一不可变候选字节/同一序号/事件**：

1. 重试先读回核验，若主档已有同一序号及候选hash→承认已提交，仅接续表现，不重复记操作/奖励。
2. 确认尚未提交→用同一候选字节和同一序号重试，不重新跑规则/抽蛋/多加1步。再次失败仍SaveError且旧棋盘/历史/进度保持。
3. 读回结果不确定或身份冲突→保持只读错误态，不演出成功，不接受新动作。返回菜单可只读显示；退出保留待核验证据。确认未提交才可丢弃候选回旧稳定局。

正常后台/恢复：已提交则恢复完整稳定终态，不恢复半程Transform/协程；原子提交前强杀则读上一成功快照。选中只是视觉，不存；重新进入增viewGeneration，旧协程/Hint/音效回调无效，不可解锁新局。

Undo为一个新事务：恢复最近Transfer前快照，弹历史、恢复操作数，成功序号仍递增；失败保留Undo前局。Undo不会再执行完成退役规则，避免空枝误退。Restart为新事务，重建同hash固定初态、清本局历史/操作数；保留最佳成绩/通关/静音。没有Undo扣星或Hint消耗账本。

## 5. 世界进度与旧档处理

进度saveVersion6：completedLevelIds、bestMoveCountByLevel、currentLevel、可选seenWorldIntro、总静音设置；world/level解锁按MAP_PROGRESSION由已提交completed集合推导，不能与另存unlock布尔互相矛盾。当前局快照含完整Undo历史，退出后可继续无限撤销，不静默截断；压缩/delta必须证明等价恢复。

| 输入 | 唯一处理策略 |
| --- | --- |
| 实际旧进度version4/rules2 | 先保留原档只读副本，归档旧campaign成绩/关号/星/提示；新campaign从01开始，不回放From/To到新布局 |
| T16设计schema3/rules3/save5 | 尚未实施，若遇实验档隔离为legacy试验；不当真实用户迁移已验证，不自动映射新关 |
| 新campaign同level同hash | 恢复快照/Undo/操作数；目标世界归属检查一致 |
| 同campaign修订hash | 备份旧局及最佳历史并标oldHash；新布局重开当前已开放关，旧证书/Undo失效；提示无消耗无需额度迁移 |
| 未知version / 未知玩法字段 | 保留原件/备份并拒绝写入该档，不用默认值吞掉；显示可理解错误；不得覆盖唯一证据 |
| 主档坏/校验失败 | 尝试身份兼容且完整bak；无可用备份则保留坏档，安全开启对应新局并提示，不能混合半新半旧 |

旧campaign星级不折算新地图解锁，旧同号新关不视为已完成。存档路径/产品名/平台目录变化要未来授权实现时记录并验收，本轮未改路径或配置。
