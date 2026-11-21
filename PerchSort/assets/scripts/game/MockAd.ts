import { BlockInputEvents, Node, tween } from 'cc';
import { UI } from './UI';
import type { AdResult } from '../platform/Platform';
import type { MockAdView } from '../platform/DebugPlatform';

/**
 * Debug builds only: a stand-in ad with no words. Rewarded: a countdown and a close cross; closing before the countdown
 * ends counts as not watched. Interstitial: the cross appears after one second.
 */
export class MockAd implements MockAdView {
  static readonly SECONDS = 5;
  constructor(private host: Node) {}
  show(kind: 'rewarded' | 'interstitial'): Promise<AdResult> {
    return new Promise((resolve) => {
      const root = UI.group(this.host, 'MockAd');
      root.addComponent(BlockInputEvents);
      UI.fill(root, 'Shade', [0, 0, 1080, 1920], '#101820');
      UI.cover(root, 'ad_mock', [0, 0, 1080, 1920]);
      let left = kind === 'rewarded' ? MockAd.SECONDS : 0;
      const counter = UI.group(root, 'Countdown', [48, 56, 120, 72]);
      const close = UI.button(root, 'Close', 'btn_purchase_close', [940, 40, 116, 119], () => {
        root.destroy();
        resolve(kind === 'rewarded' && left >= 0 ? 'skipped' : 'done');
      });
      const tick = () => {
        if (!root.isValid) return;
        UI.clear(counter);
        if (left > 0) UI.digits(counter, left, [0, 0, 120, 72]);
        if (left-- > 0) tween(root).delay(1).call(tick).start();
      };
      if (kind === 'rewarded') tick();
      else { close.active = false; tween(root).delay(1).call(() => { close.active = true; }).start(); }
    });
  }
}
