import { sys } from 'cc';
import { Art } from './Art';

/**
 * Screen text: key -> the current language's words (resources/i18n/strings.json, built from i18n/strings.csv by tools/ui/strings.py).
 * Languages and fallbacks follow fishsort: traditional Chinese falls back to simplified, every other language to English.
 */
export class I18n {
  /** Each language in its own words (the settings list; never translated). */
  static readonly NAMES: Record<string, string> = {
    zh_CN: '简体中文', zh_TW: '繁體中文', en: 'English', ja: '日本語', ko: '한국어', de: 'Deutsch',
    fr: 'Français', es: 'Español', pt: 'Português', it: 'Italiano', ru: 'Русский', tr: 'Türkçe',
  };
  /** Short names for the two-language switch (WeChat). */
  static readonly SHORT: Record<string, string> = { zh_CN: '中文', en: 'English' };
  static lang = 'en';
  private static table(): Record<string, Record<string, string>> { return Art.data('i18n/strings'); }
  /** The languages this build offers: WeChat carries simplified Chinese and English, the apps and the web all of them. */
  static languages(): string[] { return sys.platform === sys.Platform.WECHAT_GAME ? ['zh_CN', 'en'] : Object.keys(this.NAMES); }
  /** A saved or system language code -> one we carry ('zh' and 'zh-Hans' are simplified, 'zh-TW' and 'zh-Hant' traditional). */
  static match(code: string): string {
    const s = (code || '').toLowerCase().replace(/_/g, '-'), main = s.split('-')[0], offered = this.languages();
    let want = main;
    if (main === 'zh') want = /hant|-tw|-hk|-mo/.test(s) ? 'zh_TW' : 'zh_CN';
    if (offered.includes(want)) return want;
    if (want === 'zh_TW' && offered.includes('zh_CN')) return 'zh_CN';
    return offered.includes('en') ? 'en' : offered[0];
  }
  static set(code: string): void { this.lang = this.match(code); Art.setLocale(this.lang); }
  /** The words for a key; %d and %s take the arguments in order. */
  static tr(key: string, ...args: (string | number)[]): string {
    const t = this.table();
    const s = t[this.lang]?.[key] || (this.lang === 'zh_TW' ? t.zh_CN?.[key] : undefined) || t.en?.[key] || t.zh_CN?.[key];
    if (s === undefined) throw new Error(`Missing text ${this.lang}:${key}`);
    let i = 0;
    return s.replace(/%[ds]/g, () => String(args[i++] ?? ''));
  }
}
export const tr = (key: string, ...args: (string | number)[]): string => I18n.tr(key, ...args);
