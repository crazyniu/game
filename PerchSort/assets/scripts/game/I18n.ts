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
/** Earlier painted-word ids (lettering, sign and button words) -> their text keys. */
const FIXED: Record<string, string> = {
  popup_level_title: 'T_LEVEL', popup_new_friend_title: 'T_NEW_FRIEND', popup_world_complete_title: 'T_WORLD_COMPLETE', popup_world_unlock_title: 'T_WORLD_UNLOCK',
  popup_gift_title: 'T_GIFT', win_title: 'T_WIN', daily_reward_title: 'T_DAILY_REWARD', lucky_spin_title: 'T_DRAW', star_chest_title: 'T_STAR_CHEST',
  coin_shop_title: 'FREE_FEATHERS', coins_buy: 'FREE_FEATHERS', undo_more: 'T_UNDO_MORE', stuck: 'T_STUCK', tutorial_title: 'T_TUTORIAL', tasks_title: 'UI_TAB_TASKS',
  aviary: 'T_AVIARY', tab_boosters: 'T_TOOLS', tab_outfits: 'T_OUTFITS', tab_nests: 'T_HOUSES',
  white_action_ok: 'UI_OK', white_action_next: 'UI_NEXT_LEVEL', action_next: 'UI_NEXT_LEVEL', white_action_replay: 'UI_REPLAY', white_settings_home: 'H_TO_HOME',
  white_claim: 'UI_CLAIM', white_spin: 'UI_SPIN', white_open: 'UI_OPEN', white_play: 'UI_PLAY', white_buy: 'UI_BUY', white_equip: 'UI_EQUIP',
  white_equipped: 'UI_EQUIPPED', white_free_rewind: 'UI_FREE_REWIND', white_tutorial_next: 'UI_NEXT', white_help: 'H_HELP',
  'action.got_it': 'UI_GOT_IT', 'chest.line': 'CHEST_LINE', 'daily_reward.title': 'T_DAILY_REWARD', 'tasks.title': 'UI_TAB_TASKS', 'draw.title': 'T_DRAW',
  task_today: 'UI_TODAY', task_week: 'UI_WEEK', 'tutorial.pick': 'TUTORIAL_PICK', 'tutorial.move.a': 'TUTORIAL_MOVE', 'tutorial.home.a': 'TUTORIAL_HOME', 'tutorial.win.a': 'TUTORIAL_WIN',
};
const TOOL: Record<string, string> = { firefly: 'FIREFLY', sprout: 'SPROUT', owl_eye: 'OWL_EYE', wake_bell: 'WAKE_BELL', scarecrow: 'SCARECROW', spring_rain: 'SPRING_RAIN' };
/** The text key of an earlier painted-word id; unknown ids fail loudly. */
export function wordKey(id: string): string {
  id = id.replace(/^lettering_/, '');
  if (FIXED[id]) return FIXED[id];
  let m: RegExpMatchArray | null;
  if ((m = id.match(/^bird_(\w+)$/))) return 'BIRD_' + m[1].toUpperCase();
  if ((m = id.match(/^world_(\w+)$/))) return 'WORLD_' + m[1].toUpperCase();
  if ((m = id.match(/^outfit_(\w+)_(\d)$/))) return `OUTFIT_${m[1].toUpperCase()}_${m[2]}`;
  if ((m = id.match(/^item_(\w+)$/))) return 'TOOL_' + TOOL[m[1]];
  if ((m = id.match(/^cap_(\w+)$/))) return `TOOL_${TOOL[m[1]]}_DESC`;
  if ((m = id.match(/^house_(\w+)$/))) return 'HOUSE_' + m[1].toUpperCase();
  if ((m = id.match(/^lesson_(\w+)_title$/))) return 'LESSON_' + m[1].toUpperCase();
  if ((m = id.match(/^lesson\.(\w+)\.line$/))) return `LESSON_${m[1].toUpperCase()}_LINE`;
  if ((m = id.match(/^bird\.(\w+)\.ability$/))) return 'ABILITY_' + m[1].toUpperCase();
  if ((m = id.match(/^task_(\w+)$/))) return 'TASK_' + m[1].toUpperCase();
  throw new Error(`No text key for ${id}`);
}
export const word = (id: string): string => I18n.tr(wordKey(id));
export const tr =(key: string, ...args: (string | number)[]): string => I18n.tr(key, ...args);
