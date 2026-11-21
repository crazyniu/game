// Bridge between scripts and iOS; the event table is at the top of assets/scripts/platform/NativePlatform.ts.
//
// Ads: Google Mobile Ads (AdMob) with user consent (UMP, asked first where required) and tracking authorization (ATT).
// ./package.sh downloads the SDK into native/engine/ios/sdk/ (not in Git). When game.json turns ads off (CMakeLists
// GAME_ADS) or the SDK is missing, the app builds without it (__has_include) and tells the script this build has no ads:
// ad entries stay hidden and neither consent nor tracking is asked.
#import "GameBridge.h"

#import <UIKit/UIKit.h>
#include "platform/apple/JsbBridgeWrapper.h"

#if __has_include(<GoogleMobileAds/GoogleMobileAds.h>)
#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <GoogleMobileAds/GoogleMobileAds.h>
#import <UserMessagingPlatform/UserMessagingPlatform.h>
#define GAME_ADMOB 1
#else
#define GAME_ADMOB 0
#endif

static NSDictionary *GameParse(NSString *arg) {
    NSData *data = [(arg ?: @"{}") dataUsingEncoding:NSUTF8StringEncoding];
    id obj = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
    return [obj isKindOfClass:[NSDictionary class]] ? obj : @{};
}

static void GameSend(NSString *event, NSDictionary *payload) {
    NSData *data = [NSJSONSerialization dataWithJSONObject:(payload ?: @{}) options:0 error:nil];
    NSString *json = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    [[JsbBridgeWrapper sharedInstance] dispatchEventToScript:event arg:json];
}

static UIViewController *GameRootViewController(void) {
    UIWindow *window = nil;
    for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
        if ([scene isKindOfClass:[UIWindowScene class]]) {
            for (UIWindow *w in ((UIWindowScene *)scene).windows) {
                if (w.isKeyWindow) { window = w; break; }
            }
        }
    }
    if (!window) window = UIApplication.sharedApplication.delegate.window;
    UIViewController *vc = window.rootViewController;
    while (vc.presentedViewController) vc = vc.presentedViewController;
    return vc;
}

#if GAME_ADMOB
@interface GameAds : NSObject <GADFullScreenContentDelegate>
@property(nonatomic, copy) NSString *rewardedUnit;
@property(nonatomic, copy) NSString *interstitialUnit;
@property(nonatomic, strong) GADRewardedAd *rewarded;
@property(nonatomic, strong) GADInterstitialAd *interstitial;
@property(nonatomic, assign) BOOL earned;
// Seconds before retrying a failed load: from 15, doubling, at most 5 minutes; reset on success.
// Fill is low before and just after release and loads fail offline; without retries the entries never return.
@property(nonatomic, assign) NSTimeInterval rewardedRetry;
@property(nonatomic, assign) NSTimeInterval interstitialRetry;
@property(nonatomic, assign) BOOL rewardedLoading;
@property(nonatomic, assign) BOOL interstitialLoading;
@end

static const NSTimeInterval GAME_RETRY_FIRST = 15;
static const NSTimeInterval GAME_RETRY_MAX = 300;

@implementation GameAds

+ (instancetype)shared {
    static GameAds *ads;
    static dispatch_once_t once;
    dispatch_once(&once, ^{ ads = [[GameAds alloc] init]; });
    return ads;
}

/** Consent (UMP), then tracking authorization (ATT), then start the SDK and preload. */
- (void)startWithUnits:(NSDictionary *)units {
    self.rewardedUnit = units[@"rewarded"] ?: @"";
    self.interstitialUnit = units[@"interstitial"] ?: @"";
    // No rewarded unit means no ads in this build: no consent or tracking prompts, ad entries hidden.
    if (self.rewardedUnit.length == 0) {
        GameSend(@"game.ads", @{@"ready" : @NO, @"off" : @YES});
        return;
    }
    UMPRequestParameters *params = [[UMPRequestParameters alloc] init];
    [UMPConsentInformation.sharedInstance
        requestConsentInfoUpdateWithParameters:params
                             completionHandler:^(NSError *_Nullable error) {
                                 [UMPConsentForm loadAndPresentIfRequiredFromViewController:GameRootViewController()
                                                                          completionHandler:^(NSError *_Nullable formError) {
                                                                              [self askTrackingThenStart];
                                                                          }];
                             }];
}

- (void)askTrackingThenStart {
    if (@available(iOS 14, *)) {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            dispatch_async(dispatch_get_main_queue(), ^{ [self startSdk]; });
        }];
    } else {
        [self startSdk];
    }
}

- (void)startSdk {
    if (!UMPConsentInformation.sharedInstance.canRequestAds) {
        GameSend(@"game.ads", @{@"ready" : @NO});
        return;
    }
    [GADMobileAds.sharedInstance startWithCompletionHandler:^(GADInitializationStatus *status) {
        [self loadRewarded];
        [self loadInterstitial];
    }];
}

- (void)loadRewarded {
    // One ad at a time: a tap and a scheduled retry can arrive together.
    if (self.rewardedUnit.length == 0 || self.rewarded || self.rewardedLoading) return;
    self.rewardedLoading = YES;
    [GADRewardedAd loadWithAdUnitID:self.rewardedUnit
                            request:[GADRequest request]
                  completionHandler:^(GADRewardedAd *ad, NSError *error) {
                      self.rewardedLoading = NO;
                      self.rewarded = error ? nil : ad;
                      self.rewarded.fullScreenContentDelegate = self;
                      GameSend(@"game.ads", @{@"ready" : @(self.rewarded != nil)});
                      if (self.rewarded) {
                          self.rewardedRetry = GAME_RETRY_FIRST;
                      } else {
                          NSTimeInterval wait = MAX(self.rewardedRetry, GAME_RETRY_FIRST);
                          self.rewardedRetry = MIN(GAME_RETRY_MAX, wait * 2);
                          dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(wait * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{ [self loadRewarded]; });
                      }
                  }];
}

- (void)loadInterstitial {
    if (self.interstitialUnit.length == 0 || self.interstitial || self.interstitialLoading) return;
    self.interstitialLoading = YES;
    [GADInterstitialAd loadWithAdUnitID:self.interstitialUnit
                                request:[GADRequest request]
                      completionHandler:^(GADInterstitialAd *ad, NSError *error) {
                          self.interstitialLoading = NO;
                          self.interstitial = error ? nil : ad;
                          self.interstitial.fullScreenContentDelegate = self;
                          if (self.interstitial) {
                              self.interstitialRetry = GAME_RETRY_FIRST;
                          } else {
                              NSTimeInterval wait = MAX(self.interstitialRetry, GAME_RETRY_FIRST);
                              self.interstitialRetry = MIN(GAME_RETRY_MAX, wait * 2);
                              dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(wait * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{ [self loadInterstitial]; });
                          }
                      }];
}

- (void)showRewarded {
    if (!self.rewarded) {
        GameSend(@"game.rewarded", @{@"result" : @"unavailable"});
        [self loadRewarded];
        return;
    }
    self.earned = NO;
    [self.rewarded presentFromRootViewController:GameRootViewController()
                        userDidEarnRewardHandler:^{ self.earned = YES; }];
}

- (void)showInterstitial {
    if (!self.interstitial) {
        GameSend(@"game.interstitial", @{@"shown" : @NO});
        [self loadInterstitial];
        return;
    }
    [self.interstitial presentFromRootViewController:GameRootViewController()];
}

- (void)openPrivacyOptions {
    [UMPConsentForm presentPrivacyOptionsFormFromViewController:GameRootViewController()
                                              completionHandler:^(NSError *_Nullable error) {
                                                  // Consent may now allow ads: start the SDK and load.
                                                  [self startSdk];
                                              }];
}

#pragma mark GADFullScreenContentDelegate

- (void)adDidDismissFullScreenContent:(id<GADFullScreenPresentingAd>)ad {
    if (ad == self.rewarded) {
        GameSend(@"game.rewarded", @{@"result" : self.earned ? @"done" : @"skipped"});
        self.rewarded = nil;
        GameSend(@"game.ads", @{@"ready" : @NO});
        [self loadRewarded];
    } else if (ad == self.interstitial) {
        GameSend(@"game.interstitial", @{@"shown" : @YES});
        self.interstitial = nil;
        [self loadInterstitial];
    }
}

- (void)ad:(id<GADFullScreenPresentingAd>)ad didFailToPresentFullScreenContentWithError:(NSError *)error {
    if (ad == self.rewarded) {
        GameSend(@"game.rewarded", @{@"result" : @"unavailable"});
        self.rewarded = nil;
        GameSend(@"game.ads", @{@"ready" : @NO});
        [self loadRewarded];
    } else if (ad == self.interstitial) {
        GameSend(@"game.interstitial", @{@"shown" : @NO});
        self.interstitial = nil;
        [self loadInterstitial];
    }
}

@end
#endif

@implementation GameBridge

+ (void)setup {
    JsbBridgeWrapper *bridge = [JsbBridgeWrapper sharedInstance];

    [bridge addScriptEventListener:@"game.init"
                          listener:^(NSString *arg) {
#if GAME_ADMOB
                              NSDictionary *units = GameParse(arg)[@"ads"];
                              [[GameAds shared] startWithUnits:[units isKindOfClass:[NSDictionary class]] ? units : @{}];
#else
                              GameSend(@"game.ads", @{@"ready" : @NO, @"off" : @YES});
#endif
                          }];

    [bridge addScriptEventListener:@"game.rewarded"
                          listener:^(NSString *arg) {
#if GAME_ADMOB
                              [[GameAds shared] showRewarded];
#else
                              GameSend(@"game.rewarded", @{@"result" : @"unavailable"});
#endif
                          }];

    [bridge addScriptEventListener:@"game.interstitial"
                          listener:^(NSString *arg) {
#if GAME_ADMOB
                              [[GameAds shared] showInterstitial];
#else
                              GameSend(@"game.interstitial", @{});
#endif
                          }];

    [bridge addScriptEventListener:@"game.privacy"
                          listener:^(NSString *arg) {
#if GAME_ADMOB
                              [[GameAds shared] openPrivacyOptions];
#endif
                          }];

    [bridge addScriptEventListener:@"game.vibrate"
                          listener:^(NSString *arg) {
                              NSString *kind = GameParse(arg)[@"kind"];
                              UIImpactFeedbackStyle style = UIImpactFeedbackStyleMedium;
                              if ([kind isEqualToString:@"light"]) style = UIImpactFeedbackStyleLight;
                              if ([kind isEqualToString:@"heavy"]) style = UIImpactFeedbackStyleHeavy;
                              UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
                              [gen impactOccurred];
                          }];
}

@end
