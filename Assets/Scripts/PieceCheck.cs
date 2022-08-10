using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCheck : NetworkBehaviour
{
    private Material material;
    private Rigidbody rigid;

    //which team scored
    [SyncVar]
    public int team = 0;

    //if the piece has already been set
    private bool set;

    //hole location
    private Transform target;

    [SerializeField]
    private float spd, rot_spd;

    //variables
    public override void OnStartClient()
    {
        material = GetComponent<Renderer>().material;
        material.color = Manager.Instance.mat[0];
        rigid = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //ignore if not coming from the host
        if (!isServer) return;
        
        if(other.CompareTag("Hole") && !set)
        {
            set = true;

            //change the piece's color
            Rpc_ChangeColor();
            target = other.transform;
            //move the piece
            StartCoroutine("MoveTo");
        }
    }

    //changes the material's color to the scoring team's
    [ClientRpc]
    public void Rpc_ChangeColor()
    {
        material.color = Manager.Instance.mat[team];
    }

    //moves the piece to the hole
    private IEnumerator MoveTo()
    {
        rigid.isKinematic = true;

        bool completeM = false, completeR = false;

        while (!completeM || !completeR)
        {
            print("pinto");

            if (Vector3.Distance(transform.position, target.position) > 0)
                transform.position = Vector3.MoveTowards(transform.position, target.position, spd * Time.deltaTime);
            else if(!completeM) completeM = true;

            if (transform.rotation != target.rotation)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rot_spd * Time.deltaTime);
            else if(!completeR) completeR = true;

            yield return new WaitForEndOfFrame();
        }

        yield break;
    }
}
