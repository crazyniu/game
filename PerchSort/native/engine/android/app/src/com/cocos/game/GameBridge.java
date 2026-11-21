// Bridge between scripts and Android; the event table is at the top of assets/scripts/platform/NativePlatform.ts
// (the same events as iOS GameBridge.mm). Ads live in GameAds: app/ads/on (AdMob) or app/ads/off (no ads), chosen by
// app/build.gradle GAME_ADS.
package com.cocos.game;

import android.app.Activity;
import android.os.Build;
import android.os.VibrationEffect;
import android.os.Vibrator;

import com.cocos.lib.CocosHelper;
import com.cocos.lib.JsbBridgeWrapper;

import org.json.JSONException;
import org.json.JSONObject;

public final class GameBridge {
    private static Activity activity;

    private GameBridge() {}

    /** Called once from AppActivity.onCreate to register the events. */
    public static void setup(Activity act) {
        activity = act;
        JsbBridgeWrapper bridge = JsbBridgeWrapper.getInstance();
        bridge.addScriptEventListener("game.init", arg -> ui(() -> GameAds.start(activity, parse(arg).optJSONObject("ads"))));
        bridge.addScriptEventListener("game.rewarded", arg -> ui(GameAds::showRewarded));
        bridge.addScriptEventListener("game.interstitial", arg -> ui(GameAds::showInterstitial));
        bridge.addScriptEventListener("game.privacy", arg -> ui(GameAds::openPrivacyOptions));
        bridge.addScriptEventListener("game.vibrate", arg -> ui(() -> vibrate(parse(arg).optString("kind", "medium"))));
    }

    private static void vibrate(String kind) {
        Vibrator v = (Vibrator) activity.getSystemService(Activity.VIBRATOR_SERVICE);
        if (v == null || !v.hasVibrator()) return;
        int ms = "light".equals(kind) ? 12 : "heavy".equals(kind) ? 45 : 25;
        if (Build.VERSION.SDK_INT >= 26) v.vibrate(VibrationEffect.createOneShot(ms, VibrationEffect.DEFAULT_AMPLITUDE));
        else v.vibrate(ms);
    }

    private static void ui(Runnable r) {
        activity.runOnUiThread(r);
    }

    /** Reply to the script on the game thread, where scripts run. */
    static void send(String event, String json) {
        CocosHelper.runOnGameThread(() -> JsbBridgeWrapper.getInstance().dispatchEventToScript(event, json));
    }

    private static JSONObject parse(String arg) {
        try {
            return new JSONObject(arg == null || arg.isEmpty() ? "{}" : arg);
        } catch (JSONException e) {
            return new JSONObject();
        }
    }
}
