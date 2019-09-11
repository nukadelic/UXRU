using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Utils.XR
{
    // [ExecuteInEditMode]
    public class XRPlayer : MonoBehaviour
    {
        [Header("Input devices")]
        public XRTracker deviceHead;
        public XRTracker deviceLeft;
        public XRTracker deviceRight;

        [Header("Hierarchy")]
        public GameObject containerOuter;
        public GameObject containerBody;
        public GameObject containerInner;

        [Header("Multipliers")]
        [Range(0.2f, 1f)]
        public float dropMultiplier = 0.8f;
        [Range(0.2f, 1f)]
        public float flatMultiplier = 0.8f;
        [Range(0.2f, 1f)]
        public float tiltMultiplier = 0.8f;
        
        [Header("Player")]
        public float height = 2f;

        Rigidbody bodyRigidBody;
        CapsuleCollider bodyCollider;

        void OnEnable() 
        {
            bodyRigidBody = containerBody.GetComponent<Rigidbody>();
            bodyCollider = containerBody.GetComponent<CapsuleCollider>();
        }

        void OnDisable()
        {

        }


        void Update()
        {
            bodyRigidBody.velocity = new Vector3
            (
                bodyRigidBody.velocity.x * flatMultiplier,
                bodyRigidBody.velocity.y * dropMultiplier,
                bodyRigidBody.velocity.z * flatMultiplier
            );

        }

        void FixedUpdate()
        {
            MoveCollider();

            // TODO: refactor this into XRMovement
            if( deviceRight?.isActiveAndEnabled == true ) 
                updateMovement();
        }
        
        void MoveCollider()
        {
            // Move inner container inverse to the head horizontal position to match body collider position  
            Vector3 head = deviceHead.transform.localPosition;
            containerInner.transform.localPosition = new Vector3( - head.x, 0, - head.z );

            // Adjust collder size and Y position 
            float h = height + head.y;
            bodyCollider.height = h;
            bodyCollider.center = new Vector3( 0, head.y - h / 2, 0 );

            // Move outer container towards the head those making the body collider "follow" the headset
            float outer_y = containerOuter.transform.localPosition.y;
            float angle = bodyCollider.transform.rotation.eulerAngles.y;
            Quaternion yRotation = Quaternion.Euler( 0, angle, 0 );
            Vector3 headOuter = yRotation * deviceHead.transform.localPosition;
            containerOuter.transform.localPosition = new Vector3( headOuter.x, outer_y, headOuter.z );
        }

        ///////////////////////////////////////

        [Header("Speed")]
        public float speedTurn = 66f;
        public float speedHorizontal = 400f;
        public float speedVertical = 300f;


        public void Teleport( Vector3 position, bool clear_velocity = true )
        {
            transform.position = position;

            containerOuter.transform.localPosition = Vector3.zero;
            containerInner.transform.localPosition = Vector3.zero;
            bodyRigidBody.transform.localPosition = Vector3.zero;
            
            if( clear_velocity ) bodyRigidBody.velocity = Vector3.zero;

            MoveCollider();
        }

        float tilt_angle = 0f;

        static readonly float sleepAxis = 0.05f;

        void updateMovement()
        {
            if( deviceRight.GetButtonDown( XRButtons.primaryButton ) )
            {
                Teleport( new Vector3( 0, 0.2f, 0 ) );
                return;
            }

            // -- [ Lift ] --
            float grip_right = deviceRight.GetAxisGrip();
            if( grip_right > sleepAxis )
            {
                Vector3 force = speedVertical * grip_right * Time.deltaTime * Vector3.up;
                bodyRigidBody.AddRelativeForce( force, ForceMode.Force );
            }
            float grip_left = deviceLeft.GetAxisGrip();
            if( grip_left > sleepAxis )
            {
                Vector3 force = speedVertical / 2 * grip_left * Time.deltaTime * Vector3.down;
                bodyRigidBody.AddRelativeForce( force, ForceMode.Force );
            }

            Vector2 axis_left = deviceLeft.GetAxisPrimary();
            Vector2 axis_right = deviceRight.GetAxisPrimary();

            if( axis_left.magnitude > sleepAxis )
            {
                // -- [ Turn ] --
                Vector3 body_center = bodyCollider.transform.position + bodyCollider.center;
                float yRot = axis_left.x * speedTurn * Time.deltaTime;
                bodyRigidBody.transform.RotateAround( body_center, Vector3.up, yRot );
            }

            // -- [ Tilt ] --

            if( axis_left.y > 0.5f )
            {
                tilt_angle += axis_left.y * speedTurn * Time.deltaTime;
                tilt_angle = Mathf.Clamp( tilt_angle, -75, 75 );
            }
            else    tilt_angle *= tiltMultiplier;

            float look_Angle = 0 - deviceHead.transform.localRotation.eulerAngles.y; //  + bodyRigidBody.transform.localRotation.eulerAngles.y;
            float dx = Mathf.Cos( look_Angle * Mathf.PI / 180 );
            float dz = Mathf.Sin( look_Angle * Mathf.PI / 180 );
            containerInner.transform.localRotation = Quaternion.identity;
            containerInner.transform.Rotate( new Vector3( dx, 0, dz ), tilt_angle , Space.Self );

            if( axis_right.magnitude > sleepAxis )
            {
                // -- [ Move ] -- 
                // What angle on the xz plane the player is currently looking at  
                float y_Angle = deviceHead.transform.rotation.eulerAngles.y;
                Quaternion zx_Roation = Quaternion.Euler( 0, y_Angle, 0 );
                float z_Speed = axis_right.y * speedHorizontal * Time.deltaTime; // Forward speed  
                float x_Speed = axis_right.x * speedHorizontal * Time.deltaTime; // Sideways speed
                // Move towards where the player is currenly is looking
                Vector3 directional_velocity = zx_Roation * new Vector3( x_Speed, 0, z_Speed );
                bodyRigidBody.AddForce( directional_velocity, ForceMode.Force );
            }

            // if( deviceRight.GetButtonDown( CommonUsages.primaryButton ) )
            //     bodyRigidBody.AddRelativeForce( Vector3.up * 4f, ForceMode.Impulse );
        }

        //////////////////////////
        
        void OnDrawGizmos()
        {
            if( bodyCollider == null ) OnEnable();

            Gizmos.color = Color.red;

            Gizmos.DrawWireCube( 
                bodyCollider.transform.position + bodyCollider.center,
                new Vector3(
                    bodyCollider.radius * 2,
                    bodyCollider.height,
                    bodyCollider.radius * 2
                ) 
            );

            Gizmos.color = Color.white;
        }
    }    
}
