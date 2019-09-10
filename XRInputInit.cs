using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputInit : MonoBehaviour
{
    void Awake()
    {
        Application.targetFrameRate = 120;

        DontDestroyOnLoad(gameObject);
    }

    // for fps calculation.
    private int frameCount;
    private float elapsedTime;
    private double frameRate;

    private void Update()
    {
        // https://gist.github.com/sanukin39/63bdfcf6c57466abe6d44ac5a79c5ca9

        // FPS calculation
        frameCount++;
        elapsedTime += Time.deltaTime;
        if (elapsedTime > 0.5f)
        {
            frameRate = System.Math.Round(frameCount / elapsedTime, 1, System.MidpointRounding.AwayFromZero);
            frameCount = 0;
            elapsedTime = 0;
        }
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

        GUILayout.Label( frameRate + " fps" );

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
            List<int> values  = ( (GameViewRenderMode[] ) Enum.GetValues( typeof( GameViewRenderMode ) ) )
                .Select( rModeVal => (int) rModeVal ).ToList();

            var index = values.IndexOf( ( int ) XRSettings.gameViewRenderMode ) + 1;

            if( index > values.Count - 1 ) index = 0; // Avoid None

            XRSettings.gameViewRenderMode = ( GameViewRenderMode ) values[ index ];
        }
        GUILayout.EndHorizontal();
    }
}
