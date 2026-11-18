// WeChat's game runtime has no browser microtask or vibration bridge.
if (typeof globalThis.queueMicrotask !== 'function') {
  globalThis.queueMicrotask = callback => Promise.resolve().then(callback);
}
globalThis.navigator.vibrate = duration => {
  if (duration > 0) wx.vibrateShort({type: duration >= 40 ? 'medium' : 'light'});
  return true;
};
