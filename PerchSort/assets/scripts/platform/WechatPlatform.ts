// WeChat mini game. Only WeChat's own ads (流量主); the units come from game.json ads.wechat and stay empty until
// the account is approved, which hides every ad entry. AdMob ids are never used here.
import { AdResult, Capability, Platform, VibrateKind, adUnits, loadPlatformConfig } from './Platform';

/* eslint-disable @typescript-eslint/no-explicit-any */
declare const wx: any;

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
  override showRewarded(_placement: string): Promise<AdResult> {
    const ad = this.rewarded;
    if (!this.adsAvailable()) return Promise.resolve('unavailable');
    this.playing = true;
    this.setLoaded(false);
    return new Promise((resolve) => {
      let settled = false;
      const finish = (result: AdResult) => {
        if (settled) return;
        settled = true;
        ad.offClose(onClose);
        ad.offError(onError);
        this.playing = false;
        ad.load().catch(() => this.setLoaded(false));
        resolve(result);
      };
      const onError = () => finish('unavailable');
      const onClose = (res?: { isEnded?: boolean }) => finish(res?.isEnded === true ? 'done' : 'skipped');
      ad.onClose(onClose);
      ad.onError(onError);
      ad.show().catch(onError);
    });
  }
  override async showInterstitial(_placement: string): Promise<boolean> {
    const ad = this.interstitial;
    if (!ad) return false;
    return new Promise((resolve) => {
      const onClose = () => { ad.offClose(onClose); resolve(true); };
      ad.onClose(onClose);
      ad.show().catch(() => { ad.offClose(onClose); resolve(false); });
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
