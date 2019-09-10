using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Utils.XR
{
    // [ExecuteInEditMode]
    public class XRPlayer : MonoBehaviour
    {
        [Header("XR Trackers")]
        public XRTracker deviceHead;
        public XRTracker deviceLeft;
        public XRTracker deviceRight;

        public GameObject head;
        public GameObject body;

        Rigidbody bodyRigidBody;
        CapsuleCollider bodyCollider;

        void OnEnable() 
        {
            bodyRigidBody = body.GetComponent<Rigidbody>();
            bodyCollider = body.GetComponent<CapsuleCollider>();
        }

        void OnDisable()
        {

        }

        void FixedUpdate()
        {
            // TODO: refactor this into XRMovement
            if( deviceRight?.isActiveAndEnabled == true ) 
                updateMovement();

            UpdateCollider();
        }
        
        ///////////////////////////////////////

        public float height = 2f;

        public float h_scale = 1f;

        void UpdateCollider()
        {
            Vector3 headPos = deviceHead.transform.localPosition;
            
            float h = height + headPos.y;

            // Adjust collder size and Y position 
            bodyCollider.height = h;
            bodyCollider.center = new Vector3( 0, headPos.y - h / 2, 0 );

            Vector3 head_xz = new Vector3( headPos.x, 0, headPos.z );
            Vector3 head_y = new Vector3( 0, transform.localPosition.y, 0 );

            head_xz = Vector3.Project( head_xz, deviceHead.transform.forward );

            // Move root container towards head
            transform.localPosition = new Vector3( headPos.x, transform.localPosition.y, headPos.z );
            // transform.localPosition = new Vector3( head_xz.x, transform.localPosition.y, head_xz.z );
            // transform.localPosition = head_xz + head_y;
            // Move head container the other way around 
            head.transform.localPosition = new Vector3( - headPos.x, 0, - headPos.z );
            // head.transform.localPosition = new Vector3( - head_xz.x, 0, - head_xz.z );
            // head.transform.localPosition = head_y - head_xz;
        }

        ///////////////////////////////////////

        public float angularSpeed = 66f;
        public float speed = 400f;

        [Tooltip("Hello")]
        public float yLiftSpeed = 0.05f;

        public void Center()
        {
            bodyRigidBody.transform.localPosition = new Vector3();
        }

        void updateMovement()
        {
            if( deviceRight.HasAxisPrimary() )
            {
                Vector2 axis = deviceRight.GetAxisPrimary();

                if( axis.magnitude > 0.1f )
                {
                    float yRot = axis.x * angularSpeed * Time.deltaTime;
                    // Vector3 angles = bodyRigidBody.transform.localRotation.eulerAngles;
                    // angles.y += yRot;
                    // bodyRigidBody.transform.localRotation = Quaternion.Euler( angles );
                    bodyRigidBody.transform.RotateAround( bodyCollider.transform.position + bodyCollider.center, Vector3.up, yRot );
                    // bodyRigidBody.transform.Rotate( new Vector3( 0, yRot, 0 ), Space.Self );

                    
                    // -- MOVE FORWARD / BACKWARDS -- 
                    // Vector3 delta = new Vector3( 0, 0, axis.y ) * speed * Time.deltaTime;
                    // Vector3 localForward = transform.worldToLocalMatrix.MultiplyVector( transform.forward );
                    // transform.localPosition += Vector3.Project( delta, localForward );
                    // bodyRigidBody.transform.localPosition += tracker_head.transform.forward * axis.y * speed * Time.deltaTime;
                    // bodyRigidBody.transform.localPosition += bodyRigidBody.transform.forward * axis.y * speed * Time.deltaTime;
                    // bodyRigidBody.AddForce( bodyRigidBody.transform.forward * axis.y * speed * Time.deltaTime, ForceMode.Force );
                    Quaternion headRotY = Quaternion.Euler( 0, deviceHead.transform.rotation.eulerAngles.y, 0 );
                    float moveForward = axis.y * speed * Time.deltaTime;
                    float moveSideways = 0f;
                    Vector3 moveForce = new Vector3( moveSideways, 0, moveForward );
                    bodyRigidBody.AddForce( headRotY * moveForce, ForceMode.Force );
                    
                }
            }

            if( deviceRight.GetButtonDown( CommonUsages.primaryButton ) )
            {   
                bodyRigidBody.AddRelativeForce( Vector3.up * 4f, ForceMode.Impulse );
            }
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
