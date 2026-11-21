// Platform capabilities. Only platform/ touches wx, the native bridge or an ad SDK; the game asks hasCapability() and
// never branches on the platform id. The base class is "no platform": no ads, no vibration, no privacy entries.
import { JsonAsset, resources } from 'cc';
import { DEBUG } from 'cc/env';

export type AdResult = 'done' | 'skipped' | 'unavailable';
/** ads: this build has ads (game.json ads.enabled, ad units configured, SDK linked). privacy_options: the ad consent form (UMP). */
export type Capability = 'ads' | 'vibrate' | 'privacy_options' | 'privacy_policy';
export type VibrateKind = 'light' | 'medium' | 'heavy';

export interface AdUnits { rewarded: string; interstitial: string }
/** resources/config/platform.json, written from the repository's game.json by tools/native/apply.mjs. */
export interface PlatformConfig {
  name: Record<string, string>;
  version: string;
  adsEnabled: boolean;
  privacyPolicy: string;
  ads: Record<string, AdUnits>;
}

let config: Promise<PlatformConfig> | null = null;
export function loadPlatformConfig(): Promise<PlatformConfig> {
  config ??= new Promise<PlatformConfig>((resolve, reject) => resources.load('config/platform', JsonAsset, (e, a) => e ? reject(e) : resolve(a.json as PlatformConfig)))
    .catch(error => { config = null; throw error; });
  return config;
}
/** This platform's ad units; all empty when the master switch is off. */
export function adUnits(cfg: PlatformConfig, platform: string): AdUnits {
  return cfg.adsEnabled ? cfg.ads[platform] ?? { rewarded: '', interstitial: '' } : { rewarded: '', interstitial: '' };
}

export abstract class Platform {
  abstract readonly id: string;
  /** Editor preview and debug builds (./run.sh). Every ./package.sh package is a release build. */
  isDevBuild(): boolean { return DEBUG; }
  async init(): Promise<void> {}
  /** Rewarded video. 'done' only when the SDK reports the ad watched to the end; grant rewards only then. */
  async showRewarded(_placement: string): Promise<AdResult> { return 'unavailable'; }
  /** Interstitial. true when one was actually shown. */
  async showInterstitial(_placement: string): Promise<boolean> { return false; }
  /** An ad loaded, finished or consent changed: the game redraws ad entries. */
  onAdsChange: () => void = () => undefined;
  /** An ad can be shown now. Ad entries appear only when hasCapability('ads') and this are both true. */
  adsAvailable(): boolean { return false; }
  vibrate(_kind: VibrateKind): void {}
  openPrivacyOptions(): void {}
  openPrivacyPolicy(): void {}
  hasCapability(_cap: Capability): boolean { return false; }
}
