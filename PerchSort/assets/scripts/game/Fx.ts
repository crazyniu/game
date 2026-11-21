import { Color, Node, Sprite, UIOpacity, Vec3, tween } from 'cc';
import { UI } from './UI';
import { Kit } from './Popups';

/**
 * Effects from fishsort's ui/fx.ts (glow pop, rings, sparkles, screen flash), on fishsort's pictures in art/flat.
 * Points are this game's 1080 x 1920 coordinates from the top left; every effect removes itself.
 */
export class Fx {
  /** A picture centred on (x, y) with a centred pivot, tinted. */
  static piece(layer: Node, key: string, x: number, y: number, w: number, h: number, color?: string): Node {
    const n = UI.image(layer, 'art/flat/' + key, [x - w / 2, y - h / 2, w, h]);
    Kit.centerPivot(n);
    if (color) n.getComponent(Sprite)!.color = new Color().fromHEX(color);
    return n;
  }
  private static fade(n: Node, from: number, delay: number, dur: number): UIOpacity {
    const op = n.getComponent(UIOpacity) ?? n.addComponent(UIOpacity); op.opacity = from;
    tween(op).delay(delay).to(dur, { opacity: 0 }).call(() => { if (n.isValid) n.destroy(); }).start();
    return op;
  }
  /** A soft glow that swells and fades. */
  static glowPop(layer: Node, x: number, y: number, s: number, color?: string, dur = .35): void {
    if (Kit.instant) return;
    const n = this.piece(layer, 'fx_glow', x, y, s, s, color); n.setScale(.3, .3, 1);
    tween(n).to(dur * .4, { scale: new Vec3(1, 1, 1) }, { easing: 'quadOut' }).start();
    this.fade(n, 255, dur * .4, dur * .6);
  }
  /** A ring that widens from `from` to `to` and fades. */
  static ring(layer: Node, x: number, y: number, from: number, to: number, color?: string, dur = .45): void {
    if (Kit.instant) return;
    const n = this.piece(layer, 'fx_ring', x, y, to, to, color); n.setScale(from / to, from / to, 1);
    tween(n).to(dur, { scale: new Vec3(1, 1, 1) }, { easing: 'quadOut' }).start();
    this.fade(n, 230, dur * .3, dur * .7);
  }
  /** Sparks flying out in a circle. */
  static sparkle(layer: Node, x: number, y: number, count = 10, r = 160, color?: string, size = 48): void {
    if (Kit.instant) return;
    for (let i = 0; i < count; i++) {
      const a = i / count * Math.PI * 2 + Math.random() * .5, d = r * (.6 + Math.random() * .5), s = size * (.6 + Math.random() * .6);
      const n = this.piece(layer, 'fx_spark', x, y, s, s, color), p = n.position;
      tween(n).to(.5, { position: new Vec3(p.x + Math.cos(a) * d, p.y + Math.sin(a) * d, 0), angle: 180 }, { easing: 'quadOut' }).start();
      this.fade(n, 255, .25, .3);
    }
  }
  /** The whole screen flashes a colour. */
  static flash(layer: Node, color = '#FFFFFF', peak = 150): void {
    if (Kit.instant) return;
    const n = UI.fill(layer, 'Flash', [0, 0, 1080, 1920], color);
    const op = n.getComponent(UIOpacity) ?? n.addComponent(UIOpacity); op.opacity = 0;
    tween(op).to(.06, { opacity: peak }).to(.3, { opacity: 0 }).call(() => { if (n.isValid) n.destroy(); }).start();
  }
}
