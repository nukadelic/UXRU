using System.Collections.Generic;
using UnityEngine.XR;

namespace Utils.XR
{
    public static class XRStaticButtons
    {
        static public List<InputFeatureUsage<bool>> ButtonsList = new List<InputFeatureUsage<bool>>
        {
            XRButtons.primaryButton,
            XRButtons.primaryTouch,
            XRButtons.secondaryButton,
            XRButtons.secondaryTouch,
            XRButtons.gripButton,
            XRButtons.gripTouch,
            XRButtons.triggerButton,
            XRButtons.triggerTouch,
            XRButtons.menuButton,
            XRButtons.primary2DAxisClick,
            XRButtons.primary2DAxisTouch,
        };

        static public List<InputFeatureUsage<bool>> CustomButtonsList = new List<InputFeatureUsage<bool>>
        {
            XRButtons.gripButton,
            XRButtons.gripTouch,
            XRButtons.triggerButton,
        };

        static public InputFeatureUsage<bool> ButtonTypeToFeature( XRButtonType button )
        {
            return XRStaticButtons.ButtonsList[ ( int ) button ];
        }
    }
}