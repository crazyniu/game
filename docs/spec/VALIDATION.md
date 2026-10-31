# 同行依据版验收与证明边界

规则4/schema4/save6设计 · 2026-10-31。仅文档与独立临时模型分析，非游戏实现。有效规则见RULES，来源MECHANIC_EVIDENCE；旧T16 60回放和旧39死局整体失效，不能续用旧最优数/证书。

## 1. 四层证据不可混用

1. 同行证据：每项玩法指向已核第一方具体文字，USER约束、明示项目迁移和E协议分别标记；功能名字没有精确算法时不可补猜。R07由SRC-WSWEB明确的一空容器机制迁移，N容量/免费范围/事务集成为项目决定，不要求伪造每项竞品原数。缺证项退出有效规则。
2. 固定初态与独立见证：每关有明确N/K/P/数组/蛋形态和合法完整路径；临时分析逐步核对。它只证明模型/初盘下一条解存在。
3. 项目Core/最优：用将来新Core回放同hash；求解分未知/上界/Exact/穷尽无解，不能用独立分析当代码通过。
4. 真实游戏/玩家/设备：输入、Undo/磁盘、动态表现、隐藏信息公平、地图/声音/安全区与移动平台运行，必须实测，不以截图代替录屏。

## 2. 新40固定入库闸门

每关N4/6/8、每种含蛋总N、P=K+2、两空、所有枝同N；初始无遮挡外露无蛋/无完整群/无相邻蛋。检查species连续范围、稳定ID/数组原序、世界归属、hash与禁止旧字段。Replay/Restart/重进逐字段相同，不运行生成器/随机换种。

每步锁run/Count，目标余量截断，孵化不追飞；Home只一次且对应枝Retired；普通搬空仍Available；每种remaining+Home×N=N。全胜要求K种全部Home且无token；不以满枝视觉/没有下一步代替胜利。

新候选见[LEVEL_SIMULATIONS](../LEVEL_SIMULATIONS.md)。本轮40条主路线共474次Move，20关含隐藏蛋；全部独立模型见证，最优、项目Core、Unity和真人均未测。旧附录正文已替换，不是换个版本名沿用旧结果。内容结构标准化通过只能排直接同构，不能证明每关决策好玩。

## 3. 精确Given/When/Then（均为新规则断言，非已实现测试）

| ID | Given | When | Then |
| --- | --- | --- | --- |
| V01 | N4源[B,A,A]、目标[B,A,A]，余处补B2/C4 | 源→目标 | run2/Count1，源[B,A]目标[B,A,A,A]；不完成，A总4守恒 |
| V02 | N4源[B,蛋A,A]、目标[A,A]，余处补合法种数 | 源→目标 | 只飞原A1，源蛋孵成A1但不追加，目标A3；ID不变 |
| V03 | 初始数组有相邻蛋A/蛋B | 校验 | 拒绝该配置；边界来源未核，不写自拟连蛋孵化规则让其通过 |
| V04 | 任一Available枝只有A1，目标同种且不组成完整群 | 搬最后A1离源 | 空源仍Available，不能按旧Armed规则Retire |
| V05 | N4目标A3，源外露A1 | 搬入 | A4 Home、目标Retired；源若空仍Available，不等待目标/睡眠 |
| V06 | V05稳定完成后有历史 | Undo | 整笔恢复A1/A3、Home集合/步数、目标Available；不重跑空退休；Restart回固定初态 |
| V07 | 源[id1:A,id2:A]、目标[id3:A]且N4 | 搬2，即使id2先动画起飞 | 目标逻辑[id3,id1,id2]，不按视觉反序、回放/hash保持 |
| V08 | 初始?蛋species固定为B | Restart/回放/退出重进 | 同ID同种同位；孵前图案、音频、可访问名称均不泄露B；不抽种 |
| V09 | 当前没有合法Transfer但未全Home | UI检查 | NoMoves给免费Undo/Restart；不是全局穷尽无解、不扣资源 |
| V10 | Hint找到合法完整路径但未证最优 | 返回当前有效首步 | 可高亮合法首步，不收费不限次、不自动走；Unknown/超时不报无解 |
| V11 | Hint搜索中玩家移动/重进 | 旧结果返回 | stateKey/actionSequence/viewGeneration不符即丢弃，无旧高亮/解锁穿透 |
| V12 | 已提交稳定Home+Retire，动画一半 | 后台/强杀恢复 | 恢复整个已提交终态；提交前崩溃只恢复上一成功档，无半群/半枝 |
| V13 | 已提交序号7，候选8写档失败 | 重试/结果不确定 | SaveError保同字节/同8；先读回核验，成功只提交一次；失败仍旧局7，不演成功 |
| V14 | 前世界最后关成功事务 | 结果演出/回地图 | 完成集合驱动下一世界开放且首关可玩；不靠星/金币；重复回调不开第二遍 |
| V15 | 图标按钮已有明确含义 | 显示功能 | 无同义可见文字；保可访问名称；规则形按钮，无音量滑条/百分比 |
| V16 | 只有一张fly图或一张结算图 | 声称动画完成 | 验收失败：需真实振翅/轨迹/入巢遮挡及动态结算序列、可播放运行录屏 |
| V17 | 未验收蛋/振翅资源或动画库未测 | 请求实现/交付 | 当前无授权不执行；未来有授权也先验资源/库兼容，不能拿静图代替 |
| V18 | 旧targets/crate/temporary或未知version | 新格式读取 | 拒绝并备份，不静默忽略；旧证书/旧回放不可当新证明 |

局部夹具若不列全棋盘，必须标只测函数；不能称可直接运行LevelValidator的正式关。若将来引入相邻蛋须先补原作证据再改RULES/配置和重验；本版禁止该配置，不为保旧路径暗补规则。

## 4. 证明状态与Hint边界

NOT_RUN未执行；UNKNOWN预算耗尽/取消/无法确定；SOLVED_UPPER_BOUND有完整合法见证但未证最优；PROVEN_EXACT最短证明+回放；PROVEN_UNSOLVABLE穷尽有限空间。搜索找到20步只是upperBound20；证明18最短才能写Exact18。实际无合法一步只证明局部NoMoves。

证书绑定rules/schema/campaign/catalog/level/hash、assistPolicyVersion=1、actionSet及extraBranchAdded/动态枝态、求解器版本、动作类型/Transfer Count、预算/访问节点、终局键。包含蛋或完成退枝后必须重新证明启发式可采纳；旧块数减一下界不能直接套。可先h=0正确搜索。正式bestMoveCount是玩家实际成功最少记录，不伪装理论minMoves；提示只需有效解首步，仍免费不限次。

## 5. 真人、设备、素材与库验收

20蛋关是全信息模型有解，**隐藏信息公平PENDING**：用玩家当时可见信息记录首选、揭示、误判、Undo恢复、是否依赖背答案；免费无限回退不自动证明不挫败。每世界测首次教学/挑战/舒缓与前后重复感，不把K/步数增长当可玩性证据。样本量/时间为测试计划另评审，不再引用旧未测的4/5理解阈值当通过结论。

保留灰度/色觉、真实刘海/手势、design px到pt/dp换算、静音/减动效可辨状态。用户要求所有动画位置真实动态：运行录屏+关键帧时间戳，不能只PNG或单图平移冒振翅；地图/按钮无重复文字、规则形、静音开关验收分别做。LitMotion等库/IL2CPP/AOT/裁剪/场景销毁/池回收取消边界见ANIMATION_LIBRARIES。本轮无安装/编译/Unity/设备测试。

未来代码/数据测试遵AGENTS，不能删测试/造绿或以旧163/14数字代替新规则结果。当前文档-only改动不用启动Unity重建场景。

## 6. 加枝专项断言（V19–V26，文档用例；非Unity测试）

| ID | Given | When | Then |
| --- | --- | --- | --- |
| V19 | N4、P5、false，稳定且未Won | AddBranch后再次点击 | 仅S6一条空Available枝，true，操作数不变、序号+1；第二次无效果 |
| V20 | N6/P6或N8/P8 | AddBranch | 分别追加S7/N6或S9/N8；原token/蛋/枝ID/位置不变，总鸟数不变；无局内随机 |
| V21 | AddBranch→合法Transfer到末枝 | 连续Undo两次 | 首次恢复空末枝且true；第二次删除末枝恢复false和原操作数；不能直接撤除含鸟枝 |
| V22 | 额外枝外端A(N−1)，源A1 | Transfer补满→Undo | Home+Retire但extraBranchAdded仍true、不返枝；Undo恢复两枝及Available，不新开第二枝 |
| V23 | true且末枝有鸟或已Retired | 保存后重进；再Restart | 重进保同枝/额度/历史；Restart只在成功提交后回P枝、false、固定初态且清历史 |
| V24 | AddBranch候选提交结果不确定/重复回调 | 读回重试/强杀恢复 | 相同序号与候选hash确认，只append一次；未提交恢复旧局，绝不丢鸟或产生P+2枝 |
| V25 | Hint运行时AddBranch或Undo加枝 | Hint旧结果返回；新查询 | 旧代次/辅助状态不匹配即丢弃；新Hint仅搜索当前枝Transfer，不偷偷AddBranch；未知不报无解 |
| V26 | 成功路径含/不含已撤回的AddBranch | Won记成绩 | 按当前extraBranchAdded分助力/无助力两个最少Transfer记录；两类均正常解锁，不加奖励/惩罚 |

所有新快照/历史须校验assistPolicyVersion和extraBranchAdded与P/P+1长度一致；false+多枝、true+缺末枝、重复ID、未知动作、Undo强删含鸟枝均拒绝。静态H04 hash保持不变不意味着运行辅助身份可省略。40条474 Move旧新模型见证仅为无AddBranch子集；扩容事务用例/真实存盘/动画/额外枝触控仍待新Core与设备验证。加枝不能替代20蛋关按可见信息的公平验收。

### 临时文档模型回放记录（2026-10-31）

对N=4/6/8分别使用完整局部夹具K3/P5：S1=[A×(N−1),B]、S2=[B×(N−1),C]、S3=[C×(N−1),A]、S4/S5=[]；均无蛋，每种N只。执行：AddBranch S6→重复AddBranch拒绝→S1→S6 B1→Undo Transfer→Undo AddBranch→重新AddBranch→S1→S6 B1→S2→S5 C1→S2→S6 B(N−1)（B Home/S6 Retired）→JSON快照拷贝恢复→Undo末Transfer→重建固定初态。核对枝数量/ID/容量、每色守恒、0步加枝、退役不返额度、历史恢复；临时JavaScript模型共216条断言通过。

首轮夹具误在未移走S2外端C时尝试B入枝，按规则被拒；补上S2→S5揭示步骤后重跑通过，未为测试改规则。这里仅是独立文档模型，不是项目Core/真实磁盘/并发/Hint/UI测试，也不覆盖蛋公平；V24/V25和设备测试仍NOT_RUN。
