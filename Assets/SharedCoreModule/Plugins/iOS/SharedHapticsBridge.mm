#import <UIKit/UIKit.h>

static void SharedHapticsPlayOnMainThread(NSInteger type)
{
    switch (type) {
        case 0: {
            UISelectionFeedbackGenerator *generator = [[UISelectionFeedbackGenerator alloc] init];
            [generator prepare];
            [generator selectionChanged];
            break;
        }
        case 1:
        case 2:
        case 3: {
            UIImpactFeedbackStyle style = UIImpactFeedbackStyleLight;
            if (type == 2) {
                style = UIImpactFeedbackStyleMedium;
            } else if (type == 3) {
                style = UIImpactFeedbackStyleHeavy;
            }

            UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
            [generator prepare];
            [generator impactOccurred];
            break;
        }
        case 4:
        case 5:
        case 6: {
            UINotificationFeedbackType feedbackType = UINotificationFeedbackTypeSuccess;
            if (type == 5) {
                feedbackType = UINotificationFeedbackTypeWarning;
            } else if (type == 6) {
                feedbackType = UINotificationFeedbackTypeError;
            }

            UINotificationFeedbackGenerator *generator = [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:feedbackType];
            break;
        }
        default:
            break;
    }
}

extern "C" void SharedHaptics_Play(int type)
{
    dispatch_async(dispatch_get_main_queue(), ^{
        SharedHapticsPlayOnMainThread(type);
    });
}
