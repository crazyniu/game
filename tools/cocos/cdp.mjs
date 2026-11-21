#!/usr/bin/env node
// Debugging through the player window opened by ./run.sh (Chrome with --remote-debugging-port, CDP_PORT, default 9333).
// Usage: node tools/cocos/cdp.mjs logs [seconds]          reload the page and print console output and exceptions for that long
//        node tools/cocos/cdp.mjs eval '<expression>'      evaluate in the page and print JSON (window.perch exists in debug builds)
//        node tools/cocos/cdp.mjs shot <out.jpg> [x,y,w,h] capture at device scale 2, optionally a region in 0..1 fractions
//        node tools/cocos/cdp.mjs burst <dir> <count> <interval ms> '<JS>'  run JS, then capture a timed frame sequence
//        node tools/cocos/cdp.mjs watch [log]               ping every 2 s; when the page is unresponsive for 4 s, pause it, log the
//                                                           running call stack three times, resume; also logs exceptions. Runs until Ctrl+C.
// Node built-in fetch and WebSocket only.
const PORT = Number(process.env.CDP_PORT ?? 9333);
const [cmd = 'logs', arg = '6'] = process.argv.slice(2);

const pages = await (await fetch(`http://127.0.0.1:${PORT}/json`)).json();
const page = pages.find((p) => p.type === 'page' && /^http:\/\/(localhost|127\.0\.0\.1)[:/]/.test(p.url));
if (!page) {
  console.error('No game page in the player window', pages.map((p) => p.url));
  process.exit(1);
}
const ws = new WebSocket(page.webSocketDebuggerUrl);
let id = 0;
const waiting = new Map();
const send = (method, params = {}) =>
  new Promise((resolve) => {
    const n = ++id;
    waiting.set(n, resolve);
    ws.send(JSON.stringify({ id: n, method, params }));
  });
const fmt = (a) => (a.type === 'string' ? a.value : a.description ?? JSON.stringify(a.value));
ws.onmessage = (m) => {
  const d = JSON.parse(m.data);
  if (d.id && waiting.has(d.id)) {
    waiting.get(d.id)(d.result ?? d.error);
    waiting.delete(d.id);
  } else if (d.method === 'Runtime.consoleAPICalled') {
    console.log(`[${d.params.type}]`, d.params.args.map(fmt).join(' '));
  } else if (d.method === 'Runtime.exceptionThrown') {
    const e = d.params.exceptionDetails;
    console.log('[exception]', e.exception?.description ?? e.text);
  }
};
await new Promise((r) => (ws.onopen = r));
await send('Runtime.enable');
if (cmd === 'watch') {
  const { appendFileSync } = await import('node:fs');
  const out = process.argv[3] ?? 'build/cdp-watch.log';
  const log = (t) => {
    const line = `${new Date().toISOString()} ${t}`;
    console.log(line);
    appendFileSync(out, line + '\n');
  };
  let pausedStack = null;
  const prev = ws.onmessage;
  ws.onmessage = (m) => {
    const d = JSON.parse(m.data);
    if (d.method === 'Debugger.paused') {
      pausedStack = d.params.callFrames.slice(0, 25).map((f) => `    ${f.functionName || '(anonymous)'}  ${f.url.split('/').slice(-2).join('/')}:${f.location.lineNumber + 1}:${f.location.columnNumber + 1}`).join('\n');
      return;
    }
    if (d.method === 'Runtime.exceptionThrown') {
      const e = d.params.exceptionDetails;
      log(`[exception] ${e.exception?.description ?? e.text}`);
      return;
    }
    prev(m);
  };
  await send('Debugger.enable');
  log(`Watching ${page.url}; log: ${out}`);
  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
  const ping = () => Promise.race([send('Runtime.evaluate', { expression: 'Date.now()', returnByValue: true }).then(() => true), sleep(4000).then(() => false)]);
  for (;;) {
    await sleep(2000);
    if (await ping()) continue;
    log('Page unresponsive for 4 s; pausing to read the stack');
    for (let k = 1; k <= 3; k++) {
      pausedStack = null;
      ws.send(JSON.stringify({ id: ++id, method: 'Debugger.pause' }));
      for (let w = 0; w < 50 && pausedStack === null; w++) await sleep(100);
      log(`Stack at pause ${k}:\n${pausedStack ?? '    (did not pause)'}`);
      ws.send(JSON.stringify({ id: ++id, method: 'Debugger.resume' }));
      await sleep(1000);
    }
    while (!(await ping())) await sleep(5000);
    log('Page responsive again');
  }
}
if (cmd === 'eval') {
  const r = await send('Runtime.evaluate', { expression: arg, returnByValue: true, awaitPromise: true });
  console.log(JSON.stringify(r.result?.value ?? r.exceptionDetails ?? r, null, 1));
  process.exit(0);
}
if (cmd === 'shot') {
  // Render at device scale 2 regardless of the display, saved as JPEG.
  const { writeFileSync } = await import('node:fs');
  const vp = (await send('Runtime.evaluate', { expression: '[innerWidth, innerHeight]', returnByValue: true })).result.value;
  await send('Emulation.setDeviceMetricsOverride', { width: vp[0], height: vp[1], deviceScaleFactor: 2, mobile: false });
  await new Promise((r) => setTimeout(r, 900));
  const [fx, fy, fw, fh] = (process.argv[4] ?? '0,0,1,1').split(',').map(Number);
  const clip = { x: vp[0] * fx, y: vp[1] * fy, width: vp[0] * fw, height: vp[1] * fh, scale: 1 };
  const r = await send('Page.captureScreenshot', { format: 'jpeg', quality: 90, clip });
  writeFileSync(arg, Buffer.from(r.data, 'base64'));
  await send('Emulation.clearDeviceMetricsOverride');
  console.log(`SHOT_DONE ${arg}`);
  process.exit(0);
}
if (cmd === 'burst') {
  // Frame sequence for motion review: <dir>/f00.jpg, f01.jpg, ... (BURST_SCALE, default 0.36).
  const { mkdirSync, writeFileSync } = await import('node:fs');
  const [, dir, n = '10', ms = '120', js = ''] = process.argv.slice(2);
  mkdirSync(dir, { recursive: true });
  const vp = (await send('Runtime.evaluate', { expression: '[innerWidth, innerHeight]', returnByValue: true })).result.value;
  const clip = { x: 0, y: 0, width: vp[0], height: vp[1], scale: Number(process.env.BURST_SCALE ?? 0.36) };
  if (js) await send('Runtime.evaluate', { expression: js });
  const t0 = Date.now();
  for (let i = 0; i < Number(n); i++) {
    const wait = t0 + i * Number(ms) - Date.now();
    if (wait > 0) await new Promise((r) => setTimeout(r, wait));
    const r = await send('Page.captureScreenshot', { format: 'jpeg', quality: 62, clip });
    writeFileSync(`${dir}/f${String(i).padStart(2, '0')}.jpg`, Buffer.from(r.data, 'base64'));
  }
  console.log(`BURST_DONE ${dir} ${Date.now() - t0}ms`);
  process.exit(0);
}
await send('Page.enable');
await send('Page.reload', { ignoreCache: true });
setTimeout(() => process.exit(0), Number(arg) * 1000);
