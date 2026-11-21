// Bridge between scripts and iOS: handles the game.* events sent by NativePlatform.ts (ads, vibration, privacy options).
#import <Foundation/Foundation.h>

@interface GameBridge : NSObject
/** Called once by AppDelegate at startup to register the events. */
+ (void)setup;
@end
