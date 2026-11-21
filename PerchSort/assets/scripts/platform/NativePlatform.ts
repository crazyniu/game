// iOS and Android. Events go through Cocos' jsbBridgeWrapper to GameBridge (native/engine/ios/game/GameBridge.mm,
// native/engine/android/app/src/com/cocos/game/GameBridge.java); payloads and replies are JSON strings.
//
//   script -> native                    native -> script
//   game.init {ads: units}              game.ads {ready, off: no ads in this build}
//   game.rewarded {placement}           game.rewarded {result: done | skipped | unavailable}
//   game.interstitial {placement}       game.interstitial {shown}
//   game.vibrate {kind}
//   game.privacy {}                     (opens the ad consent form)
import { native, sys } from 'cc';
import { AdResult, Capability, Platform, PlatformConfig, VibrateKind, adUnits, loadPlatformConfig } from './Platform';

const EV = { init: 'game.init', ads: 'game.ads', rewarded: 'game.rewarded', interstitial: 'game.interstitial', vibrate: 'game.vibrate', privacy: 'game.privacy' };
/** Seconds to wait for a native reply; an ad that has not finished by then counts as not watched. */
const AD_TIMEOUT = 120;

export class NativePlatform extends Platform {
  readonly id = sys.os === sys.OS.IOS ? 'ios' : 'android';
  private cfg: PlatformConfig | null = null;
  private ready = false;
  private off = false;
  private waiting = new Map<string, (reply: Record<string, unknown>) => void>();

  override async init(): Promise<void> {
    this.cfg = await loadPlatformConfig();
    const bridge = native.jsbBridgeWrapper;
    bridge.addNativeEventListener(EV.ads, (arg: string) => {
      const r = parse(arg), was = this.ready;
      this.ready = r.ready === true;
      if (r.off === true) this.off = true;
      if (was !== this.ready) this.onAdsChange();
    });
    for (const ev of [EV.rewarded, EV.interstitial]) bridge.addNativeEventListener(ev, (arg: string) => {
      const done = this.waiting.get(ev);
      this.waiting.delete(ev);
      done?.(parse(arg));
    });
    bridge.dispatchEventToNative(EV.init, JSON.stringify({ ads: adUnits(this.cfg, this.id) }));
  }
  /** Send one request and wait for the reply of the same name; one ad at a time. */
  private ask(ev: string, payload: Record<string, unknown>): Promise<Record<string, unknown>> {
    return new Promise((resolve) => {
      if (this.waiting.has(ev)) { resolve({}); return; }
      const timer = setTimeout(() => { this.waiting.delete(ev); resolve({}); }, AD_TIMEOUT * 1000);
      this.waiting.set(ev, (reply) => { clearTimeout(timer); resolve(reply); });
      native.jsbBridgeWrapper.dispatchEventToNative(ev, JSON.stringify(payload));
    });
  }
  override adsAvailable(): boolean { return this.ready; }
  override async showRewarded(placement: string): Promise<AdResult> {
    if (!this.ready) return 'unavailable';
    const result = (await this.ask(EV.rewarded, { placement })).result;
    return result === 'done' || result === 'skipped' ? result : 'unavailable';
  }
  override async showInterstitial(placement: string): Promise<boolean> {
    return this.ready && (await this.ask(EV.interstitial, { placement })).shown === true;
  }
  override vibrate(kind: VibrateKind): void { native.jsbBridgeWrapper.dispatchEventToNative(EV.vibrate, JSON.stringify({ kind })); }
  override openPrivacyOptions(): void { native.jsbBridgeWrapper.dispatchEventToNative(EV.privacy, '{}'); }
  override openPrivacyPolicy(): void { if (this.cfg?.privacyPolicy) sys.openURL(this.cfg.privacyPolicy); }
  override hasCapability(cap: Capability): boolean {
    const ads = !this.off && this.cfg !== null && adUnits(this.cfg, this.id).rewarded !== '';
    if (cap === 'ads' || cap === 'privacy_options') return ads;
    if (cap === 'privacy_policy') return !!this.cfg?.privacyPolicy;
    return cap === 'vibrate';
  }
}

function parse(arg: string): Record<string, unknown> {
  try {
    const v: unknown = JSON.parse(arg || '{}');
    return typeof v === 'object' && v !== null ? v as Record<string, unknown> : {};
  } catch {
    return {};
  }
}
