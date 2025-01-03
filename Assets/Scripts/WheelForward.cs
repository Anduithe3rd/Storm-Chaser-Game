using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WheelForward : MonoBehaviour
{

    /*
    first attempt at making the driving 
    im gonna stop here and probably make a new script because i dont see how my current method
    will work with the car turning and making different scripts for front and back seems odd (wheels)
    next attempt i think im gonna work on how to keep the car off the ground first 
    (physics and collisions on wheels seems like a bad idea, maybe raycasting?)


    next time notes:
    - serialize field and make things private
    - put things in fixed update to avoid having to say Time.deltaTime repeatedly
    - use GetComponent<> instead of public 
    ` maybeeee use comments more to actually annotate what were doing
    */



    

    Vector3 beg;
    float move;
    float turn;
    public Transform t;
    public float WheelRotationSpeed = 20;
    public float Speed = 200;
    public Rigidbody CarRB;

    float grip = 1;
    void Start()
    {

        beg = new Vector3(t.position.x, t.position.y, t.position.z);        

    }

    // Update is called once per frame
    void Update()
    {
        turn = Input.GetAxis("Horizontal") * Time.deltaTime * WheelRotationSpeed;
        move = Input.GetAxis("Vertical") * Time.deltaTime * Speed;
        WheelRotation();
        MoveForward();

    }

    void WheelRotation(){

        float yRot = t.eulerAngles.y;
        Debug.Log(yRot);

        if (yRot > 180)
        {
            yRot -= 360;
        }

        if(yRot <= 45 & yRot >= -45){
            t.Rotate(0, turn , 0);
            
        }else{


            if(yRot > 45){
                t.eulerAngles = new Vector3(t.eulerAngles.x, 44.9f, t.eulerAngles.z);
            }
            else if(yRot < -45){
                t.eulerAngles = new Vector3(t.eulerAngles.x, -44.9f, t.eulerAngles.z);
            }
        }
        

    }

    void MoveForward(){

        Vector3 wheelDir = t.forward;
        CarRB.AddForceAtPosition(move * wheelDir, t.position);
    }

    


}

