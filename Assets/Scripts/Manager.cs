using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Manager : NetworkBehaviour
{
    //instance global reference
    public static Manager Instance { get; private set; }
	
	#region connection
	//if the game is ready
	[SyncVar(hook = nameof(ReadyChanged))]
	public bool ready;
	
	[SerializeField]
	private Button btn_ready;
	#endregion
	
	#region piece
	[SerializeField]
	private GameObject PiecePrefab;

	//number of spawned pieces
	[SerializeField]
	private int pieces;

    //material colors
    public Color[] mat;
	
	[Tooltip("Where the object spawning circumference is centered.")]
	[SerializeField]
	private Vector3 SpawnCenter;
	[Tooltip("Maximum distance from the center.")]
	[SerializeField]
	private float max_dist;
	[Tooltip("Minimum distance from the center.")]
	[SerializeField]
	private float min_dist;
	#endregion
	
    private void Awake()
    {
        //sets global reference
        if (Instance == null) Instance = this;
        //if there's  already an instance, remove this
        else Destroy(gameObject);
    }
	
	#region connection
	private void ReadyChanged(bool _Old, bool _New)
	{
		//deactivates the ready button
		if(ready)
		{
			btn_ready.gameObject.SetActive(false);
		}
		
		//updates ready UI settings
		SetupUI();
	}
	
	public void ReadyButton()
	{
		if(isServer)
		{
			if(NetworkServer.connections.Count > 2)
			{
				ready = true;
			}
			else
			{
				print("Not enough players.");
			}
		}
	}
	
	//ready UI settings
	private void SetupUI()
	{
		//so only the host can start the game
		if(!isServer) btn_ready.interactable = false;
		
		
	}
	#endregion
	
	#region piece
	public void SpawnPiece()
	{
		//pos X
		float sign = (Random.Range(0, 1) == 0 ? -1 : 1);
		float x = SpawnCenter.x + Random.Range(min_dist, max_dist) * sign;
		//pos Z
		sign = (Random.Range(0, 1) == 0 ? -1 : 1);
		float z = SpawnCenter.z + Random.Range(min_dist, max_dist) * sign;
		
		//creates the piece
		GameObject obj = Instantiate(PiecePrefab,
									 new Vector3(x, SpawnCenter.y, z),
									 Quaternion.identity);
		//makes the piece spawn on all the clients
		NetworkServer.Spawn(obj);
	}
	
	//spawn a number of pieces based on the pieces var
	private void StartSpawn()
	{
		for(int i = 0; i < pieces; i++)
		{
			SpawnPiece();
		}
	}
	#endregion
}
