# T10e 扇尾鸽重绘报告

完成日期：2026-10-31。任务 `task_eec194d0e379`，Dispatch `ctx_5a507195c68b`。

已重绘 bird_05 正面四态，更新 manifest 和三张 contact 预览。尺寸与像素约束检查完成，视觉验收交协调者；未修改 PerchSort/，未提交 Git。

## 尺寸实测

坐标原点为左上角，包围盒右/下界不含在内。高度采用协调者消息 `msg_9b64ec8d7886` 确认的 T10d 口径：去尾、含冠/头顶羽毛，量至脚底；身体宽度含收起的双翼，不含尾扇。

| 站立指标 | bird_01 基准 | bird_05 新稿 | T10e 要求 |
| --- | ---: | ---: | --- |
| 去尾高度（头顶至脚底） | 774px | **774px** | 相差 ≤5%；实测 0% |
| 去尾身体宽度 | 478px | **423px** | 409.6–512px；实测画布 **41.31%** |
| 含尾总宽 | 596px | **481px** | ≤563.2px；实测画布 **46.97%** |

bird_01 去尾宽度取无尾干扰的上半部轮廓 x=[281,759)，尾羽在 y≈621 后开始外露。bird_05 身体轮廓最宽处位于尾扇下方，x=[301,724)，去尾高度 y=[97,871)；全图可见边界为 [272,97,753,871)。尾扇左右各外露 **29px**，均为身体宽度的 **6.86%**，低于 25%。外轮廓判定结合原画目视与 alpha 行扫描，不将尾羽计入身体宽度。

| 状态 | 可见包围盒 | 检查结果 |
| --- | --- | --- |
| 站姿 | [272,97,753,871) | 脚底 y=870 |
| 眨眼 | [272,97,753,871) | 脚底 y=870，眼区之外 RGBA 与站姿一致 |
| 睡姿 | [237,363,787,871) | 趴下闭眼、收腿，脚底 y=870 |
| 飞行 | [22,158,1002,811) | 翼展 **980px**，无裁切 |

飞行可见中心为 (512,484.5)，站姿可见中心为 (512.5,484)，仅有整数像素放置产生的 0.5px 差。二者共同锚点为 (512,870)，飞行脚部收起，锚点为虚拟脚线。

眨眼通过 image_gen 生成闭眼原画，再仅合成两个眼区。允许眼区为 [400,235,483,326) 和 [539,235,623,326)；实际 RGB 差异包围盒 [402,237,621,325)，两眼区外没有差异。所有四张均为 **1024×1024 RGBA PNG**，alpha 范围为 0–255。

## 变更与检查范围

仅覆盖以下 8 个已有文件，另新增本报告：

- `art/raw/birds_front/bird_05.png`
- `art/raw/birds_front/bird_05_blink.png`
- `art/raw/birds_front/bird_05_sleep.png`
- `art/raw/birds_front/bird_05_fly.png`
- `art/raw/birds_front/manifest_front.json`
- `art/raw/birds_front/contact_front.png`
- `art/raw/birds_front/contact_front_row.png`
- `art/raw/birds_front/contact_front_gray64.png`

manifest 删除四态的旧缩小例外备注及 acceptedOverrides 中对应的 bird_05 缩小例外，补充站姿尺寸、身体边界、尾部外露量与眨眼眼区记录。三张 contact 使用原 build_front.py 的预览部分重新生成，版式和尺寸未变；已目视检查混排、同种排列及实际 64px 灰度图。

开始时记录 birds_front/ 下 95 个文件 SHA-256，完成时只有上列 8 个文件指纹变化，另外 **87 个文件未变**，包括其余九种鸟、原 source 母版与 build_front.py。没有运行游戏或扩展测试，本任务只交付原画素材。

**旧 build_front.py/source 仍保留 T10d 母版与导出参数，按本任务范围未改。不要全量运行旧导出脚本覆盖新鸽；本轮仅执行其 contact 生成段。** 原始生成文件保留在下列内置生成目录；最终项目使用的四张 PNG 已复制进项目，不依赖生成目录。

## 生成方式与原始文件

全部原画和闭眼均使用内置 `image_gen.imagegen`，`transparent_background=true`；未使用 API CLI、外部图库或程序绘鸟。按协调者确认，仅用 Pillow 做等比缩放、透明画布平移、眼区合成及预览。站姿最终母版等比缩放到 774px 高；睡姿等比缩放至 550px 总宽；飞行等比缩放至 980px 总宽。沿用原流程清除 alpha<8 的边缘噪点与预乘 alpha 缩放。

原始目录：`/Users/apple/.codex/generated_images/01a1915b-3a54-7581-95c7-5a61f6ceafc1/`。

| 生成轮次 | 原文件 |
| --- | --- |
| 站姿首稿，弃用 | exec-e9f8373c-358a-4012-adc2-d833c8d361f8.png |
| 站姿收窄稿，弃用 | exec-0e4b468d-2be0-4b4a-8118-da8bdb4baba8.png |
| 最终站姿母版 | exec-55eb4012-feab-4406-9651-7b3229c0e602.png |
| 闭眼母版 | exec-7cb6fb1a-8346-41e0-8b2b-96621dd74206.png |
| 睡姿母版 | exec-cfd845a7-495d-44aa-9582-8372ac1a551b.png |
| 飞行母版 | exec-ebde0d85-c846-4835-a599-da349bc203e8.png |

睡姿首次请求因连接错误失败，使用相同提示词重试成功。未切换工具或模型。

## 完整提示词

### stand

```text
Use case: stylized-concept. Create one original front-facing standing fantail dove game sprite on a truly transparent square 1024x1024 canvas. Reference images 1 and 2 are approved bird_01 and bird_04 STYLE references: match their thick dark warm brown outlines, gentle painterly feather shading, upper-left lighting, cute large glossy eyes and polished mobile-game illustration. Reference image 3 is old dove IDENTITY ONLY: cream-white feathers, gray small beak, orange feet, gray heart-shaped patches on both folded wings, upright white fan tail behind. REPLACE the old squat body and wide fan. Exact new silhouette: tall upright head+body+feet 774 pixels high, head crown top y=97, soles y=870. Body including folded wings but EXCLUDING tail 470 pixels wide, from x=277 to747, centered x=512. Head is rounded with subtle feather tuft, not the cardinal's tall crest. Body is an upright pear oval, substantially taller than the old dove. Tail is a SMALL NARROW VERTICAL fan behind the body, visible only as thin upright fans at the shoulders, maximum entire sprite width 550 pixels (x=237..787). Each tail side extends only 40px beyond body; no sprawling horizontal semicircle, no huge halo. Tail top no higher than head top, so head remains highest visible feature. Make the tall body occupy the canvas as large as the cardinal, NOT a tiny dove. Wings folded at sides, front view symmetrical, two orange feet. No ground shadow, perch, text, props or border. Preserve enough margins and genuine alpha transparency.
```

### stand_correction

```text
Use case: precise-object-edit. Edit this front-facing cream fantail dove sprite. Keep exactly the same character, art style, cream feathers, gray heart patches, gray beak, orange feet, warm brown outlines, upper-left light and transparent background. ONE TARGETED CORRECTION: make the entire head and torso including both folded wings about 12% NARROWER while keeping its current HEIGHT unchanged. Make folded wings tuck closer to the torso with the outer feathers parallel to the body instead of flaring outward. Do NOT shrink the bird uniformly and do NOT shorten it. Keep the narrow upright tail fan behind the shoulders roughly its present total width, a little wider than the now narrower body, never spreading into a large semicircle. The desired height to body-width ratio is 1.65, tall pear-shaped standing bird. Preserve feet and facial expression, no new elements. Entire bird fully within square transparent canvas.
```

### stand_final_correction

```text
Use case: precise-object-edit. One precise proportion correction to this dove sprite. Keep HEIGHT and head, face, feet, all colors, patches, outline, shading, narrow upright fan tail and transparent background unchanged. WIDEN ONLY THE TORSO AND ITS FOLDED WINGS by 10 percent, making its pear-shaped chest and belly slightly fuller. Maintain tucked folded wings. The body excluding the tail should be approximately 56 percent as wide as the entire bird is tall from head tuft to soles. The tail width stays exactly as now, only a little wider than the torso. Do not widen the tail, do not make an enormous fan, do not change the character or pose. Same cute front-facing cream-white dove, gray heart wing patches, gray beak and orange feet.
```

### blink

```text
Use case: precise-object-edit. Make a BLINK animation variant of this exact front-facing dove game sprite. Edit ONLY the two eyes: replace the open glossy eyes with peacefully closed dark brown curved eyelid lines in the same eye positions. Remove open iris pixels and fill their region naturally with the same cream facial feathers. Every other pixel should remain unchanged: exact head shape, feather detail, beak, cheeks, body, heart wing patches, narrow upright tail, orange feet, pose, position, scale and lighting. Keep 1024x1024 transparent canvas, head top y97 and feet bottom y870. Do not reposition or redraw the body. No text.
```

### sleep

```text
Use case: precise-object-edit. Create the SLEEP pose of this same front-facing cream-white fantail dove game sprite, preserving its exact identity and illustration style: dark warm brown bold contour, upper-left light, cream feather shading, small gray beak, gray heart patches on folded wings, short narrow upright white tail fan behind shoulders. Change pose to a genuinely crouched sleeping pose: head lowers into shoulders, closed eyes as soft dark brown eyelid curves, belly lowered and resting over tucked legs, small orange toes just visible underneath. Low relaxed oval body with folded wings, not simply standing with shut eyes. Keep FRONT view symmetrical and body size consistent with this dove. On 1024x1024 transparent canvas center x512, feet/belly contact bottom y870, top of sleeping head about y270, total width around 500 pixels. Tail remains small and vertical, no giant wide fan. No shadow, perch, text, props or border.
```

### fly

```text
Use case: precise-object-edit. Create the FLYING animation pose of this exact cream-white fantail dove game character. Same front-facing identity, head tuft, shiny open eyes, small gray beak, orange feet tucked up under belly, cream feather body, dark gray HEART-SHAPED patch on EACH wing, small narrow upright white fan tail behind the body. Both wings fully extended outward and slightly upward, front-facing symmetric flight, wings show layered feathers and the heart patches clearly. Preserve the approved thick dark warm brown outline, gentle painterly shading and upper-left light. Single dove on transparent square 1024x1024 canvas, entire wingspan approximately 960px and strictly no more than 980px with margins at both edges, bird centered horizontally at x512 and vertically around y484. Do not crop feathers, do not shrink the central body excessively, no perch, shadow, text, props or border.
```

