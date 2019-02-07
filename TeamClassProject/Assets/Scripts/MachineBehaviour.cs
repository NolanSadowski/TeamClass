﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Rewired.ControllerExtensions;

public class MachineBehaviour : MonoBehaviour
{
    [Tooltip("Is the current machine in use?")]
    public bool is_In_Use = false;
    [Header("All Hazards")]
    [Tooltip("Insert all GameObjects that will be controlled by THIS machine")]
    public GameObject[] Controlled_Hazzard;
    [Tooltip("Vertical and Horizontal move speed of certain hazzards")]
    public float speed;
    [Header("Max Hazzards")]
    [Tooltip("How many hazzards will this machine have access to?")]
    public int max_Machines_Amnt;

    //These are the different MACHINE TYPES
    public enum MachineID { SideCannon, BackgroundCannon, SideHazard, MovingPlatform, SpecialPlatform };
    [Header("Machine Type")]
    [Tooltip("What type of machine will this be?")]
    public MachineID mach;


    //inputs are for movement
    private float horizontalInput, verticalInput;
    //this allows us to change between hazzards
    private int Current_Haz_Num = 0;
    //velocity
    private Vector2 vel;
    
    private bool can_Use;
    //rewired after this point
    //myPlayer will properly connect this players inputs to go to the correct location in rewired
    private Player myPlayer;
    //This is in order to "Spawn in" objects
    private ObjectSpawner objectPool;
    private Vector3 Move_Rotation, originalRotation;
    


    // Start is called before the first frame update
    void Start()
    {
        originalRotation = new Vector3(Controlled_Hazzard[Current_Haz_Num].transform.rotation.x, 
            Controlled_Hazzard[Current_Haz_Num].transform.rotation.y,
            Controlled_Hazzard[Current_Haz_Num].transform.rotation.z);

        Move_Rotation = originalRotation;
        //get an instance of the object spawner so we can spawn objects
        objectPool = ObjectSpawner.Instance;

        //only do this if this machine is of type "Background Cannon" 
        if (mach == MachineID.BackgroundCannon)
        {
            Controlled_Hazzard[0].GetComponent<SpriteRenderer>().color = Color.white;
            Controlled_Hazzard[0].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if the machine is in use
        if (is_In_Use) {
            
            if (mach == MachineID.SideCannon)
            {
                verticalInput = myPlayer.GetAxisRaw("Vertical");
                SideCannonMovement();
            } else if (mach == MachineID.BackgroundCannon)
            {
                verticalInput = myPlayer.GetAxisRaw("Vertical");
                horizontalInput = myPlayer.GetAxisRaw("Horizontal");
                BackgroundCannonMovement();
            }
            else if (mach == MachineID.SideHazard)
            {
                verticalInput = myPlayer.GetAxisRaw("Vertical");
                SideHazzardControl();
            }/*else if (mach == MachineID.MovingPlatform)
            {

            }else if (mach == MachineID.SpecialPlatform)
            {

            }*/
        }
    }


    //--------------------------------------------------------------------------------------------------
    /// <summary>
    /// Function description: This contains all movement and interactions for the Side Cannons
    /// </summary>
    void SideCannonMovement()
    {

        //this allows players to change which side hazzard is currently selected
        if (myPlayer.GetButtonDown("Special"))
        {
            if (Current_Haz_Num < max_Machines_Amnt)
            {
                Current_Haz_Num++;
                Move_Rotation = originalRotation;
            }

        }

        if (Current_Haz_Num >= max_Machines_Amnt)
        {
            Current_Haz_Num = 0;
        }

        
        if (verticalInput > 0.1f)
        {
            Move_Rotation.z += Time.deltaTime * speed;
        }
        if (verticalInput < -0.1f)
        {
            Move_Rotation.z -= Time.deltaTime * speed;
        }

        Controlled_Hazzard[Current_Haz_Num].transform.rotation = Quaternion.Euler(Move_Rotation);

    }



    //--------------------------------------------------------------------------------------------------
    /// <summary>
    /// Function Description: This contains all movement and interactions for the Background Cannon.
    /// </summary>
    void BackgroundCannonMovement()
    {
        vel.x = horizontalInput * speed;
        vel.y = verticalInput * speed;

        //this allows the player to spawn an object in the position where the crosshair is
        if (myPlayer.GetButtonDown("Jump") && can_Use)
        {
            objectPool.SpawnFromPool("Tester", Controlled_Hazzard[Current_Haz_Num].transform.position, Quaternion.identity);
            End_Control();
            Debug.Log("Has Spawned object");
        }

        //this allows the player to move the crosshair
        Controlled_Hazzard[Current_Haz_Num].GetComponent<Rigidbody2D>().MovePosition(Controlled_Hazzard[Current_Haz_Num].GetComponent<Rigidbody2D>().position
            + Vector2.ClampMagnitude(vel, speed) * Time.deltaTime);
    }



    //--------------------------------------------------------------------------------------------------
    /// <summary>
    /// Function Description: This contains all movement and interactions for the Side Hazzards (currently Eels).
    /// </summary>
    void SideHazzardControl()
    {

        vel.y = verticalInput * speed;

        //this allows players to change which side hazzard is currently selected
        if (myPlayer.GetButtonDown("Special"))
        {
            if (Current_Haz_Num < max_Machines_Amnt)
            {
                Current_Haz_Num++;
            }

        }
        //reset current_haz_Num if it is greater than or equal to the max number of hazzards
        if (Current_Haz_Num >= max_Machines_Amnt)
        {
            Current_Haz_Num = 0;
        }


        //this allows the player to move the side cannon (will be changed to rotation)
        Controlled_Hazzard[Current_Haz_Num].GetComponent<Rigidbody2D>().MovePosition(Controlled_Hazzard[Current_Haz_Num].GetComponent<Rigidbody2D>().position
            + Vector2.ClampMagnitude(vel, speed) * Time.deltaTime);
    }

    //--------------------------------------------------------------------------------------------------
    /// <summary>
    ///
    //This Function requires access to the correct player number in order to have the correct player interact with the hazzards.
    /// </summary>
    /// <param name="playerNum">Player ID from the support character that is activating this machine.</param>
    /// <param name="teamID">Player's Team ID</param>
    public void Commence_Control(int playerNum, int teamID)
    {
        // Recieve the number of a player and use it as my inputs.
        myPlayer = ReInput.players.GetPlayer(playerNum - 1);

        is_In_Use = true;

        StartCoroutine(waitForUse());

        if (mach == MachineID.BackgroundCannon) {
            Controlled_Hazzard[0].SetActive(true);
            switch (teamID)
            {
                case 2:
                    Controlled_Hazzard[0].GetComponent<SpriteRenderer>().color = Color.cyan;
                    break;
                case 1:
                    Controlled_Hazzard[0].GetComponent<SpriteRenderer>().color = Color.red;
                    break;
                default:
                    Controlled_Hazzard[0].GetComponent<SpriteRenderer>().color = Color.black;
                    break;
            }
        }
        Debug.Log("Player:"+playerNum+ " has activated hazzard: "+mach);
    }


    //--------------------------------------------------------------------------------------------------
    /// <summary>
    /// Turns off the inputs that were being recieved from the player.
    /// Player is no longer going to use this machine for now.
    /// </summary>
    public void End_Control()
    {
        is_In_Use = false;
        can_Use = false;
        // The playerID "-1" does not exist, therefore, the inputs will never be recieved.
        myPlayer = ReInput.players.GetPlayer(-1);
        
        if (mach == MachineID.BackgroundCannon) {
            Controlled_Hazzard[0].GetComponent<SpriteRenderer>().color = Color.white;
            Controlled_Hazzard[0].SetActive(false);
        }
        
        Debug.Log("Player has deactivated machine: "+transform.name);
    }

    //--------------------------------------------------------------------------------------------------
    // This function just draws Gizmos in unity
    private void OnDrawGizmos()
    {
        if (mach == MachineID.SideHazard) {
            Gizmos.color = new Color32(255, 0, 0, 200);
            for (int i = 0; i < Controlled_Hazzard.Length; i++) {
                Gizmos.DrawLine(new Vector3(Controlled_Hazzard[i].transform.position.x,
                    Controlled_Hazzard[i].transform.position.y + 0.6f, Controlled_Hazzard[i].transform.position.x),
                    new Vector3(Controlled_Hazzard[i].transform.position.x,
                    Controlled_Hazzard[i].transform.position.y - 0.6f, Controlled_Hazzard[i].transform.position.x));
            }
        }

        if (mach == MachineID.BackgroundCannon)
        {
            Gizmos.color = new Color32(0,255,0, 80);
        }
        if (mach == MachineID.SideCannon)
        {
            Gizmos.color = new Color32(0, 0, 255, 125);
        }
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }


    IEnumerator waitForUse()
    {
        yield return new WaitForSeconds(0.05f);
        Debug.Log("Now can use");
        can_Use = true;
    }

}
