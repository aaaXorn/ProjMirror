using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PieceCheck : NetworkBehaviour
{
    private Material material;
    private Rigidbody rigid;

    //which team scored
    public int team = 0;

    //player the carrying piece
    [SyncVar]
    public PlayerControl Owner;

    //if the piece has already been set
    private bool p_set;

    //hole location
    private Transform target;

    [SerializeField]
    private float spd, rot_spd;

    [SerializeField]
    private LayerMask piece_layer;

    //raycast range
    [SerializeField]
    private float ray_range;

    //variables
    public override void OnStartClient()
    {
        material = GetComponent<Renderer>().material;
        material.color = Manager.Instance.mat[0];
        rigid = GetComponent<Rigidbody>();
    }
    
    #region grab
    //changes the material's color to the scoring team's
    [ClientRpc]
    public void Rpc_ChangeColor(int p_team)
    {
        team = p_team;
        material.color = Manager.Instance.mat[team];
    }

    //IEnumerator grab movement
    #endregion

    #region score
    private void OnTriggerEnter(Collider other)
    {
        //ignore if not coming from the host
        //or if the piece doesn't come from a team
        if (!isServer || team == 0 || rigid.isKinematic) return;

        if (other.CompareTag("Hole") && !p_set)
        {
            p_set = true;
            
            target = other.transform;
            //move the piece
            StartCoroutine("MoveTo");

            other.enabled = false;
        }
    }

    private void ScoreCheck()
	{
		//changes the object's tags
		switch(team)
		{
			case 1:
				gameObject.tag = "Team1";
				break;
			case 2:
				gameObject.tag = "Team2";
				break;
			default:
				Debug.LogError("Piece has no assigned team.");
				break;
		}

        //checks for other pieces and counts them
        RaycastHit[] hits;

        hits = Physics.RaycastAll(transform.position, Vector3.right * ray_range, piece_layer);
		
        int no = 0;
        foreach(RaycastHit hit in hits)
        {
            if(hit.transform.gameObject.tag == gameObject.tag)
                no++;
        }
        print(no);
	}
	
    //moves the piece to the hole
    private IEnumerator MoveTo()
    {
        rigid.isKinematic = true;
	
		//if the movement or rotation are complete
        bool completeM = false, completeR = false;

        while (!completeM || !completeR)
        {
			//movement
            if (Vector3.Distance(transform.position, target.position) > 0)
                transform.position = Vector3.MoveTowards(transform.position, target.position, spd * Time.deltaTime);
            else if(!completeM) completeM = true;
			
			//rotation
            if (transform.rotation != target.rotation)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rot_spd * Time.deltaTime);
            else if(!completeR) completeR = true;
			
			//waits for next Update to continue
            yield return new WaitForEndOfFrame();
        }
		
		//checks if this will change the score
		ScoreCheck();
		
        yield break;
    }
    #endregion
}
