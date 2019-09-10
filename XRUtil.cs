using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using System;

namespace Utils.XR
{
    public class XRNodeActivity
    {
        public XRNode node;
        public ulong id = 0;
        public bool tracked = false;

        public bool Equals( XRNodeState xrNode )
        {   
            return node == xrNode.nodeType && id == xrNode.uniqueID;
        }

        public override int GetHashCode()
        {
            return ( int ) this.id;
        }
    }

    public class XRUtil
    {
        static bool Debug = true;

        static List<XRNodeActivity> nodes;

        static XRUtil()
        {
            nodes = new List<XRNodeActivity>();
        }

        [RuntimeInitializeOnLoadMethod]
        static private void Initialize()
        {
            // return;

            InputTracking.nodeRemoved += nodeState => {

                // Log( "- - - " );
                // Log("Node Removed" );
                // Lof( "- {0,-24}\t {1}", "ID", nodeState.uniqueID );
                // Lof( "- {0,-24}\t {1}", "Type", nodeState.nodeType );
                // Lof( "- {0,-24}\t {1}", "Tracked", nodeState.tracked );
                
                string problem = "_";

                var result = nodes.Where( n => n.Equals( nodeState ) );

                if( result.Count() > 0 ) nodes.Remove( result.First() );
                
                // else Log("- /!\\ But it wasn't yet registered" );
                else problem = "R";
                
                Lof("({0}) Node Added\tID:{1} \t (tracked:{2}) \t Type: {3}", 
                    problem,
                    nodeState.uniqueID, 
                    nodeState.tracked,
                    nodeState.nodeType 
                );
            };

            InputTracking.nodeAdded += nodeState => {

                // Log( "- - - " );
                // Log("Node Added");
                // Lof( "- {0,-24}\t {1}", "ID", nodeState.uniqueID );
                // Lof( "- {0,-24}\t {1}", "Type", nodeState.nodeType );
                // Lof( "- {0,-24}\t {1}", "Tracked", nodeState.tracked );

                string problem = "_";

                var result = nodes.Where( n => n.Equals( nodeState ) );

                if( result.Count() == 0 )
                    nodes.Add( new XRNodeActivity { 
                        node = nodeState.nodeType, 
                        id = nodeState.uniqueID 
                });

                else problem = "E";
                // else Log("- /!\\ But it was allready registered");

                Lof("({0}) Node Added\tID:{1} \t (tracked:{2}) \t Type: {3}", 
                    problem,
                    nodeState.uniqueID, 
                    nodeState.tracked,
                    nodeState.nodeType 
                );
            };

            InputTracking.trackingAcquired += nodeState => {

                // Log( "- - - " );
                // Log("Node Tracking Acquired" );
                // Lof( "- {0,-24}\t {1}", "ID", nodeState.uniqueID );
                // Lof( "- {0,-24}\t {1}", "Type", nodeState.nodeType );
                // Lof( "- {0,-24}\t {1}", "Tracked", nodeState.tracked );

                string problem = "_";

                var result = nodes.Where( n => n.Equals( nodeState ) );

                // if( result.Count() == 0 ) Log("- /!\\ But it wasn't yet registered");
                if( result.Count() == 0 ) problem = "R";

                else 
                {
                    if( ! result.First().tracked ) 
                    
                        result.First().tracked = true;

                    // else Log("- /!\\ But it was allready tracked");
                    else problem = "A";
                }
                
                Lof("({0}) Node Added\tID:{1} \t (tracked:{2}) \t Type: {3}", 
                    problem,
                    nodeState.uniqueID, 
                    nodeState.tracked,
                    nodeState.nodeType 
                );
            };

            InputTracking.trackingLost += nodeState => {
                
                // Log( "- - - " );
                // Log("Node Tracking Lost" );
                // Lof( "- {0,-24}\t {1}", "ID", nodeState.uniqueID );
                // Lof( "- {0,-24}\t {1}", "Type", nodeState.nodeType );
                // Lof( "- {0,-24}\t {1}", "Tracked", nodeState.tracked );
                
                string problem = "_";

                var result = nodes.Where( n => n.Equals( nodeState ) );
                
                // if( result.Count() == 0 ) Log("- /!\\ But it wasn't yet registered");
                if( result.Count() == 0 ) problem = "R";

                else
                {
                    if( result.First().tracked ) 
                        
                        result.First().tracked = false;

                    // else Log("- /!\\ But it wasn't yet tracked");
                    else problem = "T";
                }

                Lof("({0}) Node Added\tID:{1} \t (tracked:{2}) \t Type: {3}", 
                    problem,
                    nodeState.uniqueID, 
                    nodeState.tracked,
                    nodeState.nodeType 
                );
            };


            InputDevices.deviceConnected += device => {

                if( ! Debug ) return;
                
                Log( "- - - " );
                Log( "Device Connected" );
                Lof( "- {0,-24}\t {1}", "Name", device.name );
                Lof( "- {0,-24}\t {1}", "Role", device.role );
                Lof( "- {0,-24}\t {1}", "Valid", device.isValid );

                HapticCapabilities haptics;
                if( device.TryGetHapticCapabilities( out haptics ) )
                {   
                    Log("- Device Haptics:");
                    Lof("- - {0,-36}\t {1}", "bufferFrequencyHz",   haptics.bufferFrequencyHz );
                    Lof("- - {0,-36}\t {1}", "bufferMaxSize",       haptics.bufferMaxSize );
                    Lof("- - {0,-36}\t {1}", "bufferOptimalSize",   haptics.bufferOptimalSize );
                    Lof("- - {0,-36}\t {1}", "numChannels",         haptics.numChannels );
                    Lof("- - {0,-36}\t {1}", "supportsBuffer",      haptics.supportsBuffer );
                    Lof("- - {0,-36}\t {1}", "supportsImpulse",     haptics.supportsImpulse );
                }

                // if( device.SendHapticImpulse( 0, 1 ) )   
                // {
                //     Log("- Device haptics - Impulse is reactive");
                //     device.StopHaptics();
                // }

                var featureUsages = new List<InputFeatureUsage>();

                if( device.TryGetFeatureUsages( featureUsages ) ) 
                {
                    Log("- Device Features:");
                    foreach( var feature in featureUsages )
                        Lof( "- - {0,-36}\t {1} \t ( {2} )" , feature.name, 
                            DeviceFeatureIsReactive( device, feature ), feature.type
                    );
                }
            };

            InputDevices.deviceDisconnected += device => {

                if( ! Debug ) return;

                Log( "- - - " );
                Log( "Device Connection Lost" );
                Lof( "- {0,-24}\t {1}", "Name", device.name );
                Lof( "- {0,-24}\t {1}", "Role", device.role );
            };
        }

        static internal void Log( params object[] args )
        {
            if( Debug ) UnityEngine.Debug.Log( "[XR] " + string.Join(" ", args ) );
        }
        static internal void Lof( string formatString, params object[] args )
        {
            if( Debug ) UnityEngine.Debug.LogFormat( "[XR] " + formatString, args );
        }
        static internal void LogError( string message )
        {
            if( Debug ) UnityEngine.Debug.LogError( "[XR] " + message );
        }

        static public bool DeviceFeatureIsReactive( InputDevice device, InputFeatureUsage feature )
        {
            bool value_bool;
            if( feature.type == typeof( bool ) )
                return device.TryGetFeatureValue( feature.As<bool>(), out value_bool );

            float value_float;
            if( feature.type == typeof( float ) )
                return device.TryGetFeatureValue( feature.As<float>(), out value_float );
            
            Vector2 value_vector2;
            if( feature.type == typeof( Vector2 ) )
                return device.TryGetFeatureValue( feature.As<Vector2>(), out value_vector2 );
            
            Vector3 value_vector3;
            if( feature.type == typeof( Vector3 ) )
                return device.TryGetFeatureValue( feature.As<Vector3>(), out value_vector3 );

            Quaternion value_quaternion;
            if( feature.type == typeof( Quaternion ) )
                return device.TryGetFeatureValue( feature.As<Quaternion>(), out value_quaternion );

            System.UInt32 value_int32;
            if( feature.type == typeof( System.UInt32 ) )
                return device.TryGetFeatureValue( feature.As<System.UInt32>(), out value_int32 );

            InputTrackingState value_inputTrackingState;
            if( feature.type == typeof( InputTrackingState ) )
                return device.TryGetFeatureValue( feature.As<InputTrackingState>(), out value_inputTrackingState );
            
            Hand value_hand;
            if( feature.type == typeof( Hand ) )
                return device.TryGetFeatureValue( feature.As<Hand>(), out value_hand );

            Eyes value_eyes;
            if( feature.type == typeof( Eyes ) )
                return device.TryGetFeatureValue( feature.As<Eyes>(), out value_eyes );

            return false;
        }

    }
}
