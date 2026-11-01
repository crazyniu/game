> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

# 第21–30关：v4固定布局与独立纸面回放

2026-10-31 · FIXED_CANDIDATE_V4 / INDEPENDENT_REPLAY_PASS。**本页已完全替换旧R4内容；旧目标队列、睡眠、条件箱、纹样预告蛋、临时枝任意清空退役及篮/异容量配置均不适用。** 本页不是运行JSON、最优证书或Unity记录。

规则唯一归[RULES](../spec/RULES.md)，来源键见[MECHANIC_EVIDENCE](../MECHANIC_EVIDENCE.md)，课程归[LEVEL_DESIGN](../LEVEL_DESIGN.md)，证据边界见[LEVEL_SIMULATIONS](../LEVEL_SIMULATIONS.md)。

每关N为所有枝统一容量、每种总数N；P=K+2，开局恰两空枝。数组墙→外露端；普通字母是鸟。`?(A)`表示策划固定为A的蛋，**玩家只看见?，不能读出A**；表/回放展示的是全信息分析视图，不证明盲玩公平。所有初始数组、鸟种/形态/位置固定，无运行随机；v4正式初态禁止相邻蛋，未证实的连蛋精确孵化不属于有效玩法。普通搬空不退役；完整同种N群自动归巢并使所在枝Retired；不使用额外枝也能走下列路径。

## 关卡21

揭示后回流；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A · D3 · A2` | `A · D3 · A2` |
| S2 | 6 | `B · C · B · A3` | `B · C · B · A3` |
| S3 | 6 | `C5 · B` | `C5 · B` |
| S4 | 6 | `D2 · B2 · ?(D) · B` | `D2 · B2 · ? · B` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：13个初始连续段、1颗问号蛋。主路11步，仅存在性见证。首次归巢在第6步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S4→S5 B1（run=1，Count=1）；Hatch S4 → D | 无 | S1=[A · D3 · A2]；S2=[B · C · B · A3]；S3=[C5 · B]；S4=[D2 · B2 · D]；S5=[B]；S6=[∅] |
| 2 | Move S3→S5 B1（run=1，Count=1） | 无 | S1=[A · D3 · A2]；S2=[B · C · B · A3]；S3=[C5]；S4=[D2 · B2 · D]；S5=[B2]；S6=[∅] |
| 3 | Move S1→S6 A2（run=2，Count=2） | 无 | S1=[A · D3]；S2=[B · C · B · A3]；S3=[C5]；S4=[D2 · B2 · D]；S5=[B2]；S6=[A2] |
| 4 | Move S2→S6 A3（run=3，Count=3） | 无 | S1=[A · D3]；S2=[B · C · B]；S3=[C5]；S4=[D2 · B2 · D]；S5=[B2]；S6=[A5] |
| 5 | Move S2→S5 B1（run=1，Count=1） | 无 | S1=[A · D3]；S2=[B · C]；S3=[C5]；S4=[D2 · B2 · D]；S5=[B3]；S6=[A5] |
| 6 | Move S2→S3 C1（run=1，Count=1）；Home C@S3；Retire S3 | C | S1=[A · D3]；S2=[B]；S3=[∅][Retired]；S4=[D2 · B2 · D]；S5=[B3]；S6=[A5] |
| 7 | Move S2→S5 B1（run=1，Count=1） | C | S1=[A · D3]；S2=[∅]；S3=[∅][Retired]；S4=[D2 · B2 · D]；S5=[B4]；S6=[A5] |
| 8 | Move S4→S1 D1（run=1，Count=1） | C | S1=[A · D4]；S2=[∅]；S3=[∅][Retired]；S4=[D2 · B2]；S5=[B4]；S6=[A5] |
| 9 | Move S4→S5 B2（run=2，Count=2）；Home B@S5；Retire S5 | B,C | S1=[A · D4]；S2=[∅]；S3=[∅][Retired]；S4=[D2]；S5=[∅][Retired]；S6=[A5] |
| 10 | Move S1→S4 D4（run=4，Count=4）；Home D@S4；Retire S4 | B,C,D | S1=[A]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[A5] |
| 11 | Move S1→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A,B,C,D | S1=[∅]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡22

长群空位预算；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · D · C · D` | `A3 · D · C · D` |
| S2 | 6 | `A · B5` | `A · B5` |
| S3 | 6 | `D2 · C · D2 · A` | `D2 · C · D2 · A` |
| S4 | 6 | `C4 · A · B` | `C4 · A · B` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：13个初始连续段、0颗问号蛋。主路11步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S2→S5 B5（run=5，Count=5） | 无 | S1=[A3 · D · C · D]；S2=[A]；S3=[D2 · C · D2 · A]；S4=[C4 · A · B]；S5=[B5]；S6=[∅] |
| 2 | Move S3→S2 A1（run=1，Count=1） | 无 | S1=[A3 · D · C · D]；S2=[A2]；S3=[D2 · C · D2]；S4=[C4 · A · B]；S5=[B5]；S6=[∅] |
| 3 | Move S1→S3 D1（run=1，Count=1） | 无 | S1=[A3 · D · C]；S2=[A2]；S3=[D2 · C · D3]；S4=[C4 · A · B]；S5=[B5]；S6=[∅] |
| 4 | Move S4→S5 B1（run=1，Count=1）；Home B@S5；Retire S5 | B | S1=[A3 · D · C]；S2=[A2]；S3=[D2 · C · D3]；S4=[C4 · A]；S5=[∅][Retired]；S6=[∅] |
| 5 | Move S4→S2 A1（run=1，Count=1） | B | S1=[A3 · D · C]；S2=[A3]；S3=[D2 · C · D3]；S4=[C4]；S5=[∅][Retired]；S6=[∅] |
| 6 | Move S1→S4 C1（run=1，Count=1） | B | S1=[A3 · D]；S2=[A3]；S3=[D2 · C · D3]；S4=[C5]；S5=[∅][Retired]；S6=[∅] |
| 7 | Move S1→S6 D1（run=1，Count=1） | B | S1=[A3]；S2=[A3]；S3=[D2 · C · D3]；S4=[C5]；S5=[∅][Retired]；S6=[D] |
| 8 | Move S1→S2 A3（run=3，Count=3）；Home A@S2；Retire S2 | A,B | S1=[∅]；S2=[∅][Retired]；S3=[D2 · C · D3]；S4=[C5]；S5=[∅][Retired]；S6=[D] |
| 9 | Move S3→S6 D3（run=3，Count=3） | A,B | S1=[∅]；S2=[∅][Retired]；S3=[D2 · C]；S4=[C5]；S5=[∅][Retired]；S6=[D4] |
| 10 | Move S3→S4 C1（run=1，Count=1）；Home C@S4；Retire S4 | A,B,C | S1=[∅]；S2=[∅][Retired]；S3=[D2]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[D4] |
| 11 | Move S3→S6 D2（run=2，Count=2）；Home D@S6；Retire S6 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡23

长群单蛋；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · C · A2` | `A3 · C · A2` |
| S2 | 6 | `B2 · C · ?(D) · D · C` | `B2 · C · ? · D · C` |
| S3 | 6 | `D2 · A · C2 · B` | `D2 · A · C2 · B` |
| S4 | 6 | `B2 · C · B · D2` | `B2 · C · B · D2` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：16个初始连续段、1颗问号蛋。主路15步，仅存在性见证。首次归巢在第11步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第3步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A2（run=2，Count=2） | 无 | S1=[A3 · C]；S2=[B2 · C · ?(D) · D · C]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A2]；S6=[∅] |
| 2 | Move S2→S1 C1（run=1，Count=1） | 无 | S1=[A3 · C2]；S2=[B2 · C · ?(D) · D]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A2]；S6=[∅] |
| 3 | Move S2→S6 D1（run=1，Count=1）；Hatch S2 → D | 无 | S1=[A3 · C2]；S2=[B2 · C · D]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A2]；S6=[D] |
| 4 | Move S2→S6 D1（run=1，Count=1） | 无 | S1=[A3 · C2]；S2=[B2 · C]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A2]；S6=[D2] |
| 5 | Move S1→S2 C2（run=2，Count=2） | 无 | S1=[A3]；S2=[B2 · C3]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A2]；S6=[D2] |
| 6 | Move S1→S5 A3（run=3，Count=3） | 无 | S1=[∅]；S2=[B2 · C3]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B · D2]；S5=[A5]；S6=[D2] |
| 7 | Move S4→S6 D2（run=2，Count=2） | 无 | S1=[∅]；S2=[B2 · C3]；S3=[D2 · A · C2 · B]；S4=[B2 · C · B]；S5=[A5]；S6=[D4] |
| 8 | Move S3→S4 B1（run=1，Count=1） | 无 | S1=[∅]；S2=[B2 · C3]；S3=[D2 · A · C2]；S4=[B2 · C · B2]；S5=[A5]；S6=[D4] |
| 9 | Move S2→S1 C3（run=3，Count=3） | 无 | S1=[C3]；S2=[B2]；S3=[D2 · A · C2]；S4=[B2 · C · B2]；S5=[A5]；S6=[D4] |
| 10 | Move S3→S1 C2（run=2，Count=2） | 无 | S1=[C5]；S2=[B2]；S3=[D2 · A]；S4=[B2 · C · B2]；S5=[A5]；S6=[D4] |
| 11 | Move S3→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A | S1=[C5]；S2=[B2]；S3=[D2]；S4=[B2 · C · B2]；S5=[∅][Retired]；S6=[D4] |
| 12 | Move S3→S6 D2（run=2，Count=2）；Home D@S6；Retire S6 | A,D | S1=[C5]；S2=[B2]；S3=[∅]；S4=[B2 · C · B2]；S5=[∅][Retired]；S6=[∅][Retired] |
| 13 | Move S4→S2 B2（run=2，Count=2） | A,D | S1=[C5]；S2=[B4]；S3=[∅]；S4=[B2 · C]；S5=[∅][Retired]；S6=[∅][Retired] |
| 14 | Move S4→S1 C1（run=1，Count=1）；Home C@S1；Retire S1 | A,C,D | S1=[∅][Retired]；S2=[B4]；S3=[∅]；S4=[B2]；S5=[∅][Retired]；S6=[∅][Retired] |
| 15 | Move S2→S4 B4（run=4，Count=4）；Home B@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡24

秋日枫林挑战；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A · D · C3 · A` | `A · D · C3 · A` |
| S2 | 6 | `A · B3 · A · B` | `A · B3 · A · B` |
| S3 | 6 | `C · D · C · A · D · C` | `C · D · C · A · D · C` |
| S4 | 6 | `B · A · B · D3` | `B · A · B · D3` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：18个初始连续段、0颗问号蛋。主路18步，仅存在性见证。首次归巢在第14步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A1（run=1，Count=1） | 无 | S1=[A · D · C3]；S2=[A · B3 · A · B]；S3=[C · D · C · A · D · C]；S4=[B · A · B · D3]；S5=[A]；S6=[∅] |
| 2 | Move S3→S1 C1（run=1，Count=1） | 无 | S1=[A · D · C4]；S2=[A · B3 · A · B]；S3=[C · D · C · A · D]；S4=[B · A · B · D3]；S5=[A]；S6=[∅] |
| 3 | Move S1→S6 C4（run=4，Count=4） | 无 | S1=[A · D]；S2=[A · B3 · A · B]；S3=[C · D · C · A · D]；S4=[B · A · B · D3]；S5=[A]；S6=[C4] |
| 4 | Move S1→S3 D1（run=1，Count=1） | 无 | S1=[A]；S2=[A · B3 · A · B]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A]；S6=[C4] |
| 5 | Move S1→S5 A1（run=1，Count=1） | 无 | S1=[∅]；S2=[A · B3 · A · B]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A2]；S6=[C4] |
| 6 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[B]；S2=[A · B3 · A]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A2]；S6=[C4] |
| 7 | Move S2→S5 A1（run=1，Count=1） | 无 | S1=[B]；S2=[A · B3]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A3]；S6=[C4] |
| 8 | Move S2→S1 B3（run=3，Count=3） | 无 | S1=[B4]；S2=[A]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A3]；S6=[C4] |
| 9 | Move S2→S5 A1（run=1，Count=1） | 无 | S1=[B4]；S2=[∅]；S3=[C · D · C · A · D2]；S4=[B · A · B · D3]；S5=[A4]；S6=[C4] |
| 10 | Move S3→S2 D2（run=2，Count=2） | 无 | S1=[B4]；S2=[D2]；S3=[C · D · C · A]；S4=[B · A · B · D3]；S5=[A4]；S6=[C4] |
| 11 | Move S3→S5 A1（run=1，Count=1） | 无 | S1=[B4]；S2=[D2]；S3=[C · D · C]；S4=[B · A · B · D3]；S5=[A5]；S6=[C4] |
| 12 | Move S3→S6 C1（run=1，Count=1） | 无 | S1=[B4]；S2=[D2]；S3=[C · D]；S4=[B · A · B · D3]；S5=[A5]；S6=[C5] |
| 13 | Move S3→S2 D1（run=1，Count=1） | 无 | S1=[B4]；S2=[D3]；S3=[C]；S4=[B · A · B · D3]；S5=[A5]；S6=[C5] |
| 14 | Move S3→S6 C1（run=1，Count=1）；Home C@S6；Retire S6 | C | S1=[B4]；S2=[D3]；S3=[∅]；S4=[B · A · B · D3]；S5=[A5]；S6=[∅][Retired] |
| 15 | Move S4→S2 D3（run=3，Count=3）；Home D@S2；Retire S2 | C,D | S1=[B4]；S2=[∅][Retired]；S3=[∅]；S4=[B · A · B]；S5=[A5]；S6=[∅][Retired] |
| 16 | Move S4→S1 B1（run=1，Count=1） | C,D | S1=[B5]；S2=[∅][Retired]；S3=[∅]；S4=[B · A]；S5=[A5]；S6=[∅][Retired] |
| 17 | Move S4→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A,C,D | S1=[B5]；S2=[∅][Retired]；S3=[∅]；S4=[B]；S5=[∅][Retired]；S6=[∅][Retired] |
| 18 | Move S1→S4 B5（run=5，Count=5）；Home B@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡25

秋日枫林回落；世界C 秋日枫林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `B2 · A2 · ?(D) · A` | `B2 · A2 · ? · A` |
| S2 | 6 | `A2 · B4` | `A2 · B4` |
| S3 | 6 | `C2 · D3 · C` | `C2 · D3 · C` |
| S4 | 6 | `A · D · ?(D) · C3` | `A · D · ? · C3` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：13个初始连续段、2颗问号蛋。主路11步，仅存在性见证。首次归巢在第4步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第1,5步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S4→S5 C3（run=3，Count=3）；Hatch S4 → D | 无 | S1=[B2 · A2 · ?(D) · A]；S2=[A2 · B4]；S3=[C2 · D3 · C]；S4=[A · D2]；S5=[C3]；S6=[∅] |
| 2 | Move S3→S5 C1（run=1，Count=1） | 无 | S1=[B2 · A2 · ?(D) · A]；S2=[A2 · B4]；S3=[C2 · D3]；S4=[A · D2]；S5=[C4]；S6=[∅] |
| 3 | Move S3→S4 D3（run=3，Count=3） | 无 | S1=[B2 · A2 · ?(D) · A]；S2=[A2 · B4]；S3=[C2]；S4=[A · D5]；S5=[C4]；S6=[∅] |
| 4 | Move S3→S5 C2（run=2，Count=2）；Home C@S5；Retire S5 | C | S1=[B2 · A2 · ?(D) · A]；S2=[A2 · B4]；S3=[∅]；S4=[A · D5]；S5=[∅][Retired]；S6=[∅] |
| 5 | Move S1→S3 A1（run=1，Count=1）；Hatch S1 → D | C | S1=[B2 · A2 · D]；S2=[A2 · B4]；S3=[A]；S4=[A · D5]；S5=[∅][Retired]；S6=[∅] |
| 6 | Move S1→S6 D1（run=1，Count=1） | C | S1=[B2 · A2]；S2=[A2 · B4]；S3=[A]；S4=[A · D5]；S5=[∅][Retired]；S6=[D] |
| 7 | Move S1→S3 A2（run=2，Count=2） | C | S1=[B2]；S2=[A2 · B4]；S3=[A3]；S4=[A · D5]；S5=[∅][Retired]；S6=[D] |
| 8 | Move S2→S1 B4（run=4，Count=4）；Home B@S1；Retire S1 | B,C | S1=[∅][Retired]；S2=[A2]；S3=[A3]；S4=[A · D5]；S5=[∅][Retired]；S6=[D] |
| 9 | Move S2→S3 A2（run=2，Count=2） | B,C | S1=[∅][Retired]；S2=[∅]；S3=[A5]；S4=[A · D5]；S5=[∅][Retired]；S6=[D] |
| 10 | Move S4→S6 D5（run=5，Count=5）；Home D@S6；Retire S6 | B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[A5]；S4=[A]；S5=[∅][Retired]；S6=[∅][Retired] |
| 11 | Move S3→S4 A5（run=5，Count=5）；Home A@S4；Retire S4 | A,B,C,D | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡26

五种交错；世界C 秋日枫林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A · B2 · C · D2` | `A · B2 · C · D2` |
| S2 | 6 | `B3 · A2 · B` | `B3 · A2 · B` |
| S3 | 6 | `C3 · D3` | `C3 · D3` |
| S4 | 6 | `D · C2 · E3` | `D · C2 · E3` |
| S5 | 6 | `E3 · A3` | `E3 · A3` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：14个初始连续段、0颗问号蛋。主路11步，仅存在性见证。首次归巢在第7步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 D2（run=2，Count=2） | 无 | S1=[A · B2 · C]；S2=[B3 · A2 · B]；S3=[C3 · D3]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D2]；S7=[∅] |
| 2 | Move S3→S6 D3（run=3，Count=3） | 无 | S1=[A · B2 · C]；S2=[B3 · A2 · B]；S3=[C3]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[∅] |
| 3 | Move S1→S3 C1（run=1，Count=1） | 无 | S1=[A · B2]；S2=[B3 · A2 · B]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[∅] |
| 4 | Move S2→S1 B1（run=1，Count=1） | 无 | S1=[A · B3]；S2=[B3 · A2]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[∅] |
| 5 | Move S1→S7 B3（run=3，Count=3） | 无 | S1=[A]；S2=[B3 · A2]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[B3] |
| 6 | Move S2→S1 A2（run=2，Count=2） | 无 | S1=[A3]；S2=[B3]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[B3] |
| 7 | Move S2→S7 B3（run=3，Count=3）；Home B@S7；Retire S7 | B | S1=[A3]；S2=[∅]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3 · A3]；S6=[D5]；S7=[∅][Retired] |
| 8 | Move S5→S1 A3（run=3，Count=3）；Home A@S1；Retire S1 | A,B | S1=[∅][Retired]；S2=[∅]；S3=[C4]；S4=[D · C2 · E3]；S5=[E3]；S6=[D5]；S7=[∅][Retired] |
| 9 | Move S4→S5 E3（run=3，Count=3）；Home E@S5；Retire S5 | A,B,E | S1=[∅][Retired]；S2=[∅]；S3=[C4]；S4=[D · C2]；S5=[∅][Retired]；S6=[D5]；S7=[∅][Retired] |
| 10 | Move S4→S3 C2（run=2，Count=2）；Home C@S3；Retire S3 | A,B,C,E | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[D]；S5=[∅][Retired]；S6=[D5]；S7=[∅][Retired] |
| 11 | Move S4→S6 D1（run=1，Count=1）；Home D@S6；Retire S6 | A,B,C,D,E | S1=[∅][Retired]；S2=[∅]；S3=[∅][Retired]；S4=[∅]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡27

先解挡头；世界C 秋日枫林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A2 · ?(A) · C · E2` | `A2 · ? · C · E2` |
| S2 | 6 | `B3 · D2 · E` | `B3 · D2 · E` |
| S3 | 6 | `C2 · A · C3` | `C2 · A · C3` |
| S4 | 6 | `D · E · D · B2 · D` | `D · E · D · B2 · D` |
| S5 | 6 | `A2 · E · D · E · B` | `A2 · E · D · E · B` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：20个初始连续段、1颗问号蛋。主路17步，仅存在性见证。首次归巢在第10步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第2步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 E2（run=2，Count=2） | 无 | S1=[A2 · ?(A) · C]；S2=[B3 · D2 · E]；S3=[C2 · A · C3]；S4=[D · E · D · B2 · D]；S5=[A2 · E · D · E · B]；S6=[E2]；S7=[∅] |
| 2 | Move S1→S7 C1（run=1，Count=1）；Hatch S1 → A | 无 | S1=[A3]；S2=[B3 · D2 · E]；S3=[C2 · A · C3]；S4=[D · E · D · B2 · D]；S5=[A2 · E · D · E · B]；S6=[E2]；S7=[C] |
| 3 | Move S2→S6 E1（run=1，Count=1） | 无 | S1=[A3]；S2=[B3 · D2]；S3=[C2 · A · C3]；S4=[D · E · D · B2 · D]；S5=[A2 · E · D · E · B]；S6=[E3]；S7=[C] |
| 4 | Move S3→S7 C3（run=3，Count=3） | 无 | S1=[A3]；S2=[B3 · D2]；S3=[C2 · A]；S4=[D · E · D · B2 · D]；S5=[A2 · E · D · E · B]；S6=[E3]；S7=[C4] |
| 5 | Move S1→S3 A3（run=3，Count=3） | 无 | S1=[∅]；S2=[B3 · D2]；S3=[C2 · A4]；S4=[D · E · D · B2 · D]；S5=[A2 · E · D · E · B]；S6=[E3]；S7=[C4] |
| 6 | Move S4→S2 D1（run=1，Count=1） | 无 | S1=[∅]；S2=[B3 · D3]；S3=[C2 · A4]；S4=[D · E · D · B2]；S5=[A2 · E · D · E · B]；S6=[E3]；S7=[C4] |
| 7 | Move S5→S4 B1（run=1，Count=1） | 无 | S1=[∅]；S2=[B3 · D3]；S3=[C2 · A4]；S4=[D · E · D · B3]；S5=[A2 · E · D · E]；S6=[E3]；S7=[C4] |
| 8 | Move S5→S6 E1（run=1，Count=1） | 无 | S1=[∅]；S2=[B3 · D3]；S3=[C2 · A4]；S4=[D · E · D · B3]；S5=[A2 · E · D]；S6=[E4]；S7=[C4] |
| 9 | Move S2→S1 D3（run=3，Count=3） | 无 | S1=[D3]；S2=[B3]；S3=[C2 · A4]；S4=[D · E · D · B3]；S5=[A2 · E · D]；S6=[E4]；S7=[C4] |
| 10 | Move S4→S2 B3（run=3，Count=3）；Home B@S2；Retire S2 | B | S1=[D3]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[D · E · D]；S5=[A2 · E · D]；S6=[E4]；S7=[C4] |
| 11 | Move S4→S1 D1（run=1，Count=1） | B | S1=[D4]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[D · E]；S5=[A2 · E · D]；S6=[E4]；S7=[C4] |
| 12 | Move S4→S6 E1（run=1，Count=1） | B | S1=[D4]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[D]；S5=[A2 · E · D]；S6=[E5]；S7=[C4] |
| 13 | Move S1→S4 D4（run=4，Count=4） | B | S1=[∅]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[D5]；S5=[A2 · E · D]；S6=[E5]；S7=[C4] |
| 14 | Move S5→S4 D1（run=1，Count=1）；Home D@S4；Retire S4 | B,D | S1=[∅]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[∅][Retired]；S5=[A2 · E]；S6=[E5]；S7=[C4] |
| 15 | Move S5→S6 E1（run=1，Count=1）；Home E@S6；Retire S6 | B,D,E | S1=[∅]；S2=[∅][Retired]；S3=[C2 · A4]；S4=[∅][Retired]；S5=[A2]；S6=[∅][Retired]；S7=[C4] |
| 16 | Move S3→S5 A4（run=4，Count=4）；Home A@S5；Retire S5 | A,B,D,E | S1=[∅]；S2=[∅][Retired]；S3=[C2]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[C4] |
| 17 | Move S3→S7 C2（run=2，Count=2）；Home C@S7；Retire S7 | A,B,C,D,E | S1=[∅]；S2=[∅][Retired]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡28

秋日枫林总结；世界C 秋日枫林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `?(A) · A · D4` | `? · A · D4` |
| S2 | 6 | `E4 · C · B` | `E4 · C · B` |
| S3 | 6 | `C · D · A · E · B2` | `C · D · A · E · B2` |
| S4 | 6 | `C3 · A2 · D` | `C3 · A2 · D` |
| S5 | 6 | `?(E) · B3 · A · C` | `? · B3 · A · C` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：18个初始连续段、2颗问号蛋。主路15步，仅存在性见证。首次归巢在第11步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第3,9步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 D4（run=4，Count=4） | 无 | S1=[?(A) · A]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C3 · A2 · D]；S5=[?(E) · B3 · A · C]；S6=[D4]；S7=[∅] |
| 2 | Move S4→S6 D1（run=1，Count=1） | 无 | S1=[?(A) · A]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C3 · A2]；S5=[?(E) · B3 · A · C]；S6=[D5]；S7=[∅] |
| 3 | Move S1→S4 A1（run=1，Count=1）；Hatch S1 → A | 无 | S1=[A]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C3 · A3]；S5=[?(E) · B3 · A · C]；S6=[D5]；S7=[∅] |
| 4 | Move S4→S1 A3（run=3，Count=3） | 无 | S1=[A4]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C3]；S5=[?(E) · B3 · A · C]；S6=[D5]；S7=[∅] |
| 5 | Move S5→S4 C1（run=1，Count=1） | 无 | S1=[A4]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C4]；S5=[?(E) · B3 · A]；S6=[D5]；S7=[∅] |
| 6 | Move S5→S1 A1（run=1，Count=1） | 无 | S1=[A5]；S2=[E4 · C · B]；S3=[C · D · A · E · B2]；S4=[C4]；S5=[?(E) · B3]；S6=[D5]；S7=[∅] |
| 7 | Move S2→S5 B1（run=1，Count=1） | 无 | S1=[A5]；S2=[E4 · C]；S3=[C · D · A · E · B2]；S4=[C4]；S5=[?(E) · B4]；S6=[D5]；S7=[∅] |
| 8 | Move S2→S4 C1（run=1，Count=1） | 无 | S1=[A5]；S2=[E4]；S3=[C · D · A · E · B2]；S4=[C5]；S5=[?(E) · B4]；S6=[D5]；S7=[∅] |
| 9 | Move S5→S7 B4（run=4，Count=4）；Hatch S5 → E | 无 | S1=[A5]；S2=[E4]；S3=[C · D · A · E · B2]；S4=[C5]；S5=[E]；S6=[D5]；S7=[B4] |
| 10 | Move S2→S5 E4（run=4，Count=4） | 无 | S1=[A5]；S2=[∅]；S3=[C · D · A · E · B2]；S4=[C5]；S5=[E5]；S6=[D5]；S7=[B4] |
| 11 | Move S3→S7 B2（run=2，Count=2）；Home B@S7；Retire S7 | B | S1=[A5]；S2=[∅]；S3=[C · D · A · E]；S4=[C5]；S5=[E5]；S6=[D5]；S7=[∅][Retired] |
| 12 | Move S3→S5 E1（run=1，Count=1）；Home E@S5；Retire S5 | B,E | S1=[A5]；S2=[∅]；S3=[C · D · A]；S4=[C5]；S5=[∅][Retired]；S6=[D5]；S7=[∅][Retired] |
| 13 | Move S3→S1 A1（run=1，Count=1）；Home A@S1；Retire S1 | A,B,E | S1=[∅][Retired]；S2=[∅]；S3=[C · D]；S4=[C5]；S5=[∅][Retired]；S6=[D5]；S7=[∅][Retired] |
| 14 | Move S3→S6 D1（run=1，Count=1）；Home D@S6；Retire S6 | A,B,D,E | S1=[∅][Retired]；S2=[∅]；S3=[C]；S4=[C5]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |
| 15 | Move S3→S4 C1（run=1，Count=1）；Home C@S4；Retire S4 | A,B,C,D,E | S1=[∅][Retired]；S2=[∅]；S3=[∅]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡29

雪松林长群；世界D 雪松林；N=6，K=5，P=7；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `B · D · A4` | `B · D · A4` |
| S2 | 6 | `B2 · A · C · D2` | `B2 · A · C · D2` |
| S3 | 6 | `E · C3 · D · B` | `E · C3 · D · B` |
| S4 | 6 | `D · C · B · A · B · D` | `D · C · B · A · B · D` |
| S5 | 6 | `E4 · C · E` | `E4 · C · E` |
| S6 | 6 | `∅` | `∅` |
| S7 | 6 | `∅` | `∅` |

本关固定结构：20个初始连续段、0颗问号蛋。主路18步，仅存在性见证。首次归巢在第9步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S6 A4（run=4，Count=4） | 无 | S1=[B · D]；S2=[B2 · A · C · D2]；S3=[E · C3 · D · B]；S4=[D · C · B · A · B · D]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 2 | Move S2→S1 D2（run=2，Count=2） | 无 | S1=[B · D3]；S2=[B2 · A · C]；S3=[E · C3 · D · B]；S4=[D · C · B · A · B · D]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 3 | Move S4→S1 D1（run=1，Count=1） | 无 | S1=[B · D4]；S2=[B2 · A · C]；S3=[E · C3 · D · B]；S4=[D · C · B · A · B]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 4 | Move S3→S4 B1（run=1，Count=1） | 无 | S1=[B · D4]；S2=[B2 · A · C]；S3=[E · C3 · D]；S4=[D · C · B · A · B2]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 5 | Move S3→S1 D1（run=1，Count=1） | 无 | S1=[B · D5]；S2=[B2 · A · C]；S3=[E · C3]；S4=[D · C · B · A · B2]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 6 | Move S2→S3 C1（run=1，Count=1） | 无 | S1=[B · D5]；S2=[B2 · A]；S3=[E · C4]；S4=[D · C · B · A · B2]；S5=[E4 · C · E]；S6=[A4]；S7=[∅] |
| 7 | Move S2→S6 A1（run=1，Count=1） | 无 | S1=[B · D5]；S2=[B2]；S3=[E · C4]；S4=[D · C · B · A · B2]；S5=[E4 · C · E]；S6=[A5]；S7=[∅] |
| 8 | Move S4→S2 B2（run=2，Count=2） | 无 | S1=[B · D5]；S2=[B4]；S3=[E · C4]；S4=[D · C · B · A]；S5=[E4 · C · E]；S6=[A5]；S7=[∅] |
| 9 | Move S4→S6 A1（run=1，Count=1）；Home A@S6；Retire S6 | A | S1=[B · D5]；S2=[B4]；S3=[E · C4]；S4=[D · C · B]；S5=[E4 · C · E]；S6=[∅][Retired]；S7=[∅] |
| 10 | Move S4→S2 B1（run=1，Count=1） | A | S1=[B · D5]；S2=[B5]；S3=[E · C4]；S4=[D · C]；S5=[E4 · C · E]；S6=[∅][Retired]；S7=[∅] |
| 11 | Move S3→S4 C4（run=4，Count=4） | A | S1=[B · D5]；S2=[B5]；S3=[E]；S4=[D · C5]；S5=[E4 · C · E]；S6=[∅][Retired]；S7=[∅] |
| 12 | Move S5→S3 E1（run=1，Count=1） | A | S1=[B · D5]；S2=[B5]；S3=[E2]；S4=[D · C5]；S5=[E4 · C]；S6=[∅][Retired]；S7=[∅] |
| 13 | Move S1→S7 D5（run=5，Count=5） | A | S1=[B]；S2=[B5]；S3=[E2]；S4=[D · C5]；S5=[E4 · C]；S6=[∅][Retired]；S7=[D5] |
| 14 | Move S1→S2 B1（run=1，Count=1）；Home B@S2；Retire S2 | A,B | S1=[∅]；S2=[∅][Retired]；S3=[E2]；S4=[D · C5]；S5=[E4 · C]；S6=[∅][Retired]；S7=[D5] |
| 15 | Move S4→S1 C5（run=5，Count=5） | A,B | S1=[C5]；S2=[∅][Retired]；S3=[E2]；S4=[D]；S5=[E4 · C]；S6=[∅][Retired]；S7=[D5] |
| 16 | Move S4→S7 D1（run=1，Count=1）；Home D@S7；Retire S7 | A,B,D | S1=[C5]；S2=[∅][Retired]；S3=[E2]；S4=[∅]；S5=[E4 · C]；S6=[∅][Retired]；S7=[∅][Retired] |
| 17 | Move S5→S1 C1（run=1，Count=1）；Home C@S1；Retire S1 | A,B,C,D | S1=[∅][Retired]；S2=[∅][Retired]；S3=[E2]；S4=[∅]；S5=[E4]；S6=[∅][Retired]；S7=[∅][Retired] |
| 18 | Move S3→S5 E2（run=2，Count=2）；Home E@S5；Retire S5 | A,B,C,D,E | S1=[∅][Retired]；S2=[∅][Retired]；S3=[∅]；S4=[∅]；S5=[∅][Retired]；S6=[∅][Retired]；S7=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。

## 关卡30

四种揭示；世界D 雪松林；N=6，K=4，P=6；所有枝相同容量；每种含蛋共6token；开局两空。

来源：SRC-LEAF/SRC-SIMPLE（端部、接收与移动）；SRC-BIRD2（完成飞走失枝）；容量SRC-BOOP；SRC-QUEST（?蛋）。

| 固定ID | 容量 | 策划完整初始数组 | 玩家初始可见数组 |
| --- | --- | --- | --- |
| S1 | 6 | `A2 · D · A3` | `A2 · D · A3` |
| S2 | 6 | `?(B) · B3 · D2` | `? · B3 · D2` |
| S3 | 6 | `C · B · D · C3` | `C · B · D · C3` |
| S4 | 6 | `D · A · D · B · C2` | `D · A · D · B · C2` |
| S5 | 6 | `∅` | `∅` |
| S6 | 6 | `∅` | `∅` |

本关固定结构：15个初始连续段、1颗问号蛋。主路16步，仅存在性见证。首次归巢在第12步，之后该枝不再参与排序。本见证未发生容量截断，不证明所有路线都不会发生。孵化见第3步；未孵化种类对玩家隐藏。

### 完整合法见证

| 步 | 玩家动作和自动事件 | 已归巢种类 | 稳定后所有位置（分析视图） |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A3（run=3，Count=3） | 无 | S1=[A2 · D]；S2=[?(B) · B3 · D2]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A3]；S6=[∅] |
| 2 | Move S2→S1 D2（run=2，Count=2） | 无 | S1=[A2 · D3]；S2=[?(B) · B3]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A3]；S6=[∅] |
| 3 | Move S2→S6 B3（run=3，Count=3）；Hatch S2 → B | 无 | S1=[A2 · D3]；S2=[B]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A3]；S6=[B3] |
| 4 | Move S2→S6 B1（run=1，Count=1） | 无 | S1=[A2 · D3]；S2=[∅]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A3]；S6=[B4] |
| 5 | Move S1→S2 D3（run=3，Count=3） | 无 | S1=[A2]；S2=[D3]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A3]；S6=[B4] |
| 6 | Move S1→S5 A2（run=2，Count=2） | 无 | S1=[∅]；S2=[D3]；S3=[C · B · D · C3]；S4=[D · A · D · B · C2]；S5=[A5]；S6=[B4] |
| 7 | Move S3→S1 C3（run=3，Count=3） | 无 | S1=[C3]；S2=[D3]；S3=[C · B · D]；S4=[D · A · D · B · C2]；S5=[A5]；S6=[B4] |
| 8 | Move S2→S3 D3（run=3，Count=3） | 无 | S1=[C3]；S2=[∅]；S3=[C · B · D4]；S4=[D · A · D · B · C2]；S5=[A5]；S6=[B4] |
| 9 | Move S4→S1 C2（run=2，Count=2） | 无 | S1=[C5]；S2=[∅]；S3=[C · B · D4]；S4=[D · A · D · B]；S5=[A5]；S6=[B4] |
| 10 | Move S4→S6 B1（run=1，Count=1） | 无 | S1=[C5]；S2=[∅]；S3=[C · B · D4]；S4=[D · A · D]；S5=[A5]；S6=[B5] |
| 11 | Move S3→S2 D4（run=4，Count=4） | 无 | S1=[C5]；S2=[D4]；S3=[C · B]；S4=[D · A · D]；S5=[A5]；S6=[B5] |
| 12 | Move S3→S6 B1（run=1，Count=1）；Home B@S6；Retire S6 | B | S1=[C5]；S2=[D4]；S3=[C]；S4=[D · A · D]；S5=[A5]；S6=[∅][Retired] |
| 13 | Move S1→S3 C5（run=5，Count=5）；Home C@S3；Retire S3 | B,C | S1=[∅]；S2=[D4]；S3=[∅][Retired]；S4=[D · A · D]；S5=[A5]；S6=[∅][Retired] |
| 14 | Move S4→S2 D1（run=1，Count=1） | B,C | S1=[∅]；S2=[D5]；S3=[∅][Retired]；S4=[D · A]；S5=[A5]；S6=[∅][Retired] |
| 15 | Move S4→S5 A1（run=1，Count=1）；Home A@S5；Retire S5 | A,B,C | S1=[∅]；S2=[D5]；S3=[∅][Retired]；S4=[D]；S5=[∅][Retired]；S6=[∅][Retired] |
| 16 | Move S2→S4 D5（run=5，Count=5）；Home D@S4；Retire S4 | A,B,C,D | S1=[∅]；S2=[∅]；S3=[∅][Retired]；S4=[∅][Retired]；S5=[∅][Retired]；S6=[∅][Retired] |

终态：全部K种归巢、所有token为空、恰K条完成枝Retired，最初及中途普通搬空枝不因空而退休；每种现存token+已收集数量在每步均等于N。全程未使用扩枝、加高、提示、广告或付费。

验收边界：该路径由独立文档模型按完整species状态复算，非项目Core/Unity、非最优、非实际Undo/存档/音画或真人通过。含蛋关还须可见信息公平性验证；无此证据不得宣称玩家无需试错就能知道下一步。
