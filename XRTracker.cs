 using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Utils.XR
{
    // Minimum requirments: Unity version 2019.2
    // Based on: https://forum.unity.com/threads/any-example-of-the-new-2019-1-xr-input-system.629824/#post-4513171
    // Note: install [ XR Legacy Input Helpers ] package 

    public class XRTracker : MonoBehaviour
    {
                            //Keep this around to avoid creating heap garbage
        static              List<InputDevice> devices = new List<InputDevice>();
        
        [Tooltip("UnityEngine.XR.InputDeviceRole")]
        [SerializeField]    public InputDeviceRole role = InputDeviceRole.Generic;

        [HideInInspector] public bool trackPosition = true;
        [HideInInspector] public bool trackRotation = true;

        [HideInInspector]   public InputDevice trackedDevice;

        public UpdateFlags trackTransformation = UpdateFlags.DuringUpdate;
        
        [Tooltip("By default local space is used")]
        [SerializeField]    bool useWorldSpace = false;

        public bool isConnected()
        {
            return trackedDevice != null && trackedDevice.isValid;
        }

        /// Check if tracker device is hand ( left or right )
        public bool IsHand { get { return role == InputDeviceRole.LeftHanded || role == InputDeviceRole.RightHanded; } }

        [HideInInspector]   public List<InputFeatureUsage> features = new List<InputFeatureUsage>();
        [HideInInspector]   public List<string> featureNames;

        [HideInInspector]   public List<InputFeatureUsage<bool>> trackedButtons    = new List<InputFeatureUsage<bool>>();
        [HideInInspector]   public List<bool> trackedButtons_pressed               = new List<bool>();
        [HideInInspector]   public List<bool> trackedButtons_released              = new List<bool>();
        [HideInInspector]   public List<bool> trackedButtons_state                 = new List<bool>();

        public bool HasFeature( string featureName, bool throwException = false )
        {
            // Feature validity is unknown when device is not connected 
            if( ! trackedDevice.isValid ) return false;

            // From when the device was connected and FeatureUsages was captured
            bool has_feature = featureNames.Contains( featureName );

            if( ! has_feature && throwException ) 
                
                throw new Exception("Unsupported feature " + featureName + " for device " + trackedDevice.name );

            return has_feature;
        }

        public bool HasAxisPrimary() { return HasFeature( CommonUsages.primary2DAxis.name ); }
        /// Returns primary Axis (2D) vector
        public Vector2 GetAxisPrimary()
        {
            if( ! HasAxisPrimary() ) return Vector2.zero;

            Vector2 axis;

            if( ! trackedDevice.TryGetFeatureValue( CommonUsages.primary2DAxis, out axis ) )
            {
                XRUtil.LogError("Failed to get feature value");
                return Vector2.zero;
            }

            return axis;
        }

        // public bool HasAxisSecondary() { return HasFeature( CommonUsages.secondary2DAxis.name ); }
        // /// Returns secondary Axis (2D) vector
        // public Vector2 GetAxisSecondary()
        // {
        //     if( ! HasAxisSecondary() ) return Vector2.zero;

        //     Vector2 axis;

        //     if( ! trackedDevice.TryGetFeatureValue( CommonUsages.secondary2DAxis, out axis ) )
        //     {
        //         XRUtil.LogError("Failed to get feature value");
        //         return Vector2.zero;
        //     }

        //     return axis;
        // }

        public bool HasAxisTrigger() { return HasFeature( CommonUsages.trigger.name ); }
        /// Returns trigger Axis (1D) value from 0 to 1 ( index finger )
        public float GetAxisTrigger()
        {   
            // If trigger Axis is not supported, return it's button alternative when avalible 
            if( ! HasAxisTrigger() && HasFeature( CommonUsages.triggerButton.name ) )
                return GetButton( CommonUsages.triggerButton ) ? 1 : 0;

            float value;

            if( ! trackedDevice.TryGetFeatureValue( CommonUsages.trigger, out value ) )
            {
                XRUtil.LogError("Failed to get feature value");
                return 0;
            }

            return value;
        }

        public bool HasAxisGrip() { return HasFeature( CommonUsages.grip.name ); }
        /// Returns grip Axis (1D) value from 0 to 1
        public float GetAxisGrip()
        {
            // If grip Axis is not supported, return it's button alternative when avalible 
            if( ! HasAxisGrip() && HasFeature( CommonUsages.gripButton.name ) ) 
                return GetButton( CommonUsages.gripButton ) ? 1 : 0;

            float value;

            if( ! trackedDevice.TryGetFeatureValue( CommonUsages.grip, out value ) )
            {
                XRUtil.LogError("Failed to get feature value");
                return 0;
            }

            return value;
        }

        /// Returns true while the button is held down ( See XRStaticButtons )
        public bool GetButton( InputFeatureUsage<bool> button_feature )
        {
            int index = trackedButtons.IndexOf( button_feature );
            if( index == -1 ) TrackButton( button_feature );
            else return trackedButtons_state[ index ];
            return false;
        }

        /// Returns true while the busson is held down
        public bool GetButton( XRButtonType button_type )
        {
            return GetButton( XRStaticButtons.ButtonTypeToFeature( button_type ) );

            // return GetButton( XRTracker.ButtonGet( button ) );
        }

        /// Returns true during the frame the user pressed down the button ( See XRStaticButtons )
        public bool GetButtonDown( InputFeatureUsage<bool> button )
        {
            int index = trackedButtons.IndexOf( button );
            if( index == -1 ) TrackButton( button );
            else return trackedButtons_pressed[ index ];
            return false;
        }

        /// Returns true the first frame the user releases the button ( See XRStaticButtons )
        public bool GetButtonUp( InputFeatureUsage<bool> button )
        {
            int index = trackedButtons.IndexOf( button );
            if( index == -1 ) TrackButton( button );
            else return trackedButtons_released[ index ];
            return false;
        }

        void TrackButton( InputFeatureUsage<bool> button )
        {
            if( trackedButtons.Contains( button ) ) return;

            trackedButtons              .Add( button );
            trackedButtons_pressed      .Add( false );
            trackedButtons_released     .Add( false );
            trackedButtons_state        .Add( false );
        }

        static readonly float MinValue_Touch = 0.001f;
        static readonly float MinValue_Button = 0.5f;

        bool GetCustomButtonValue( InputFeatureUsage<bool> usage )
        {
            float value = 0f;

            if( usage.name.Contains("_Grip") )      value = GetAxisGrip();
            if( usage.name.Contains("_Trigger") )   value = GetAxisTrigger();
            
            if( usage.name.Contains("_Touch") )       return value >=  MinValue_Touch;
            if( usage.name.Contains("_Button") )      return value >= MinValue_Button;

            return false;
        }

        void UpdateButtons( bool reset )
        {
            for( int i = 0; i < trackedButtons.Count; ++i )
            {
                if( reset )
                {
                    trackedButtons_pressed[ i ] = false;
                    trackedButtons_released[ i ] = false;
                }

                InputFeatureUsage<bool> input_device = trackedButtons[ i ];

                bool is_pressed = false;

                if( XRStaticButtons.CustomButtonsList.Contains( input_device ) )
                {
                    is_pressed = GetCustomButtonValue( input_device );
                }
                else if( ! trackedDevice.TryGetFeatureValue( input_device, out is_pressed ) ) 
                {
                    continue;
                }
                
                if( trackedButtons_state[ i ] != is_pressed )
                {
                    if( is_pressed )    trackedButtons_pressed[ i ] = true;
                    else                trackedButtons_released[ i ] = true;
                }

                trackedButtons_state[ i ] = is_pressed;
            }
        }

        void Awake()
        {
            trackedDevice = new InputDevice();
            features = new List<InputFeatureUsage>();
        }

        void OnEnable()
        {
            InputDevices.deviceConnected        += OnDeviceConnected;
            InputDevices.deviceDisconnected     += OnDeviceDisconnected;
            // // Application.onBeforeRender          += OnBeforeRender;

            InputDevices.GetDevicesWithRole( role, devices );

            if ( devices.Count > 0) OnDeviceConnected( devices[0] );
        }

        void Start()
        {

        }

        void OnDisable()
        {
            InputDevices.deviceConnected        -= OnDeviceConnected;
            InputDevices.deviceDisconnected     -= OnDeviceDisconnected;
            // Application.onBeforeRender          -= OnBeforeRender;

            trackedButtons.Clear();
            trackedButtons_pressed.Clear();
            trackedButtons_released.Clear();
            trackedButtons_state.Clear();
            features.Clear();
        }

        bool oscillator = false;

        void Update()
        {
            if ( trackedDevice.isValid ) 
            {
                if( trackTransformation.HasFlag( UpdateFlags.DuringUpdate ) )

                    TrackDeviceTransform( trackedDevice );

                UpdateButtons( oscillator = ! oscillator );
            }
        }

        void OnBeforeRender()
        {
            if ( trackedDevice.isValid ) 
            {
                if( trackTransformation.HasFlag( UpdateFlags.BeforeRender ) )

                    TrackDeviceTransform( trackedDevice );
            }
        }

        void OnDeviceConnected( InputDevice device )
        {
            if ( ! trackedDevice.isValid && device.role == role ) 
            {
                trackedDevice = device;
                
                if( trackedDevice.TryGetFeatureUsages( features ) )
                    
                    featureNames = features.Select( feature => feature.name ).ToList();

                // TODO: add error reporting everywhere that uses a [TryGet"Something"] pattern
                else XRUtil.LogError("Failed to get feature usages");
            }
        }

        void OnDeviceDisconnected( InputDevice device )
        {
            if (device == trackedDevice)
            {
                // Set current as empty
                trackedDevice = new InputDevice();
            }
        }

        void TrackDeviceTransform( InputDevice trackedDevice )
        {
            if( trackPosition )
            {
                Vector3 position;
                
                if ( trackedDevice.TryGetFeatureValue( CommonUsages.devicePosition, out position ) )
                {   
                    if( useWorldSpace ) this.transform.position = position;
                    else this.transform.localPosition = position;
                }
            }

            if( trackRotation )
            {
                Quaternion rotation;

                if( trackedDevice.TryGetFeatureValue( CommonUsages.deviceRotation, out rotation ) )
                {
                    if( useWorldSpace ) this.transform.rotation = rotation;
                    else this.transform.localRotation = rotation;
                }
            }
        }

        [Flags]
        public enum UpdateFlags
        {
            Never = 0,
            DuringUpdate = 1 << 0,
            BeforeRender = 1 << 1,
            Always = -1
        }

        // public static class Axis
        // {
        //     public static InputFeatureUsage<float> batteryLevel = CommonUsages.batteryLevel;
        //     public static InputFeatureUsage<float> grip = CommonUsages.grip;
        //     public static InputFeatureUsage<float> trigger = CommonUsages.trigger;
        //     public static InputFeatureUsage<Vector2> primary2DAxis = CommonUsages.primary2DAxis;
        // }

        // public static class XRButtons
        // {
        //     // public static InputFeatureUsage<bool> isTracked = CommonUsages.isTracked;
        //     public static InputFeatureUsage<bool> primaryButton         = CommonUsages.primaryButton;
        //     public static InputFeatureUsage<bool> primaryTouch          = CommonUsages.primaryTouch;
        //     public static InputFeatureUsage<bool> secondaryButton       = CommonUsages.secondaryButton;
        //     public static InputFeatureUsage<bool> secondaryTouch        = CommonUsages.secondaryTouch;
        //     public static InputFeatureUsage<bool> gripButton            = new InputFeatureUsage<bool>("_Grip_Button");
        //     public static InputFeatureUsage<bool> gripTouch             = new InputFeatureUsage<bool>("_Grip_Touch");
        //     public static InputFeatureUsage<bool> triggerButton         = new InputFeatureUsage<bool>("_Trigger_Button");
        //     // Oculus Rift S - this is actually the trigger touch and it works very well 
        //     public static InputFeatureUsage<bool> triggerTouch          = CommonUsages.triggerButton;
        //     // public static InputFeatureUsage<bool> triggerButton         = new InputFeatureUsage<bool>("_Trigger_Touch");
        //     public static InputFeatureUsage<bool> menuButton            = CommonUsages.menuButton;
        //     public static InputFeatureUsage<bool> primary2DAxisClick    = CommonUsages.primary2DAxisClick;
        //     public static InputFeatureUsage<bool> primary2DAxisTouch    = CommonUsages.primary2DAxisTouch;
        //     // public static InputFeatureUsage<bool> secondary2DAxisClick = CommonUsages.secondary2DAxisClick;
        //     // public static InputFeatureUsage<bool> secondary2DAxisTouch = CommonUsages.secondary2DAxisTouch;
        // }

        // static List<InputFeatureUsage<bool>> CustomButtons = new List<InputFeatureUsage<bool>>
        // {
        //     XRButtons.gripButton,
        //     XRButtons.gripTouch,
        //     XRButtons.triggerButton,
        //     // Buttons.triggerTouch,
        // };

        // static List<InputFeatureUsage<bool>> ButtonsList = new List<InputFeatureUsage<bool>>
        // {
        //     XRButtons.primaryButton,
        //     XRButtons.primaryTouch,
        //     XRButtons.secondaryButton,
        //     XRButtons.secondaryTouch,
        //     XRButtons.gripButton,
        //     XRButtons.gripTouch,
        //     XRButtons.triggerButton,
        //     XRButtons.triggerTouch,
        //     XRButtons.menuButton,
        //     XRButtons.primary2DAxisClick,
        //     XRButtons.primary2DAxisTouch,
        // };

        // static public InputFeatureUsage<bool> ButtonGet( XRButtonType button )
        // {
        //     return XRStaticButtons.ButtonsList[ ( int ) button ];
        // }

        // public class CommonUsage
        // {
        //     public Type type;
        //     public string name;
        //     public string link;
        //     public CommonUsage() { }
        // }

        // public static List<CommonUsage> usages = new List<CommonUsage>
        // {
        //     new CommonUsage { type = typeof( bool )                 , name = "isTracked"                        , link = "IsTracked" },
        //     new CommonUsage { type = typeof( bool )                 , name = "primaryButton"                    , link = "PrimaryButton" },
        //     new CommonUsage { type = typeof( bool )                 , name = "primaryTouch"                     , link = "PrimaryTouch" },
        //     new CommonUsage { type = typeof( bool )                 , name = "secondaryButton"                  , link = "SecondaryButton" },
        //     new CommonUsage { type = typeof( bool )                 , name = "secondaryTouch"                   , link = "SecondaryTouch" },
        //     new CommonUsage { type = typeof( bool )                 , name = "gripButton"                       , link = "GripButton" },
        //     new CommonUsage { type = typeof( bool )                 , name = "triggerButton"                    , link = "TriggerButton" },
        //     new CommonUsage { type = typeof( bool )                 , name = "menuButton"                       , link = "MenuButton" },
        //     new CommonUsage { type = typeof( bool )                 , name = "primary2DAxisClick"               , link = "Primary2DAxisClick" },
        //     new CommonUsage { type = typeof( bool )                 , name = "primary2DAxisTouch"               , link = "Primary2DAxisTouch" },
        //     new CommonUsage { type = typeof( bool )                 , name = "secondary2DAxisClick"             , link = "Secondary2DAxisClick" },
        //     new CommonUsage { type = typeof( bool )                 , name = "secondary2DAxisTouch"             , link = "Secondary2DAxisTouch" },
        //     new CommonUsage { type = typeof( InputTrackingState )   , name = "trackingState"                    , link = "TrackingState" },
        //     new CommonUsage { type = typeof( float )                , name = "batteryLevel"                     , link = "BatteryLevel" },
        //     new CommonUsage { type = typeof( float )                , name = "trigger"                          , link = "Trigger" },
        //     new CommonUsage { type = typeof( float )                , name = "grip"                             , link = "Grip" },
        //     new CommonUsage { type = typeof( Vector2 )              , name = "primary2DAxis"                    , link = "Primary2DAxis" },
        //     new CommonUsage { type = typeof( Vector2 )              , name = "secondary2DAxis"                  , link = "Secondary2DAxis" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "devicePosition"                   , link = "DevicePosition" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "leftEyePosition"                  , link = "LeftEyePosition" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "rightEyePosition"                 , link = "RightEyePosition" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "centerEyePosition"                , link = "CenterEyePosition" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "colorCameraPosition"              , link = "CameraPosition" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "deviceVelocity"                   , link = "DeviceVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "deviceAngularVelocity"            , link = "DeviceAngularVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "leftEyeVelocity"                  , link = "LeftEyeVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "leftEyeAngularVelocity"           , link = "LeftEyeAngularVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "rightEyeVelocity"                 , link = "RightEyeVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "rightEyeAngularVelocity"          , link = "RightEyeAngularVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "centerEyeVelocity"                , link = "CenterEyeVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "centerEyeAngularVelocity"         , link = "CenterEyeAngularVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "colorCameraVelocity"              , link = "CameraVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "colorCameraAngularVelocity"       , link = "CameraAngularVelocity" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "deviceAcceleration"               , link = "DeviceAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "deviceAngularAcceleration"        , link = "DeviceAngularAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "leftEyeAcceleration"              , link = "LeftEyeAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "leftEyeAngularAcceleration"       , link = "LeftEyeAngularAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "rightEyeAcceleration"             , link = "RightEyeAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "rightEyeAngularAcceleration"      , link = "RightEyeAngularAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "centerEyeAcceleration"            , link = "CenterEyeAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "centerEyeAngularAcceleration"     , link = "CenterEyeAngularAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "colorCameraAcceleration"          , link = "CameraAcceleration" },
        //     new CommonUsage { type = typeof( Vector3 )              , name = "colorCameraAngularAcceleration"   , link = "CameraAngularAcceleration" },
        //     new CommonUsage { type = typeof( Quaternion )           , name = "deviceRotation"                   , link = "DeviceRotation" },
        //     new CommonUsage { type = typeof( Quaternion )           , name = "leftEyeRotation"                  , link = "LeftEyeRotation" },
        //     new CommonUsage { type = typeof( Quaternion )           , name = "rightEyeRotation"                 , link = "RightEyeRotation" },
        //     new CommonUsage { type = typeof( Quaternion )           , name = "centerEyeRotation"                , link = "CenterEyeRotation" },
        //     new CommonUsage { type = typeof( Quaternion )           , name = "colorCameraRotation"              , link = "CameraRotation" },
        //     new CommonUsage { type = typeof( Hand )                 , name = "handData"                         , link = "HandData" },
        //     new CommonUsage { type = typeof( Eyes )                 , name = "eyesData"                         , link = "EyesData" },
        // };
    }
}