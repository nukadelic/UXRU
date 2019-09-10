using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR;

namespace Utils.XR
{
    public class XRBounds : MonoBehaviour
    {
        static List<Vector3> points;

        [Tooltip("If set to true, cones boundaries will be static in world space, otherwise they will be added to this gameObject")]
        public bool worldSpace = false;
        
        List<GameObject> objectBounds;

        [Tooltip("The model place around the boundary, keep null to use default cylinder")]
        public GameObject model = null;

        void Awake()
        {
            points = new List<Vector3>();
            objectBounds = new List<GameObject>();
        }

        void Update()
        {
            // Rift S
            // Without guardian setup:
            // - Boundary.TryGetDimensions       =>         (495647600000000000000.0, 0.0, 0.0)
            // - Boundary.TryGetGeometry         =>         zero length list ... duh
            // With guardian setup: 
            // - Boundary.TryGetDimensions       =>         (3, 0, 2.77)
            // - Boundary.TryGetGeometry         =>         (-1.5, -1.5,  1.4)
            //                                              ( 1.5, -1.5,  1.4)
            //                                              ( 1.5, -1.5, -1.4)
            //                                              (-1.5, -1.5, -1.4)

            // Boundary.TryGetDimensions(out boundaryDimensions)

            points.Clear();

            if( Boundary.TryGetGeometry( points ) )
            {
                for (int i = 0; i < points.Count; i++) 
                {
                    if( objectBounds.Count < i + 1 ) MakeObject();

                    objectBounds[ i ].transform.position = points[ i ];
                }
            }
        }

        void MakeObject()
        {
            GameObject newObject;
            
            if( model != null ) newObject = Instantiate( model, Vector3.zero, Quaternion.identity ); 

            else newObject = GameObject.CreatePrimitive( PrimitiveType.Cylinder );

            newObject.transform.localScale = Vector3.one * 0.2f;

            if( ! worldSpace ) newObject.transform.parent = gameObject.transform;

            objectBounds.Add( newObject );
        }
    }
}