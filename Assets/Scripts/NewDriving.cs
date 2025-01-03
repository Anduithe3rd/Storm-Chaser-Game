using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.Callbacks;
using UnityEngine;

public class NewDriving : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] Transform[] rays;
    [SerializeField] LayerMask driveable;
    [SerializeField] Transform accelPoint;


    [SerializeField] float springStiff;
    [SerializeField] float restLength;
    [SerializeField] float springTravel;
    [SerializeField] float wheelRadius;

    [SerializeField] float damperStiff;

    private float moveIn = 0;
    private float steerIn = 0;


    [SerializeField] private float accel = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float decel = 10f;


    private Vector3 currentVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    private int[] wheelsGround = new int[4];
    private bool isGrounded = false;

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

        moveIn= Input.GetAxis("Vertical");
        steerIn = Input.GetAxis("Horizontal");
    }

    #endregion

    #region Car check

    private void GroundCheck(){
        int tempGroundedWheels = 0;

        for(int i = 0; i < wheelsGround.Length; i++){
            tempGroundedWheels += wheelsGround[i];
        }

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
             float maxLength = restLength + springTravel;

        
            if(Physics.Raycast(rays[i].position, -rays[i].up, out hit, maxLength + wheelRadius, driveable))
            {
                wheelsGround[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompress = (restLength - currentSpringLength) / springTravel;

                float springVelo = Vector3.Dot(rb.GetPointVelocity(rays[i].position), rays[i].up);
                float dampForce = damperStiff * springVelo;

                float springForce = springStiff * springCompress;

                float netForce = springForce - dampForce;

                rb.AddForceAtPosition(netForce * rays[i].up, rays[i].position);
                Debug.DrawLine(rays[i].position, hit.point, Color.red);
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
        
        if(isGrounded){
            Acceleration();
            Deceleration();
        }

    }
    private void Acceleration(){
        rb.AddForceAtPosition(accel * moveIn * transform.forward, accelPoint.position, ForceMode.Acceleration);
    }
    
    private void Deceleration(){
        rb.AddForceAtPosition(decel * moveIn * -transform.forward, accelPoint.position, ForceMode.Acceleration);
    }
    
    #endregion

}
