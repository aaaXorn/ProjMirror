using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
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
    public GameObject Owner;

    //if the piece has already been set
    private bool p_set;

    //hole location
    private Transform target;
	
    [SerializeField]
    private float spd, rot_spd, scale_spd;
	[SerializeField]
	private Vector3 TargetScale;
	/*[SyncVar(hook = nameof(OnChangeScale))]
	private Vector3 CurrScale;*/
	
    [SerializeField]
    private LayerMask piece_layer;
	
	#region raycast
    //raycast range
    [SerializeField]
    private float ray_range;
	
	public class RaycastResult: IComparable<RaycastResult>
	{
		public float distance;
		public Collider collider;
		public Vector2 textureCoord;
		public string tag;
		public RaycastResult(RaycastHit hit)
		{
			distance = hit.distance;
			collider = hit.collider;
			textureCoord = hit.textureCoord;
			tag =  hit.transform.gameObject.tag;
		}
	 
		public int CompareTo(RaycastResult other)
		{
			return distance.CompareTo(other.distance);
		}
	}
	private RaycastHit[] rc_hits;
	private List<RaycastResult> hitList = new List<RaycastResult>();
	
	private void SortDistances(RaycastHit[] sort_hits)
    {
        hitList.Clear();
        rc_hits = sort_hits;
		
        foreach(RaycastHit hit in rc_hits)
        {
            hitList.Add(new RaycastResult(hit));
        }
        hitList.Sort();
    }
	#endregion

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
	/*
	private void OnChangeScale(Vector3 _Old, Vector3 _New)
	{
		transform.localScale = _New;
	}*/
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
			
			Manager.Instance.curr_pieces++;
            
            target = other.transform;
            //move the piece
            StartCoroutine("MoveTo");

            other.enabled = false;

			Rpc_IsTrigger();
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
	
		#region raycasts
        //checks for other pieces and counts them
        RaycastHit[] hits;
		
		int no = 0;
		int scr = 0;
		//x
		int no1 = 0;
		int no2 = 0;
		
		#region X
        hits = Physics.RaycastAll(transform.position, Vector3.right * 1000, piece_layer);
		SortDistances(hits);
		
        foreach(RaycastResult hit in hitList)
        {
			if(hit.tag == gameObject.tag && hit.distance <= ray_range * (no1 + 1))
                no1++;
        }
		
		//-x
		hits = Physics.RaycastAll(transform.position, -Vector3.right * 1000, piece_layer);
		SortDistances(hits);
		
        foreach(RaycastResult hit in hitList)
        {
			if(hit.tag == gameObject.tag && hit.distance <= ray_range * (no2 + 1))
                no2++;
        }
		
		no = no1 + no2;
		
		if(no == 1)
			scr += 5;
		else if(no == 2)
			scr += 15;
		else if(no >= 3)
			scr += 30;
		
		#endregion
		
		#region Z
		no = 0;
		
		//z
		no1 = 0;
		
        hits = Physics.RaycastAll(transform.position, Vector3.forward * 1000, piece_layer);
		SortDistances(hits);
		
        foreach(RaycastResult hit in hitList)
        {
			if(hit.tag == gameObject.tag && hit.distance <= ray_range * (no1 + 1))
                no1++;
        }
		
		//-z
		no2 = 0;
		
        hits = Physics.RaycastAll(transform.position, -Vector3.forward * ray_range, piece_layer);
		SortDistances(hits);
		
        foreach(RaycastResult hit in hitList)
        {
			if(hit.tag == gameObject.tag && hit.distance <= ray_range * (no2 + 1))
                no2++;
        }
		
		no = no1 + no2;
		
		if(no == 1)
			scr += 5;
		else if(no == 2)
			scr += 15;
		else if(no >= 3)
			scr += 30;
        #endregion
		
		if(team == 1)
			Manager.Instance.t1_score += scr;
		else if(team == 2)
			Manager.Instance.t2_score += scr;
		else
			Debug.LogError("Score error: team not defined.");
		
		Manager.Instance.SpawnPiece();
		#endregion
	}
	
	[ClientRpc]
	private void Rpc_IsTrigger()
    {
		GetComponent<Collider>().isTrigger = true;
    }

    //moves the piece to the hole
    private IEnumerator MoveTo()
    {
		Manager.Instance.PieceList.Add(gameObject);
		Manager.Instance.ActPieceList.Remove(gameObject);

        rigid.isKinematic = true;
	
		//if the movement or rotation are complete
        bool completeM = false, completeR = false, completeS = false;

        while (!completeM || !completeR || !completeS)
        {
			//movement
            if (Vector3.Distance(transform.position, target.position) > 0)
                transform.position = Vector3.MoveTowards(transform.position, target.position, spd * Time.deltaTime);
            else if(!completeM) completeM = true;
			
			//rotation
            if (transform.rotation != target.rotation)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target.rotation, rot_spd * Time.deltaTime);
            else if(!completeR) completeR = true;
			
			if(transform.localScale != TargetScale)
				transform.localScale = Vector3.MoveTowards(transform.localScale, TargetScale, scale_spd * Time.deltaTime);
			else if(!completeS) completeS = true;
			
			//waits for next Update to continue
            yield return new WaitForEndOfFrame();
        }
		
		//checks if this will change the score
		ScoreCheck();
		
        yield break;
    }
    #endregion
}
