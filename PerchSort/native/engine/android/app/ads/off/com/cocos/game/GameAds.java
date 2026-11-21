// The build without ads (game.json ads.enabled false, app/build.gradle GAME_ADS): no ad SDK dependency. Tells the script
// this build has no ads (off) so ad entries stay hidden; any request still gets an "unavailable" reply.
package com.cocos.game;

import android.app.Activity;

import org.json.JSONObject;

final class GameAds {
    private GameAds() {}

    static void start(Activity act, JSONObject ads) {
        GameBridge.send("game.ads", "{\"ready\":false,\"off\":true}");
    }

    static void showRewarded() {
        GameBridge.send("game.rewarded", "{\"result\":\"unavailable\"}");
    }

    static void showInterstitial() {
        GameBridge.send("game.interstitial", "{}");
    }

    static void openPrivacyOptions() {}
}
