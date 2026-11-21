#import "SceneDelegate.h"
#import "AppDelegate.h"

@implementation SceneDelegate

static AppDelegate *GameApp(void) {
    return (AppDelegate *)UIApplication.sharedApplication.delegate;
}

- (void)scene:(UIScene *)scene willConnectToSession:(UISceneSession *)session options:(UISceneConnectionOptions *)connectionOptions {
    if (![scene isKindOfClass:[UIWindowScene class]]) return;
    self.window = [GameApp() startInWindowScene:(UIWindowScene *)scene];
}

- (void)sceneDidBecomeActive:(UIScene *)scene {
    [GameApp() applicationDidBecomeActive:UIApplication.sharedApplication];
}

- (void)sceneWillResignActive:(UIScene *)scene {
    [GameApp() applicationWillResignActive:UIApplication.sharedApplication];
}

- (void)sceneWillEnterForeground:(UIScene *)scene {
    [GameApp() applicationWillEnterForeground:UIApplication.sharedApplication];
}

- (void)sceneDidEnterBackground:(UIScene *)scene {
    [GameApp() applicationDidEnterBackground:UIApplication.sharedApplication];
}

@end
