// WeChat mini game. Only WeChat's own ads (流量主); the units come from game.json ads.wechat and stay empty until
// the account is approved, which hides every ad entry. AdMob ids are never used here.
import { AdResult, Capability, Platform, VibrateKind, adUnits, loadPlatformConfig } from './Platform';

/* eslint-disable @typescript-eslint/no-explicit-any */
declare const wx: any;

const SHOW_TIMEOUT_MS = 10_000;
const REWARDED_CLOSE_TIMEOUT_MS = 120_000;
const INTERSTITIAL_CLOSE_TIMEOUT_MS = 30_000;

export function inWechat(): boolean { return typeof wx !== 'undefined' && typeof wx.getSystemInfoSync === 'function'; }

export class WechatPlatform extends Platform {
  readonly id = 'wechat';
  private rewarded: any = null;
  private interstitial: any = null;
  private loaded = false;
  private playing = false;

  override async init(): Promise<void> {
    const units = adUnits(await loadPlatformConfig(), 'wechat');
    if (units.rewarded && typeof wx.createRewardedVideoAd === 'function') {
      this.rewarded = wx.createRewardedVideoAd({ adUnitId: units.rewarded });
      this.rewarded.onLoad(() => this.setLoaded(true));
      this.rewarded.onError(() => this.setLoaded(false));
      const preload = () => { if (!this.loaded && !this.playing) this.rewarded.load().catch(() => this.setLoaded(false)); };
      preload();
      wx.onShow(preload);
    }
    if (units.interstitial && typeof wx.createInterstitialAd === 'function') {
      this.interstitial = wx.createInterstitialAd({ adUnitId: units.interstitial });
      this.interstitial.onError(() => undefined);
    }
  }
  private setLoaded(on: boolean): void {
    const was = this.loaded;
    this.loaded = on;
    if (was !== on) this.onAdsChange();
  }
  override adsAvailable(): boolean { return this.rewarded !== null && this.loaded && !this.playing; }
  override async showRewarded(_placement: string): Promise<AdResult> {
    const ad = this.rewarded;
    if (!this.adsAvailable()) return 'unavailable';
    this.playing = true;
    this.setLoaded(false);
    try {
      return await this.play<AdResult>(ad, res => res?.isEnded === true ? 'done' : 'skipped', 'unavailable', REWARDED_CLOSE_TIMEOUT_MS);
    } finally {
      this.playing = false;
      void Promise.resolve().then(() => ad.load()).catch(() => this.setLoaded(false));
    }
  }
  override async showInterstitial(_placement: string): Promise<boolean> {
    if (!this.interstitial) return false;
    return this.play<boolean>(this.interstitial, () => true, false, INTERSTITIAL_CLOSE_TIMEOUT_MS);
  }
  /** A missing callback or SDK error is not a completed ad. Start the close timeout only once show succeeds. */
  private play<T>(ad: any, resultOnClose: (res?: { isEnded?: boolean }) => T, unavailable: T, closeTimeout: number): Promise<T> {
    return new Promise(resolve => {
      let settled = false;
      const finish = (result: T) => {
        if (settled) return;
        settled = true;
        clearTimeout(timer);
        ad.offClose(onClose);
        ad.offError(onError);
        resolve(result);
      };
      const onClose = (res?: { isEnded?: boolean }) => finish(resultOnClose(res));
      const onError = () => finish(unavailable);
      let timer = setTimeout(onError, SHOW_TIMEOUT_MS);
      ad.onClose(onClose);
      ad.onError(onError);
      void Promise.resolve().then(() => ad.show()).then(() => {
        if (settled) return;
        clearTimeout(timer);
        timer = setTimeout(onError, closeTimeout);
      }, onError);
    });
  }
  override vibrate(kind: VibrateKind): void {
    if (kind === 'heavy') wx.vibrateLong?.({});
    else wx.vibrateShort?.({ type: kind === 'light' ? 'light' : 'medium' });
  }
  override openPrivacyPolicy(): void { wx.openPrivacyContract?.({}); }
  override hasCapability(cap: Capability): boolean {
    if (cap === 'ads') return this.rewarded !== null;
    if (cap === 'privacy_policy') return typeof wx.openPrivacyContract === 'function';
    return cap === 'vibrate';
  }
}
