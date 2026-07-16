#import <GameKit/GameKit.h>
#import "UnityAppController.h"
#import "UnityInterface.h"

static void SharedGameCenterSend(NSString *gameObjectName, const char *method, NSString *message)
{
    UnitySendMessage(
        gameObjectName.UTF8String,
        method,
        (message ?: @"").UTF8String);
}

extern "C" void SharedGameCenter_Authenticate(const char *gameObjectName, int allowLoginUi)
{
    NSString *receiver = [NSString stringWithUTF8String:gameObjectName ?: ""];
    GKLocalPlayer *player = GKLocalPlayer.localPlayer;

    player.authenticateHandler = ^(UIViewController *viewController, NSError *error) {
        dispatch_async(dispatch_get_main_queue(), ^{
            if (viewController != nil) {
                if (allowLoginUi) {
                    [UnityGetGLViewController() presentViewController:viewController animated:YES completion:nil];
                } else {
                    player.authenticateHandler = nil;
                    SharedGameCenterSend(receiver, "OnGameCenterAuthenticationFailed", @"Game Center login requires user action");
                }
                return;
            }

            if (player.isAuthenticated) {
                NSDictionary *payload = @{
                    @"playerId": player.gamePlayerID ?: @"",
                    @"displayName": player.displayName ?: @""
                };
                NSData *data = [NSJSONSerialization dataWithJSONObject:payload options:0 error:nil];
                NSString *json = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
                SharedGameCenterSend(receiver, "OnGameCenterAuthenticationSucceeded", json);
                return;
            }

            NSString *message = error.localizedDescription ?: @"Game Center login was cancelled";
            SharedGameCenterSend(receiver, "OnGameCenterAuthenticationFailed", message);
        });
    };
}
