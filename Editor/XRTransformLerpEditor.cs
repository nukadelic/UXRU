
using UnityEngine;
using UnityEditor;

namespace Utils.XR
{
    [CustomEditor( typeof( XRTransformLerp ) )]
    public class XRTransformLerpEditor : Editor
    {
        SerializedProperty s_pivot;

        void OnEnable()
        {
            s_pivot = serializedObject.FindProperty("pivot");
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            var target = ( XRTransformLerp ) this.target;

            // target.usePivot = EditorGUILayout.Toggle( "Use Pivot", target.usePivot );

            // if( target.usePivot )
            // {
            //     EditorGUILayout.PropertyField( s_pivot );
            // }

            // serializedObject.ApplyModifiedProperties();
        }
    }
}