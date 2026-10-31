# 第01–10关：固定候选与纸面完整回放

2026-10-31 · DESIGNED / INDEPENDENT_REPLAY_PASS / PENDING_GRAYBOX。这是本批**固定初态唯一权威**；[设计卡](../LEVEL_DESIGN.md)只拥有教学/节奏，[模拟入口](../LEVEL_SIMULATIONS.md)拥有记号与证据边界，[RULES](../spec/RULES.md)拥有算法。仅Markdown候选，未修改游戏关卡JSON。

数组末项外露，`蛋(A)`是一只固定A种蛋，普通`E`始终是E鸟。`A3`精确等于三只A，不是随机数量；展开原序分配token ID。未列机关不存在。Complete/Sleep与Complete/Home表示本次首次成组六只、完成数+1；AutoHome不重复加数。表内接收临时枝事件包含首次Armed或已Armed再次接收，具体三态已在完整快照标出。

每条主路/对照路由独立文档模型逐步校验合法性、锁run/Count、稳定化、每种六只守恒及全空终态；不是项目Core/Unity/最优/真人证书。下面数值是路线长度，不是solverMinMoves。固定内容不得运行时生成、换色或换seed。

## 关卡01

K3/P5；Q=`A,B,C`。线性单鸟释放，不用空枝中转。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B5 · A` | 普通枝 |
| S2 | 6 | `A5` | 普通枝 |
| S3 | 6 | `C5 · B` | 普通枝 |
| S4 | 6 | `C` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |

### 主路线（3步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 A1；Complete/Home A@S2；run=1,Count=1 | 1 / A / BC | S1=[B5]；S2=[∅]；S3=[C5 · B]；S4=[C]；S5=[∅] |
| 2 | Move S3→S1 B1；Complete/Home B@S1；run=1,Count=1 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C5]；S4=[C]；S5=[∅] |
| 3 | Move S4→S3 C1；Complete/Home C@S3；run=1,Count=1 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡02

K3/P5；Q=`A,B,C`。A/B两枝双向互挡；B先或A先都可四步，C是独立汇合。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A4 · B2` | 普通枝 |
| S2 | 6 | `B4 · A2` | 普通枝 |
| S3 | 6 | `C3` | 普通枝 |
| S4 | 6 | `C3` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S5 A2；run=2,Count=2 | 0 / 无 / AB | S1=[A4 · B2]；S2=[B4]；S3=[C3]；S4=[C3]；S5=[A2] |
| 2 | Move S1→S2 B2；Complete/Home B@S2；run=2,Count=2 | 1 / B / AC | S1=[A4]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[A2] |
| 3 | Move S5→S1 A2；Complete/Home A@S1；run=2,Count=2 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 4 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S5 B2；run=2,Count=2 | 0 / 无 / AB | S1=[A4]；S2=[B4 · A2]；S3=[C3]；S4=[C3]；S5=[B2] |
| 2 | Move S2→S1 A2；Complete/Home A@S1；run=2,Count=2 | 1 / A / BC | S1=[∅]；S2=[B4]；S3=[C3]；S4=[C3]；S5=[B2] |
| 3 | Move S5→S2 B2；Complete/Home B@S2；run=2,Count=2 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 4 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡03

K3/P5；Q=`A,B,C`。主路示范睡眠连收；替代A先四步绕过睡眠，不声称必经。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · B3` | 普通枝 |
| S2 | 6 | `B3 · C3` | 普通枝 |
| S3 | 6 | `C3 · A3` | 普通枝 |
| S4 | 6 | `∅` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S5 A3；run=3,Count=3 | 0 / 无 / AB | S1=[A3 · B3]；S2=[B3 · C3]；S3=[C3]；S4=[∅]；S5=[A3] |
| 2 | Move S2→S3 C3；Complete/Sleep C@S3；run=3,Count=3 | 1 / 无 / AB | S1=[A3 · B3]；S2=[B3]；S3=[C6][睡眠]；S4=[∅]；S5=[A3] |
| 3 | Move S1→S2 B3；Complete/Home B@S2；AutoHome C@S3；run=3,Count=3 | 2 / BC / A | S1=[A3]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[A3] |
| 4 | Move S5→S1 A3；Complete/Home A@S1；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S4 B3；run=3,Count=3 | 0 / 无 / AB | S1=[A3]；S2=[B3 · C3]；S3=[C3 · A3]；S4=[B3]；S5=[∅] |
| 2 | Move S3→S1 A3；Complete/Home A@S1；run=3,Count=3 | 1 / A / BC | S1=[∅]；S2=[B3 · C3]；S3=[C3]；S4=[B3]；S5=[∅] |
| 3 | Move S2→S3 C3；Complete/Home C@S3；run=3,Count=3 | 2 / AC / B | S1=[∅]；S2=[B3]；S3=[∅]；S4=[B3]；S5=[∅] |
| 4 | Move S4→S2 B3；Complete/Home B@S2；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡04

K3/P5；Q=`A,C,B`。三层非连续同种，先剥外层再回流；不是整枝抽色。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A2 · B2 · A2` | 普通枝 |
| S2 | 6 | `B2 · C2 · B2` | 普通枝 |
| S3 | 6 | `C2 · A2 · C2` | 普通枝 |
| S4 | 6 | `∅` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |

### 主路线（7步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S4 A2；run=2,Count=2 | 0 / 无 / AC | S1=[A2 · B2]；S2=[B2 · C2 · B2]；S3=[C2 · A2 · C2]；S4=[A2]；S5=[∅] |
| 2 | Move S2→S1 B2；run=2,Count=2 | 0 / 无 / AC | S1=[A2 · B4]；S2=[B2 · C2]；S3=[C2 · A2 · C2]；S4=[A2]；S5=[∅] |
| 3 | Move S3→S2 C2；run=2,Count=2 | 0 / 无 / AC | S1=[A2 · B4]；S2=[B2 · C4]；S3=[C2 · A2]；S4=[A2]；S5=[∅] |
| 4 | Move S3→S4 A2；run=2,Count=2 | 0 / 无 / AC | S1=[A2 · B4]；S2=[B2 · C4]；S3=[C2]；S4=[A4]；S5=[∅] |
| 5 | Move S2→S3 C4；Complete/Home C@S3；run=4,Count=4 | 1 / C / AB | S1=[A2 · B4]；S2=[B2]；S3=[∅]；S4=[A4]；S5=[∅] |
| 6 | Move S1→S2 B4；Complete/Home B@S2；run=4,Count=4 | 2 / CB / A | S1=[A2]；S2=[∅]；S3=[∅]；S4=[A4]；S5=[∅] |
| 7 | Move S1→S4 A2；Complete/Home A@S4；run=2,Count=2 | 3 / ACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡05

K3/P5；Q=`C,A,B`。四枝分布、大段回流，舒缓。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A4 · C2` | 普通枝 |
| S2 | 6 | `B4 · A2` | 普通枝 |
| S3 | 6 | `C2 · B2` | 普通枝 |
| S4 | 6 | `C2` | 普通枝 |
| S5 | 6 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S4 C2；run=2,Count=2 | 0 / 无 / CA | S1=[A4]；S2=[B4 · A2]；S3=[C2 · B2]；S4=[C4]；S5=[∅] |
| 2 | Move S2→S1 A2；Complete/Home A@S1；run=2,Count=2 | 1 / A / CB | S1=[∅]；S2=[B4]；S3=[C2 · B2]；S4=[C4]；S5=[∅] |
| 3 | Move S3→S2 B2；Complete/Home B@S2；run=2,Count=2 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C2]；S4=[C4]；S5=[∅] |
| 4 | Move S3→S4 C2；Complete/Home C@S4；run=2,Count=2 | 3 / CAB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡06

K3/P5；Q=`A,B,C`。A4占满普通短枝仍不完成；替代不用短枝也能赢，教学非必要性。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A2 · B4` | 普通枝 |
| S2 | 6 | `B2 · A4` | 普通枝 |
| S3 | 6 | `C3` | 普通枝 |
| S4 | 6 | `C3` | 普通枝 |
| S5 | 4 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→S5 A4；run=4,Count=4 | 0 / 无 / AB | S1=[A2 · B4]；S2=[B2]；S3=[C3]；S4=[C3]；S5=[A4] |
| 2 | Move S1→S2 B4；Complete/Home B@S2；run=4,Count=4 | 1 / B / AC | S1=[A2]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[A4] |
| 3 | Move S5→S1 A4；Complete/Home A@S1；run=4,Count=4 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 4 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S4 C3；Complete/Sleep C@S4；run=3,Count=3 | 1 / 无 / AB | S1=[A2 · B4]；S2=[B2 · A4]；S3=[∅]；S4=[C6][睡眠]；S5=[∅] |
| 2 | Move S1→S3 B4；run=4,Count=4 | 1 / 无 / AB | S1=[A2]；S2=[B2 · A4]；S3=[B4]；S4=[C6][睡眠]；S5=[∅] |
| 3 | Move S2→S1 A4；Complete/Home A@S1；AutoHome C@S4；run=4,Count=4 | 2 / AC / B | S1=[∅]；S2=[B2]；S3=[B4]；S4=[∅]；S5=[∅] |
| 4 | Move S2→S3 B2；Complete/Home B@S3；run=2,Count=2 | 3 / ABC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡07

K3/P5；Q=`B,C,A`。第二步run3只进2，留1；替代可绕过截断，非强制最优。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `B3 · A3` | 普通枝 |
| S2 | 6 | `B3` | 普通枝 |
| S3 | 6 | `C3 · A3` | 普通枝 |
| S4 | 6 | `C3` | 普通枝 |
| S5 | 5 | `∅` | 普通枝 |

### 主路线（6步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A3；run=3,Count=3 | 0 / 无 / BC | S1=[B3]；S2=[B3]；S3=[C3 · A3]；S4=[C3]；S5=[A3] |
| 2 | Move S3→S5 A2；run=3,Count=2 | 0 / 无 / BC | S1=[B3]；S2=[B3]；S3=[C3 · A]；S4=[C3]；S5=[A5] |
| 3 | Move S1→S2 B3；Complete/Home B@S2；run=3,Count=3 | 1 / B / CA | S1=[∅]；S2=[∅]；S3=[C3 · A]；S4=[C3]；S5=[A5] |
| 4 | Move S3→S1 A1；run=1,Count=1 | 1 / B / CA | S1=[A]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[A5] |
| 5 | Move S5→S1 A5；Complete/Home A@S1；run=5,Count=5 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 6 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / BCA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S5 A3；run=3,Count=3 | 0 / 无 / BC | S1=[B3]；S2=[B3]；S3=[C3 · A3]；S4=[C3]；S5=[A3] |
| 2 | Move S1→S2 B3；Complete/Home B@S2；run=3,Count=3 | 1 / B / CA | S1=[∅]；S2=[∅]；S3=[C3 · A3]；S4=[C3]；S5=[A3] |
| 3 | Move S3→S1 A3；run=3,Count=3 | 1 / B / CA | S1=[A3]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[A3] |
| 4 | Move S5→S1 A3；Complete/Home A@S1；run=3,Count=3 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 5 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / BCA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡08

K3/P5；Q=`B,A,C`。五枝开局非空，篮进出教学；不用篮反而四步，收益不足是待灰盒问题。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A5 · B` | 普通枝 |
| S2 | 6 | `B5` | 普通枝 |
| S3 | 6 | `C4 · A` | 普通枝 |
| S4 | 6 | `C` | 普通枝 |
| S5 | 6 | `C` | 普通枝 |
| T1 | 1 | `∅` | 篮 |

### 主路线（5步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 B1；run=1,Count=1 | 0 / 无 / BA | S1=[A5]；S2=[B5]；S3=[C4 · A]；S4=[C]；S5=[C]；T1=[B] |
| 2 | Move S3→S1 A1；Complete/Home A@S1；run=1,Count=1 | 1 / A / BC | S1=[∅]；S2=[B5]；S3=[C4]；S4=[C]；S5=[C]；T1=[B] |
| 3 | Move T1→S2 B1；Complete/Home B@S2；run=1,Count=1 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C4]；S4=[C]；S5=[C]；T1=[∅] |
| 4 | Move S4→S3 C1；run=1,Count=1 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C5]；S4=[∅]；S5=[C]；T1=[∅] |
| 5 | Move S5→S3 C1；Complete/Home C@S3；run=1,Count=1 | 3 / BAC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；T1=[∅] |


### 对照路线1（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 B1；Complete/Home B@S2；run=1,Count=1 | 1 / B / AC | S1=[A5]；S2=[∅]；S3=[C4 · A]；S4=[C]；S5=[C]；T1=[∅] |
| 2 | Move S3→S1 A1；Complete/Home A@S1；run=1,Count=1 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C4]；S4=[C]；S5=[C]；T1=[∅] |
| 3 | Move S4→S3 C1；run=1,Count=1 | 2 / BA / C | S1=[∅]；S2=[∅]；S3=[C5]；S4=[∅]；S5=[C]；T1=[∅] |
| 4 | Move S5→S3 C1；Complete/Home C@S3；run=1,Count=1 | 3 / BAC / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅]；T1=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡09

K3/P4；Q=`C,B,A`。P4章节挑战，同篮先C后A复用；首还篮早于首归巢。独立禁篮穷尽11状态无胜利仅为外部模型证据。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `C · B · C · A · B · A` | 普通枝 |
| S2 | 6 | `A · B · C · A · B · C` | 普通枝 |
| S3 | 6 | `A · B · C · A · B · C` | 普通枝 |
| S4 | 6 | `∅` | 普通枝 |
| T1 | 1 | `∅` | 篮 |

### 主路线（18步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S2→T1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A · B · A]；S2=[A · B · C · A · B]；S3=[A · B · C · A · B · C]；S4=[∅]；T1=[C] |
| 2 | Move S2→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A · B · A]；S2=[A · B · C · A]；S3=[A · B · C · A · B · C]；S4=[B]；T1=[C] |
| 3 | Move S1→S2 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A · B]；S2=[A · B · C · A2]；S3=[A · B · C · A · B · C]；S4=[B]；T1=[C] |
| 4 | Move S1→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A]；S2=[A · B · C · A2]；S3=[A · B · C · A · B · C]；S4=[B2]；T1=[C] |
| 5 | Move S1→S2 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C]；S2=[A · B · C · A3]；S3=[A · B · C · A · B · C]；S4=[B2]；T1=[C] |
| 6 | Move S3→S1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C2]；S2=[A · B · C · A3]；S3=[A · B · C · A · B]；S4=[B2]；T1=[C] |
| 7 | Move S3→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C2]；S2=[A · B · C · A3]；S3=[A · B · C · A]；S4=[B3]；T1=[C] |
| 8 | Move T1→S1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C3]；S2=[A · B · C · A3]；S3=[A · B · C · A]；S4=[B3]；T1=[∅] |
| 9 | Move S3→T1 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C3]；S2=[A · B · C · A3]；S3=[A · B · C]；S4=[B3]；T1=[A] |
| 10 | Move S1→S3 C3；run=3,Count=3 | 0 / 无 / CB | S1=[C · B]；S2=[A · B · C · A3]；S3=[A · B · C4]；S4=[B3]；T1=[A] |
| 11 | Move S1→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C]；S2=[A · B · C · A3]；S3=[A · B · C4]；S4=[B4]；T1=[A] |
| 12 | Move S3→S1 C4；run=4,Count=4 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C · A3]；S3=[A · B]；S4=[B4]；T1=[A] |
| 13 | Move S3→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C · A3]；S3=[A]；S4=[B5]；T1=[A] |
| 14 | Move S2→S3 A3；run=3,Count=3 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C]；S3=[A4]；S4=[B5]；T1=[A] |
| 15 | Move S2→S1 C1；Complete/Home C@S1；run=1,Count=1 | 1 / C / BA | S1=[∅]；S2=[A · B]；S3=[A4]；S4=[B5]；T1=[A] |
| 16 | Move S2→S4 B1；Complete/Home B@S4；run=1,Count=1 | 2 / CB / A | S1=[∅]；S2=[A]；S3=[A4]；S4=[∅]；T1=[A] |
| 17 | Move S2→S3 A1；run=1,Count=1 | 2 / CB / A | S1=[∅]；S2=[∅]；S3=[A5]；S4=[∅]；T1=[A] |
| 18 | Move T1→S3 A1；Complete/Home A@S3；run=1,Count=1 | 3 / CBA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；T1=[∅] |


### 对照路线1（19步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→T1 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A · B]；S2=[A · B · C · A · B · C]；S3=[A · B · C · A · B · C]；S4=[∅]；T1=[A] |
| 2 | Move S1→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A]；S2=[A · B · C · A · B · C]；S3=[A · B · C · A · B · C]；S4=[B]；T1=[A] |
| 3 | Move T1→S1 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A2]；S2=[A · B · C · A · B · C]；S3=[A · B · C · A · B · C]；S4=[B]；T1=[∅] |
| 4 | Move S2→T1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A2]；S2=[A · B · C · A · B]；S3=[A · B · C · A · B · C]；S4=[B]；T1=[C] |
| 5 | Move S2→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C · A2]；S2=[A · B · C · A]；S3=[A · B · C · A · B · C]；S4=[B2]；T1=[C] |
| 6 | Move S1→S2 A2；run=2,Count=2 | 0 / 无 / CB | S1=[C · B · C]；S2=[A · B · C · A3]；S3=[A · B · C · A · B · C]；S4=[B2]；T1=[C] |
| 7 | Move S3→S1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C2]；S2=[A · B · C · A3]；S3=[A · B · C · A · B]；S4=[B2]；T1=[C] |
| 8 | Move S3→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C2]；S2=[A · B · C · A3]；S3=[A · B · C · A]；S4=[B3]；T1=[C] |
| 9 | Move T1→S1 C1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C3]；S2=[A · B · C · A3]；S3=[A · B · C · A]；S4=[B3]；T1=[∅] |
| 10 | Move S3→T1 A1；run=1,Count=1 | 0 / 无 / CB | S1=[C · B · C3]；S2=[A · B · C · A3]；S3=[A · B · C]；S4=[B3]；T1=[A] |
| 11 | Move S1→S3 C3；run=3,Count=3 | 0 / 无 / CB | S1=[C · B]；S2=[A · B · C · A3]；S3=[A · B · C4]；S4=[B3]；T1=[A] |
| 12 | Move S1→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C]；S2=[A · B · C · A3]；S3=[A · B · C4]；S4=[B4]；T1=[A] |
| 13 | Move S3→S1 C4；run=4,Count=4 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C · A3]；S3=[A · B]；S4=[B4]；T1=[A] |
| 14 | Move S3→S4 B1；run=1,Count=1 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C · A3]；S3=[A]；S4=[B5]；T1=[A] |
| 15 | Move S2→S3 A3；run=3,Count=3 | 0 / 无 / CB | S1=[C5]；S2=[A · B · C]；S3=[A4]；S4=[B5]；T1=[A] |
| 16 | Move S2→S1 C1；Complete/Home C@S1；run=1,Count=1 | 1 / C / BA | S1=[∅]；S2=[A · B]；S3=[A4]；S4=[B5]；T1=[A] |
| 17 | Move S2→S4 B1；Complete/Home B@S4；run=1,Count=1 | 2 / CB / A | S1=[∅]；S2=[A]；S3=[A4]；S4=[∅]；T1=[A] |
| 18 | Move S2→S3 A1；run=1,Count=1 | 2 / CB / A | S1=[∅]；S2=[∅]；S3=[A5]；S4=[∅]；T1=[A] |
| 19 | Move T1→S3 A1；Complete/Home A@S3；run=1,Count=1 | 3 / CBA / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；T1=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。

## 关卡10

K3/P5；Q=`A,C,B`。可选短枝+睡眠连收，09后舒缓。

| 固定ID | 容量 | 固定初始数组（墙→露端） | 类型/条件 |
| --- | --- | --- | --- |
| S1 | 6 | `A3 · B3` | 普通枝 |
| S2 | 6 | `B3` | 普通枝 |
| S3 | 6 | `C3 · A3` | 普通枝 |
| S4 | 6 | `C3` | 普通枝 |
| S5 | 5 | `∅` | 普通枝 |

### 主路线（4步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S3→S5 A3；run=3,Count=3 | 0 / 无 / AC | S1=[A3 · B3]；S2=[B3]；S3=[C3]；S4=[C3]；S5=[A3] |
| 2 | Move S1→S2 B3；Complete/Sleep B@S2；run=3,Count=3 | 1 / 无 / AC | S1=[A3]；S2=[B6][睡眠]；S3=[C3]；S4=[C3]；S5=[A3] |
| 3 | Move S5→S1 A3；Complete/Home A@S1；AutoHome B@S2；run=3,Count=3 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 4 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / ACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


### 对照路线1（3步，非最优证明）

| 步 | 玩家动作/自动事件顺序 | 完成数 / 已归巢 / 当前 | 稳定后全部容器 |
| --- | --- | --- | --- |
| 1 | Move S1→S2 B3；Complete/Sleep B@S2；run=3,Count=3 | 1 / 无 / AC | S1=[A3]；S2=[B6][睡眠]；S3=[C3 · A3]；S4=[C3]；S5=[∅] |
| 2 | Move S3→S1 A3；Complete/Home A@S1；AutoHome B@S2；run=3,Count=3 | 2 / AB / C | S1=[∅]；S2=[∅]；S3=[C3]；S4=[C3]；S5=[∅] |
| 3 | Move S3→S4 C3；Complete/Home C@S4；run=3,Count=3 | 3 / ACB / 全部完成 | S1=[∅]；S2=[∅]；S3=[∅]；S4=[∅]；S5=[∅] |


终态：全部目标已收集；全部容器无token，临时枝若启用已退役；物种每步守恒。项目最优证明/Unity/真人/设备/实际Undo存档均未验证。
