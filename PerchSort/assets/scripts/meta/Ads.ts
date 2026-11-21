import { Profile } from './Profile';

/** Interstitial pacing for one session. */
export interface AdPacing { lastShown: number; levelsSince: number; lastRewarded: number }

/**
 * Ad placements and their rules (docs/work-log/ads/spec.md). Rewards are granted only after the platform reports a
 * rewarded ad watched to the end; entries are hidden when the build has no ads or none is loaded.
 */
export class Ads {
  /** Rewarded placements. */
  static readonly DOUBLE_COINS = 'rv_double_coin';
  static readonly TOOL = 'rv_tool';
  static readonly UNDO = 'rv_undo';
  static readonly FREE_COINS = 'rv_free_coin';
  static readonly DRAW = 'rv_draw';
  static readonly DAILY_DOUBLE = 'rv_daily_double';
  static readonly LEVEL_END = 'int_level_end';
  /** Placements with a daily cap; the others are unlimited. */
  static readonly DAILY_CAP: Record<string, number> = { rv_free_coin: 5, rv_draw: 3 };
  static readonly FREE_COIN_AMOUNT = 60;
  static readonly INTERSTITIAL = { minLevel: 8, minSeconds: 90, minLevelsBetween: 2, skipAfterRewardedSeconds: 60 };

  private static day(p: Profile, today: string): void {
    if (p.ad_date !== today) { p.ad_date = today; p.ad_views = {}; }
  }
  /** Views left today; Infinity for uncapped placements. */
  static left(p: Profile, placement: string, today: string): number {
    const cap = this.DAILY_CAP[placement];
    if (cap === undefined) return Infinity;
    return p.ad_date === today ? Math.max(0, cap - (p.ad_views[placement] ?? 0)) : cap;
  }
  /** Count one completed rewarded view. */
  static record(p: Profile, placement: string, today: string): void {
    this.day(p, today);
    p.ad_views[placement] = (p.ad_views[placement] ?? 0) + 1;
  }
  static pacing(): AdPacing { return { lastShown: -Infinity, levelsSince: Infinity, lastRewarded: -Infinity }; }
  /** After a cleared level closes its win popup: is an interstitial due? Times in seconds. */
  static interstitial_due(s: AdPacing, level: number, now: number): boolean {
    const r = this.INTERSTITIAL;
    return level >= r.minLevel && now - s.lastShown >= r.minSeconds && s.levelsSince >= r.minLevelsBetween && now - s.lastRewarded >= r.skipAfterRewardedSeconds;
  }
}
