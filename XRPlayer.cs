using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Utils.XR
{
    // [ExecuteInEditMode]
    public class XRPlayer : MonoBehaviour
    {
        static readonly float sleepAxis = 0.05f;

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
        [Range(0.5f, 0.98f)]
        public float tiltMultiplier = 0.98f;
        
        [Header("Player")]
        public float height = 2f;

        [Header("Speed")]
        public float speedTurn = 66f;
        public float speedTilt = 33f;
        public float speedHorizontal = 1000f;
        public float speedVertical = 1300f;
        public float boostScale = 1.56f;

        Rigidbody bodyRigidBody;
        CapsuleCollider bodyCollider;

        /// Current tilt angle
        float tilt_angle = 0f;
        /// Limit in degrees - how far can it tilt before locking
        float tilt_limit = 45f;
        /// How much of the noraml force will be applied using tilt
        float tilt_force = 0.75f;
        /// What should be the axis min value before applyting tilt
        ///     this is usful when using rotation only and avoid hyper sensative tilt
        float tilt_forward_min = 0.25f;

        public XRAxis tiltAxis = XRAxis.positive( 0.25f, 1 );

        void Awake()
        {
            tiltAxis = XRAxis.positive( tilt_forward_min, 1 );
        }

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
            PopulateValues();

            MoveCollider();

            // TODO: refactor this into XRMovement
            if( deviceRight?.isActiveAndEnabled == true ) 
                UpdateMovement();
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

        public void Teleport( Vector3 position, bool clear_velocity = true )
        {
            transform.position = position;

            containerOuter.transform.localPosition = Vector3.zero;
            containerInner.transform.localPosition = Vector3.zero;
            bodyRigidBody.transform.localPosition = Vector3.zero;
            
            if( clear_velocity ) bodyRigidBody.velocity = Vector3.zero;

            MoveCollider();
        }

        Vector2 _axis_left;
        Vector2 _axis_right;
        
        bool _axis_left_down;
        bool _axis_right_down;

        float _grip_right;
        float _grip_left;

        float _abs_right_x;
        float _abs_right_y;
        float _abs_left_x;
        float _abs_left_y;

        void PopulateValues()
        {
            _grip_right = deviceRight.GetAxisGrip();
            _grip_left = deviceLeft.GetAxisGrip();
            
            _axis_left = deviceLeft.GetAxisPrimary();
            _axis_right = deviceRight.GetAxisPrimary();
            
            _axis_left_down = deviceLeft.GetButton( XRButtons.primary2DAxisClick );
            _axis_right_down = deviceRight.GetButton( XRButtons.primary2DAxisClick ); 

            _abs_right_x = Mathf.Abs( _axis_right.x );
            _abs_right_y = Mathf.Abs( _axis_right.y );
            _abs_left_x = Mathf.Abs( _axis_left.x );
            _abs_left_y = Mathf.Abs( _axis_left.y );
        }

        void UpdateMovement()
        {
            if( deviceRight.GetButtonDown( XRButtons.primaryButton ) )
            {
                Teleport( new Vector3( 0, 0.2f, 0 ) );
                return;
            }

            // -- [ Lift Up ] --

            if( _grip_right > sleepAxis )
            {
                Vector3 force = speedVertical * _grip_right * Time.deltaTime * Vector3.up;

                if( _axis_right_down ) force *= boostScale;

                bodyRigidBody.AddRelativeForce( force, ForceMode.Force );
            }

            // -- [ Drop down ] --

            if( _grip_left > sleepAxis )
            {
                Vector3 force = speedVertical / 2 * _grip_left * Time.deltaTime * Vector3.down;

                if( _axis_left_down ) force *= boostScale;

                bodyRigidBody.AddRelativeForce( force, ForceMode.Force );
            }

            // -- [ Turn ] --

            if( _abs_right_x > sleepAxis )
            {
                Vector3 body_center = bodyCollider.transform.position + bodyCollider.center;

                float delta_angle = _axis_right.x * speedTurn * Time.deltaTime;

                if( _axis_right_down ) delta_angle *= boostScale;

                bodyRigidBody.transform.RotateAround( body_center, Vector3.up, delta_angle );
            }

            UpdateTilt();
            
            if( _abs_left_x > sleepAxis || _abs_right_y > sleepAxis  )
            {
                float mx = _axis_left.x;
                float my = _axis_right.y;

                if( _axis_left_down )  mx *= boostScale;
                if( _axis_right_down ) my *= boostScale;

                Move( mx, my );
            }

            // if( deviceRight.GetButtonDown( CommonUsages.primaryButton ) )
            //     bodyRigidBody.AddRelativeForce( Vector3.up * 4f, ForceMode.Impulse );
        }

        void UpdateTilt()
        {
            // Tilt only forwards
            if( tiltAxis.Active( _axis_left.y ) )
            // if( axis_left.y > tilt_forward_min )
            {
                // [ tilt_forward_min , 1 ] -> [ 0, 1 ]
                float value = tiltAxis.Solve( _axis_left.y );
                // float value = ( axis_left.y - tilt_forward_min ) / ( 1 - tilt_forward_min );

                tilt_angle += value * speedTilt * Time.deltaTime;
                tilt_angle = Mathf.Clamp( tilt_angle, - tilt_limit, tilt_limit );
            }

            else if( _axis_left.y < sleepAxis && Mathf.Abs( tilt_angle ) > 0 ) 
            {
                tilt_angle *= tiltMultiplier;

                if( Mathf.Abs( tilt_angle ) < sleepAxis ) tilt_angle = 0;
            }

            float look_Angle = 0 - deviceHead.transform.localRotation.eulerAngles.y; //  + bodyRigidBody.transform.localRotation.eulerAngles.y;
            float dx = Mathf.Cos( look_Angle * Mathf.PI / 180 );
            float dz = Mathf.Sin( look_Angle * Mathf.PI / 180 );

            containerInner.transform.localRotation = Quaternion.identity;
            containerInner.transform.Rotate( new Vector3( dx, 0, dz ), tilt_angle , Space.Self );

            // Also add additional motion when tilted forward
            if( tilt_angle > 0 )
            {
                float value = Mathf.Abs( tilt_angle ) / tilt_limit;
                // [ 0, 1 ] => [ 0, 1, 0 ]
                // value = ( value - 0.5f ) * 2;
                // reduce force for the tilt motion
                value = value * tilt_force;
                // flip the value
                // value = - value;

                if( _axis_left_down ) value *= boostScale;

                Move( 0, value );
            }
        }

        void Move( float sideway, float forward )
        {
            // What angle on the xz plane the player is currently looking at  
            float y_Angle = deviceHead.transform.rotation.eulerAngles.y;
            Quaternion zx_Roation = Quaternion.Euler( 0, y_Angle, 0 );
            float x_Speed = sideway * speedHorizontal * Time.deltaTime;
            float z_Speed = forward * speedHorizontal * Time.deltaTime; 
            // Move towards where the player is currenly is looking
            Vector3 directional_velocity = zx_Roation * new Vector3( x_Speed, 0, z_Speed );
            bodyRigidBody.AddForce( directional_velocity, ForceMode.Force );
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
