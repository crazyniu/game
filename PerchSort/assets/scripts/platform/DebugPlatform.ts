// Browser play and captures. Ads follow game.json's master switch; ?ads=1 or ?ads=0 overrides it to view the other build.
// The mock ad screen is supplied by the game layer (MockAd.ts); nothing here grants a reward.
import { sys } from 'cc';
import { AdResult, Capability, Platform, VibrateKind, loadPlatformConfig } from './Platform';

export interface MockAdView { show(kind: 'rewarded' | 'interstitial'): Promise<AdResult> }

export class DebugPlatform extends Platform {
  readonly id = 'debug';
  hasAds = false;
  /** false simulates "no ad loaded yet" (set from the console: perch.platform.adsOn=false). */
  adsOn = false;
  view: MockAdView | null = null;
  private privacyUrl = '';

  override async init(): Promise<void> {
    const cfg = await loadPlatformConfig();
    const q = new URLSearchParams(globalThis.location?.search ?? '').get('ads');
    this.hasAds = this.isDevBuild() && (q === '1' ? true : q === '0' ? false : cfg.adsEnabled);
    this.adsOn = this.hasAds;
    this.privacyUrl = cfg.privacyPolicy;
  }
  override adsAvailable(): boolean { return this.adsOn && this.view !== null; }
  override async showRewarded(_placement: string): Promise<AdResult> { return this.adsAvailable() ? this.view!.show('rewarded') : 'unavailable'; }
  override async showInterstitial(_placement: string): Promise<boolean> { return this.adsAvailable() && (await this.view!.show('interstitial')) === 'done'; }
  override vibrate(kind: VibrateKind): void {
    const nav = globalThis.navigator as (Navigator & { vibrate?: (ms: number) => boolean }) | undefined;
    nav?.vibrate?.(kind === 'light' ? 12 : kind === 'medium' ? 25 : 45);
  }
  override openPrivacyPolicy(): void { if (this.privacyUrl) sys.openURL(this.privacyUrl); }
  override hasCapability(cap: Capability): boolean {
    if (cap === 'ads') return this.hasAds;
    if (cap === 'privacy_policy') return this.privacyUrl !== '';
    return cap === 'vibrate';
  }
}
