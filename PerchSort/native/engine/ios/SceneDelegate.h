// Scene lifecycle (UIScene): apps built with the iOS 27 SDK must adopt it or abort at launch
// (_UIApplicationEvaluateRuntimeIssueForNoSceneLifecycleAdoption, seen on iPhone 2026-11-20).
// AppDelegate creates the window and engine when the scene connects; foreground/background changes reach only the
// scene, which forwards them to AppDelegate's existing methods.
#import <UIKit/UIKit.h>

@interface SceneDelegate : UIResponder <UIWindowSceneDelegate>
@property(strong, nonatomic) UIWindow *window;
@end
