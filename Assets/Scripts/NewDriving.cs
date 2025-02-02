using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Callbacks;
using UnityEngine;

public class NewDriving : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform[] rays; // Array of raycast origins (positioned near each wheel for suspension checks)
    [SerializeField] LayerMask driveable; // Define what surface we can drive on
    [SerializeField] Transform accelPoint; // Point where acceleration will be applied

    //suspension properties
    [SerializeField] float springStiff;
    [SerializeField] float restLength; // Default length of suspension
    [SerializeField] float springTravel; // Maximum travel distance of suspension
    [SerializeField] float wheelRadius;
 
    [SerializeField] float damperStiff; // Damping force

    // Player input variables
    private float moveIn = 0;
    private float steerIn = 0;

    // Driving force and steering properties
    [SerializeField] private float accel = 25f; 
    [SerializeField] private float maxSpeed = 100f; 
    [SerializeField] private float decel = 10f; 
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve; //Adjusts steering sensitivity based on speed
    [SerializeField] private float drag; 


    // Velocity tracking
    private Vector3 currentVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    // Ground detection of each wheel
    private int[] wheelsGround = new int[4]; 
    private bool isGrounded = false;

    // All physics of the Car placed in fixed update for consistency
    private void FixedUpdate() {
        suspension();
        GroundCheck();   
        CalculateCarVelocity();
        Movement();

    }

    private void Update(){
        GetPlayerIn();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    #region Inputs

    private void GetPlayerIn(){
        // Forward/backward input and left/right steering input
        moveIn= Input.GetAxis("Vertical");
        steerIn = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Car check

    private void GroundCheck(){
        int tempGroundedWheels = 0;
        //how many wheels are grounded
        for(int i = 0; i < wheelsGround.Length; i++){
            tempGroundedWheels += wheelsGround[i];
        }
        //more than one wheel is on the ground so were grounded
        if(tempGroundedWheels > 1){
            isGrounded = true;
        }
        else{
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity(){
        currentVelocity = transform.InverseTransformDirection(rb.velocity); 
        carVelocityRatio = currentVelocity.z / maxSpeed; 
    }

    #endregion

    #region Car suspension

    void suspension(){

        for(int i = 0; i < rays.Length; i++)
        {

             RaycastHit hit;
             float maxLength = restLength + springTravel; // Max suspension stretch

            //Raycast downward to detect ground and simulate suspension
            if(Physics.Raycast(rays[i].position, -rays[i].up, out hit, maxLength + wheelRadius, driveable))
            {
                wheelsGround[i] = 1;

                // Calculate spring compression
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompress = (restLength - currentSpringLength) / springTravel;

                //Calculate damp force
                float springVelo = Vector3.Dot(rb.GetPointVelocity(rays[i].position), rays[i].up);
                float dampForce = damperStiff * springVelo;

                //Apply spring and damp force
                float springForce = springStiff * springCompress;
                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rays[i].up, rays[i].position);
                Debug.DrawLine(rays[i].position, hit.point, Color.red); //debugging line for wheels
            }
            else{
                wheelsGround[i] = 0;
                Debug.DrawLine(rays[i].position, rays[i].position + (wheelRadius + maxLength) * -rays[i].up, Color.blue);
            }

        }
    }

    #endregion

    #region Movement 

    private void Movement(){
        //if we're on the ground apply all physics of car movement
        if(isGrounded){
            Turn();
            Acceleration();
            Deceleration();
            SidewaysDrag();
            Debug.Log("isGrounded");
        }
        else{
            Debug.Log("NOT");
        }

    }
    private void Acceleration(){
        rb.AddForceAtPosition(accel * moveIn * transform.forward, accelPoint.position, ForceMode.Acceleration); //Forward movement
    }
    
    private void Deceleration(){
        rb.AddForceAtPosition(decel * moveIn * -transform.forward, accelPoint.position, ForceMode.Acceleration); //Braking
    }

    private void Turn(){
        rb.AddTorque(steerStrength * steerIn * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration); //steering torque
    }
    
    private void SidewaysDrag(){
        float currentSideways = currentVelocity.x; //detect sideways movement

        float dragMagnitude = -currentSideways * drag; //apply drag

        Vector3 dragForce = transform.right * dragMagnitude;
        rb.AddForceAtPosition(dragForce, rb.worldCenterOfMass, ForceMode.Acceleration); //reduce sliding
    }
    
    #endregion

}
