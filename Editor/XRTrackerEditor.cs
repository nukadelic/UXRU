
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.XR;

namespace Utils.XR
{
    [CanEditMultipleObjects]
    [CustomEditor( typeof( XRTracker ) )]
    public class XRTrackerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            
            var target = ( XRTracker ) this.target;

            if( target.trackTransformation != XRTracker.UpdateFlags.Never )
            {
                target.trackPosition = EditorGUILayout.Toggle( "Track Position", target.trackPosition );
                target.trackRotation = EditorGUILayout.Toggle( "Track Rotation", target.trackRotation );
            }
            
            // GUILayout.Label("update flags: " + target.update.ToString() + " " + ( (int) target.update ) );

            if( ! target.trackedDevice.isValid )
            {
                EditorGUILayout.HelpBox( "Device information will be displayed here in play mode", MessageType.Info );
                return;
            }

            TrackedButtonsGUI();

            GUILayout.Space( 5 );

            DebugFeaturesGUI();
            
            GUILayout.Space( 5 );

            TestHapticsGUI();
        }

        bool featuresVisible = false;

        void DebugFeaturesGUI()
        {
            var target = ( XRTracker ) this.target;

            featuresVisible = EditorGUILayout.ToggleLeft( "View Feautes", featuresVisible, EditorStyles.boldLabel );
            
            if( ! featuresVisible ) return;

            EditorGUILayout.LabelField( "Count", target.featureNames.Count.ToString() );

            if( target.featureNames.Count > 0 )
            {
                GUILayout.Label("Names", EditorStyles.boldLabel );

                using( new GUILayout.VerticalScope( EditorStyles.helpBox ) )
                {
                    for( int i = 0; i < target.featureNames.Count; ++i )
                    {
                        var featureName = target.featureNames[ i ];
                        GUILayout.Label( featureName );
                    }
                }
            }

            // target.featureNames
        }

        void TrackedButtonsGUI()
        {
            var target = ( XRTracker ) this.target;

            EditorGUILayout.LabelField( "Device Name", target.trackedDevice.name );

            if( target.trackedButtons.Count > 0 )
            {
                GUILayout.Label("Tracked Buttons", EditorStyles.boldLabel );

                using( new GUILayout.VerticalScope( EditorStyles.helpBox ) )
                {
                    for( int i = 0; i < target.trackedButtons.Count; ++i )
                    {
                        var btnName = target.trackedButtons[ i ].name;
                        bool down = target.trackedButtons_state[ i ];

                        EditorGUILayout.LabelField( btnName, down ? "Active" : "Idle" );
                    }
                }

            }
        }

        bool hapticsTest = false;
        int hapticsResult = 0;
        int hapticsChannel = 0;
        float hapticsAmplitude = 1f;
        float hapticsDuration = 1f;

        void TestHapticsGUI()
        {   
            GUILayout.Space( 5 );

            // hapticsTest = EditorGUILayout.BeginFoldoutHeaderGroup( hapticsTest, "Test Haptics" );
            // EditorGUILayout.EndFoldoutHeaderGroup();

            hapticsTest = EditorGUILayout.ToggleLeft( "Test Haptics", hapticsTest, EditorStyles.boldLabel );

            if( ! hapticsTest ) return;

            GUILayout.BeginVertical( EditorStyles.helpBox );

            GUILayout.Space( 5 );

            var target = ( XRTracker ) this.target;

            var device = target.trackedDevice;

            hapticsChannel = EditorGUILayout.IntSlider( "Channel", hapticsChannel, 0, 10  );
            hapticsAmplitude = EditorGUILayout.Slider( "Amplitude", hapticsAmplitude, 0, 1  );
            hapticsDuration = EditorGUILayout.Slider( "Duration", hapticsDuration, 0.1f, 2.0f  );

            GUILayout.BeginHorizontal();

            if( GUILayout.Button( "Impulse" ) )
            {
                bool result = device.SendHapticImpulse( (uint) hapticsChannel, hapticsAmplitude, hapticsDuration );

                hapticsResult = result ? 1 : -1;
            }

            if( GUILayout.Button( "Stop" ) )
            {
                device.StopHaptics();
            }

            if( hapticsResult == -1 )
            {
                EditorGUILayout.HelpBox( "Failed to send impulse", MessageType.Error );
            }

            GUILayout.EndHorizontal();

            GUILayout.Space( 5 );

            GUILayout.EndVertical();
            
        }
    }
}