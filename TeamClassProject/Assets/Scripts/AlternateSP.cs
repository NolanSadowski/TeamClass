﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;

public class AlternateSP : MonoBehaviour
{

    //public variables
    [Tooltip("Speed of the player")]
    public float speed;
    [Header("ID numbers")]
    [Tooltip("Insert player ID numbers")]
    public int playerNum, teamID;


    //private variables

    //This allows us to change what they can do, when they are either at or away from a machine.
    private enum Status { Free, AtMachine };
    private Rigidbody2D rb;
    private Vector2 vel;
    private float horizontalInput;
    private Player myPlayer;
    [SerializeField]
    private bool is_In_Area = false;
    //The current status of the player: Free (Away from machine- Free to move)
    // AtMachine(Player is at machine- not free to move).
    Status status;
    //reference to whichever machine the player is going to use
    private GameObject myMachine;


    //--------------------------------------------------------------------------------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }



    //--------------------------------------------------------------------------------------------------
    // Start is called before the first frame update
    void Start()
    {
        myMachine = null;
        status = Status.Free;
        myPlayer = ReInput.players.GetPlayer(playerNum - 1);
        ReInput.ControllerConnectedEvent += OnControllerConnected;
        CheckController(myPlayer);
    }


    //--------------------------------------------------------------------------------------------------
    // Update is called once per frame
    void Update()
    {
        if (status == Status.Free) {
            horizontalInput = myPlayer.GetAxisRaw("Horizontal");
        }else
        {
            horizontalInput = 0;
        }

        // If player is infront of machine and they press "Jump" and they are free, set their status to at a machine
        // Give the machine my inputs.
        // If the player presses "Jump" and is using a machine, set them to free again.
        // Also if they press heavy attack, they jump off the machine without using it.
        if (is_In_Area && myPlayer.GetButtonDown("Jump"))
        {
            if (status == Status.Free)
            {
                status = Status.AtMachine;
                if (!myMachine.GetComponent<MachineBehaviour>().is_In_Use) {
                    myMachine.GetComponent<MachineBehaviour>().Commence_Control(playerNum, teamID);
                }
                Debug.Log(status);
            }else if (status == Status.AtMachine)
            {
                status = Status.Free;
                if (myMachine.GetComponent<MachineBehaviour>().is_In_Use)
                {
                    myMachine.GetComponent<MachineBehaviour>().End_Control();
                    Debug.Log("Has detached from machine with Jump");
                }
                
                Debug.Log(status);
            }
        }else if (is_In_Area && myPlayer.GetButtonDown("HeavyAttack"))
        {
            if (myMachine.GetComponent<MachineBehaviour>().is_In_Use)
            {
                myMachine.GetComponent<MachineBehaviour>().End_Control();
                myMachine = null;
                Debug.Log("Has detached from machine with Heavy Attack");
            }
            if (status == Status.AtMachine)
            {
                status = Status.Free;
                Debug.Log(status);
            }

        }

        



    }

    //--------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        if (horizontalInput < -0.1f || horizontalInput > 0.1f)
        {
            Movement();
        }
    }


    //--------------------------------------------------------------------------------------------------
    //this is a temporary function that is to test whether or not the enum is working correctly.
    public void SetFree()
    {
        status = Status.Free;
    }


    //--------------------------------------------------------------------------------------------------
    //This controls the movement of the player.
    void Movement()
    {
        vel.x = horizontalInput * speed;
        vel.y = 0;

        rb.MovePosition(rb.position + Vector2.ClampMagnitude(vel, speed) * Time.deltaTime);
    }

    //--------------------------------------------------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Machine")
        {
            is_In_Area = true;
            myMachine = other.gameObject;
        }
        
    }


    //--------------------------------------------------------------------------------------------------
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag == "Machine")
        {           
            myMachine = other.gameObject;
        }
    }


    //--------------------------------------------------------------------------------------------------
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Machine")
        {
            is_In_Area = false;
            myMachine = null;
        }
    }



    //--------------------------------------------------------------------------------------------------
    ///rewired functions
    // Event that triggers when a controller is plugged in to the computer.
    void OnControllerConnected(ControllerStatusChangedEventArgs arg)
    {
        CheckController(myPlayer);
    }


    //--------------------------------------------------------------------------------------------------
    //Checks to see if this controller is a PS4 controller. If it is, change the color on the back of the controller.
    void CheckController(Player player)
    {
        foreach (Joystick joyStick in player.controllers.Joysticks)
        {
            var ds4 = joyStick.GetExtension<DualShock4Extension>();
            if (ds4 == null) continue;//skip this if not DualShock4
            switch (playerNum)
            {
                case 4:
                    ds4.SetLightColor(Color.yellow);
                    break;
                case 3:
                    ds4.SetLightColor(Color.green);
                    break;
                case 2:
                    ds4.SetLightColor(Color.blue);
                    break;
                case 1:
                    ds4.SetLightColor(Color.red);
                    break;
                default:
                    ds4.SetLightColor(Color.white);
                    Debug.LogError("Player Num is 0, please change to a number >0");
                    break;
            }


        }
    }
}
