// Ads: Google Mobile Ads (AdMob) with user consent (UMP, asked first where required). Compiled in when game.json
// ads.enabled is true (app/build.gradle GAME_ADS); otherwise ads/off supplies an empty implementation without the SDK.
// Called through GameBridge; the event table is in NativePlatform.ts.
package com.cocos.game;

import android.app.Activity;
import android.os.Handler;
import android.os.Looper;

import androidx.annotation.NonNull;

import com.google.android.gms.ads.AdError;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.MobileAds;
import com.google.android.gms.ads.interstitial.InterstitialAd;
import com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback;
import com.google.android.gms.ads.rewarded.RewardedAd;
import com.google.android.gms.ads.rewarded.RewardedAdLoadCallback;
import com.google.android.ump.ConsentInformation;
import com.google.android.ump.ConsentRequestParameters;
import com.google.android.ump.UserMessagingPlatform;

import org.json.JSONObject;

final class GameAds {
    private static Activity activity;
    private static String rewardedUnit = "";
    private static String interstitialUnit = "";
    private static RewardedAd rewarded;
    private static InterstitialAd interstitial;
    private static boolean earned;
    /** Seconds before retrying a failed load: from 15, doubling, at most 5 minutes; reset on success.
     * Fill is low before and just after release and loads fail offline; without retries the entries never return. */
    private static final int RETRY_FIRST = 15;
    private static final int RETRY_MAX = 300;
    private static int rewardedRetry = RETRY_FIRST;
    private static int interstitialRetry = RETRY_FIRST;
    private static final Handler main = new Handler(Looper.getMainLooper());

    private GameAds() {}

    private static void send(String event, String json) {
        GameBridge.send(event, json);
    }

    /** game.init: keep the units, ask consent (UMP), then start the SDK and preload once ads may be requested. */
    static void start(Activity act, JSONObject ads) {
        activity = act;
        if (ads != null) {
            rewardedUnit = ads.optString("rewarded", "");
            interstitialUnit = ads.optString("interstitial", "");
        }
        // No rewarded unit means no ads in this build: no consent prompt, ad entries hidden.
        if (rewardedUnit.isEmpty()) {
            send("game.ads", "{\"ready\":false,\"off\":true}");
            return;
        }
        askConsentThenStart();
    }

    static void openPrivacyOptions() {
        // Consent may now allow ads: start the SDK and load.
        UserMessagingPlatform.showPrivacyOptionsForm(activity, error -> startSdk(UserMessagingPlatform.getConsentInformation(activity)));
    }

    private static void askConsentThenStart() {
        ConsentInformation info = UserMessagingPlatform.getConsentInformation(activity);
        ConsentRequestParameters params = new ConsentRequestParameters.Builder().build();
        info.requestConsentInfoUpdate(activity, params,
            () -> UserMessagingPlatform.loadAndShowConsentFormIfRequired(activity, formError -> startSdk(info)),
            requestError -> startSdk(info));
    }

    private static void startSdk(ConsentInformation info) {
        if (!info.canRequestAds()) {
            // No consent: no ads can be requested; the privacy options can still change it.
            send("game.ads", "{\"ready\":false}");
            return;
        }
        MobileAds.initialize(activity, status -> {
            loadRewarded();
            loadInterstitial();
        });
    }

    private static boolean rewardedLoading;
    private static boolean interstitialLoading;

    private static void loadRewarded() {
        // One ad at a time: a tap and a scheduled retry can arrive together.
        if (rewardedUnit.isEmpty() || rewarded != null || rewardedLoading) return;
        rewardedLoading = true;
        RewardedAd.load(activity, rewardedUnit, new AdRequest.Builder().build(), new RewardedAdLoadCallback() {
            @Override
            public void onAdLoaded(@NonNull RewardedAd ad) {
                rewardedLoading = false;
                rewarded = ad;
                rewardedRetry = RETRY_FIRST;
                rewarded.setFullScreenContentCallback(new FullScreenContentCallback() {
                    @Override
                    public void onAdDismissedFullScreenContent() {
                        send("game.rewarded", earned ? "{\"result\":\"done\"}" : "{\"result\":\"skipped\"}");
                        rewarded = null;
                        send("game.ads", "{\"ready\":false}");
                        loadRewarded();
                    }

                    @Override
                    public void onAdFailedToShowFullScreenContent(@NonNull AdError error) {
                        send("game.rewarded", "{\"result\":\"unavailable\"}");
                        rewarded = null;
                        send("game.ads", "{\"ready\":false}");
                        loadRewarded();
                    }
                });
                send("game.ads", "{\"ready\":true}");
            }

            @Override
            public void onAdFailedToLoad(@NonNull LoadAdError error) {
                rewardedLoading = false;
                rewarded = null;
                send("game.ads", "{\"ready\":false}");
                int wait = rewardedRetry;
                rewardedRetry = Math.min(RETRY_MAX, rewardedRetry * 2);
                main.postDelayed(GameAds::loadRewarded, wait * 1000L);
            }
        });
    }

    private static void loadInterstitial() {
        if (interstitialUnit.isEmpty() || interstitial != null || interstitialLoading) return;
        interstitialLoading = true;
        InterstitialAd.load(activity, interstitialUnit, new AdRequest.Builder().build(), new InterstitialAdLoadCallback() {
            @Override
            public void onAdLoaded(@NonNull InterstitialAd ad) {
                interstitialLoading = false;
                interstitial = ad;
                interstitialRetry = RETRY_FIRST;
                interstitial.setFullScreenContentCallback(new FullScreenContentCallback() {
                    @Override
                    public void onAdDismissedFullScreenContent() {
                        send("game.interstitial", "{\"shown\":true}");
                        interstitial = null;
                        loadInterstitial();
                    }

                    @Override
                    public void onAdFailedToShowFullScreenContent(@NonNull AdError error) {
                        send("game.interstitial", "{\"shown\":false}");
                        interstitial = null;
                        loadInterstitial();
                    }
                });
            }

            @Override
            public void onAdFailedToLoad(@NonNull LoadAdError error) {
                interstitialLoading = false;
                interstitial = null;
                int wait = interstitialRetry;
                interstitialRetry = Math.min(RETRY_MAX, interstitialRetry * 2);
                main.postDelayed(GameAds::loadInterstitial, wait * 1000L);
            }
        });
    }

    static void showRewarded() {
        if (rewarded == null) {
            send("game.rewarded", "{\"result\":\"unavailable\"}");
            loadRewarded();
            return;
        }
        earned = false;
        rewarded.show(activity, reward -> earned = true);
    }

    static void showInterstitial() {
        if (interstitial == null) {
            send("game.interstitial", "{\"shown\":false}");
            loadInterstitial();
            return;
        }
        interstitial.show(activity);
    }
}
