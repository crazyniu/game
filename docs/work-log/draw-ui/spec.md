# Draw UI restoration

Restore the authorized draw UI after workspace loss. Scope: draw heading and English action lettering only. Preserve all draw limits, prize data, callbacks, save and reward behavior.

- Heading: centered painted visible ink within (260,224,560,100), matching current shop heading box.
- English: Lucky Spin / Spin. Chinese: 每日抽奖 / 抽奖.
- Reuse existing English heading and Chinese action. Missing Chinese heading and English complete action asset generated separately using built-in image generation, checked visually letter by letter before wiring.
- Preserve original image assets. No design-image cropping.
