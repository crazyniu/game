// Pick the platform at startup: WeChat mini game, iOS/Android package, otherwise the browser (debug).
import { sys } from 'cc';
import { Platform } from './Platform';
import { DebugPlatform } from './DebugPlatform';
import { NativePlatform } from './NativePlatform';
import { WechatPlatform, inWechat } from './WechatPlatform';

export function createPlatform(): Platform {
  if (inWechat()) return new WechatPlatform();
  if (sys.isNative && (sys.os === sys.OS.IOS || sys.os === sys.OS.ANDROID)) return new NativePlatform();
  return new DebugPlatform();
}
