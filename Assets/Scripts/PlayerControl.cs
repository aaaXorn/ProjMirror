using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerControl : NetworkBehaviour
{
    //movement inputs
    private float inputX, inputZ;
    //movement speed
    [SerializeField]
    private float h_spd;

    //rotation input
    private Vector3 mouse_posDiff = Vector3.zero;//difference between current and previous mouse position
    private Vector3 mouse_lastPos = Vector3.zero;//previous mouse position
    //rotation speed
    [SerializeField]
    private float cam_x_spd, cam_y_spd;

    //camera
    public override void OnStartLocalPlayer()
    {
        mouse_lastPos = Input.mousePosition;
    }

    private void Update()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;
        
        //movement inputs
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        //mouse inputs
        mouse_posDiff = Input.mousePosition - mouse_lastPos;
        mouse_lastPos = Input.mousePosition;

        //rotation
        if(mouse_posDiff != Vector3.zero)
        {
            transform.Rotate(0, mouse_posDiff.x * cam_x_spd * Time.deltaTime, 0);
        }
    }

    private void FixedUpdate()
    {
        //skips if object isn't owned by client
        if (!isLocalPlayer) return;

        //movement

    }
}
