using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace UXRU.Crap
{
    public class XRScreenGUI : MonoBehaviour
    {
        private List<int> RenderModeIndexes; 

        private FpsCalculator fps;

        void Awake()
        {
            fps = new FpsCalculator();

            RenderModeIndexes = ( 
                ( GameViewRenderMode[] ) Enum.GetValues( typeof( GameViewRenderMode ) ) 
                ).Select( enumValue => ( int ) enumValue ).ToList();

            Application.targetFrameRate = 120;

            DontDestroyOnLoad(gameObject);
        }

        float frameRate = 0;

        void Update()
        {
            frameRate = fps.Update();
        }

        bool gui_open = true;
    
        void OnGUI()
        {
            GUIStyle s = GUI.skin.textArea;

            GUILayout.BeginHorizontal(s);   GUILayout.Space( 5 );
            GUILayout.BeginVertical();      GUILayout.Space( 5 );

            GUILayout.BeginHorizontal();
            
            if( GUILayout.Button( gui_open ? "[ - ]" : "[ + ]", GUILayout.Width( 40 ) ) )
                gui_open = ! gui_open;

            GUILayout.Label( frameRate.ToString("0.00") + " fps" );

            GUILayout.EndHorizontal();

            if( gui_open ) DrawGUI();

            GUILayout.EndVertical();        GUILayout.Space( 5 );
            GUILayout.EndHorizontal();      GUILayout.Space( 5 );
        }

        void DrawGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current render mode (Only works in standarlone): " );
            if( GUILayout.Button( XRSettings.gameViewRenderMode.ToString(), GUILayout.Width( 160 ) ) ) 
            {
                var index = 1 + RenderModeIndexes.IndexOf( ( int ) XRSettings.gameViewRenderMode );

                if( index > RenderModeIndexes.Count - 1 ) index = 1; // 1 as the starting value to avoid 'None'

                XRSettings.gameViewRenderMode = ( GameViewRenderMode ) RenderModeIndexes[ index ];
            }
            GUILayout.EndHorizontal();
        }
    }

    internal class FpsCalculator
    {
        // https://gist.github.com/sanukin39/63bdfcf6c57466abe6d44ac5a79c5ca9

        int frameCount;
        float elapsedTime;
        float frameRate;

        public float Update()
        {
            // FPS calculation
            frameCount ++;
            elapsedTime += Time.deltaTime;
            if ( elapsedTime > 0.5f )
            {
                frameRate = (float) System.Math.Round(
                    frameCount / elapsedTime, 1, 
                    System.MidpointRounding.AwayFromZero
                );
                frameCount = 0;
                elapsedTime = 0;
            }

            return frameRate;
        }
    }
}

