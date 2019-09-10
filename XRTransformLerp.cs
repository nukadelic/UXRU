
using UnityEngine;

namespace Utils.XR
{
    public class XRTransformLerp : MonoBehaviour
    {
        [Range(5f, 99.9f)]
        public float weight = 30f;

        Vector3 start_position;
        Quaternion start_rotation;

        Vector3 last_position;
        Quaternion last_rotation;

        // [HideInInspector]
        // public bool usePivot = false;

        // [HideInInspector]
        // public Transform pivot;

        public UpdateAt updateAt = UpdateAt.NormalUpdate;
        public enum UpdateAt
        {
            FirstFixedUpdate, 
            NormalUpdate, 
            LateUpdate
        }

        public bool lerpPosition = false;
        public bool lerpRotation = true;

        void Start()
        {
            start_position = transform.localPosition;
            start_rotation = transform.localRotation;

            last_position = transform.position;
            last_rotation = transform.rotation;
        }

        bool fixedUpdate = false;

        void FixedUpdate()
        {
            if( updateAt == UpdateAt.FirstFixedUpdate )
            {
                if( fixedUpdate )
                {
                    fixedUpdate = false;

                    Lerp();
                }
            }
        }

        void Update()
        {
            if( updateAt == UpdateAt.NormalUpdate ) Lerp();
        }

        void LateUpdate()
        {
            fixedUpdate = false;

            if( updateAt == UpdateAt.LateUpdate ) Lerp();
        }

        void Lerp()
        {
            // float w = EasingFunction.EaseInCubic( 0, 1, ( 100 - weight ) / 100 );
            float W = 1 - ( 0.5f + weight / 200 );

            if( lerpPosition )
            {
                transform.localPosition = start_position;
                Vector3 delata_pos = transform.position - last_position;
                transform.position = last_position + delata_pos * W ;
                last_position = transform.position;
            }
            
            if( lerpRotation )
            {
                transform.localRotation = start_rotation;
                Quaternion delta_rot = transform.rotation * Quaternion.Inverse( last_rotation );
                transform.rotation = Quaternion.Lerp( last_rotation, transform.rotation, W );
                last_rotation = transform.rotation;
            }

        }
    }
}