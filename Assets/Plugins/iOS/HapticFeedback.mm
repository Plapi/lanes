#import <UIKit/UIKit.h>

extern "C" {

void VibrateHaptic(int type) {
    if (@available(iOS 13.0, *)) {
        UIImpactFeedbackStyle style;
        style = type == 0 ? UIImpactFeedbackStyleHeavy :
            type == 1 ? UIImpactFeedbackStyleLight :
            type == 2 ? UIImpactFeedbackStyleMedium :
            type == 3 ? UIImpactFeedbackStyleRigid :
            UIImpactFeedbackStyleSoft;
        UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
        [generator prepare];
        [generator impactOccurred];
    }
}

}
