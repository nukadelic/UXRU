

using UnityEngine.XR;

namespace Utils.XR
{
    public static class XRButtons
    {
        // public static InputFeatureUsage<bool> isTracked = CommonUsages.isTracked;
        public static InputFeatureUsage<bool> primaryButton         = CommonUsages.primaryButton;
        public static InputFeatureUsage<bool> primaryTouch          = CommonUsages.primaryTouch;
        public static InputFeatureUsage<bool> secondaryButton       = CommonUsages.secondaryButton;
        public static InputFeatureUsage<bool> secondaryTouch        = CommonUsages.secondaryTouch;
        public static InputFeatureUsage<bool> gripButton            = new InputFeatureUsage<bool>("_Grip_Button");
        public static InputFeatureUsage<bool> gripTouch             = new InputFeatureUsage<bool>("_Grip_Touch");
        public static InputFeatureUsage<bool> triggerButton         = new InputFeatureUsage<bool>("_Trigger_Button");
        // Oculus Rift S - this is actually the trigger touch and it works very well 
        public static InputFeatureUsage<bool> triggerTouch          = CommonUsages.triggerButton;
        // public static InputFeatureUsage<bool> triggerButton         = new InputFeatureUsage<bool>("_Trigger_Touch");
        public static InputFeatureUsage<bool> menuButton            = CommonUsages.menuButton;
        public static InputFeatureUsage<bool> primary2DAxisClick    = CommonUsages.primary2DAxisClick;
        public static InputFeatureUsage<bool> primary2DAxisTouch    = CommonUsages.primary2DAxisTouch;
        // public static InputFeatureUsage<bool> secondary2DAxisClick = CommonUsages.secondary2DAxisClick;
        // public static InputFeatureUsage<bool> secondary2DAxisTouch = CommonUsages.secondary2DAxisTouch;
    }
}