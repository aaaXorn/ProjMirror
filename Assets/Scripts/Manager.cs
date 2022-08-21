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
	//local player script
	public PlayerControl local_PC;
	
	//if the game is ready
	[SyncVar(hook = nameof(ReadyChanged))]
	public bool ready;
	
	//how many holes must be filled until the game resets
	[SyncVar(hook = nameof(OnCurrPieces))]
	public int curr_pieces;
	private	int total_pieces;
	
	[SerializeField]
	private Button btn_ready;
	#endregion

	#region piece
	//score
	[SerializeField]
	private Text t1_txt, t2_txt;
	[SyncVar(hook = nameof(OnTeam1Score))]
	public float t1_score;
	[SyncVar(hook = nameof(OnTeam2Score))]
	public float t2_score;

	[SerializeField]
	private GameObject PiecePrefab;

	[SerializeField]
	private Collider[] HoleArray;

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
	
	//game end condition
	private void OnCurrPieces(int _Old, int _New)
	{
		if(curr_pieces >= total_pieces)
		{
			
		}
	}
	private void OnTeam1Score(float _Old, float _New)
	{
		t1_txt.text = "TEAM 1: " + _New;
	}
	private void OnTeam2Score(float _Old, float _New)
	{
		t2_txt.text = "TEAM 2: " + _New;
	}
	
    private void Awake()
    {
        //sets global reference
        if (Instance == null) Instance = this;
        //if there's  already an instance, remove this
        else Destroy(gameObject);
    }
	
	private void Start()
	{
		btn_ready.onClick.AddListener(ReadyButton);
		
		total_pieces = HoleArray.Length;
		
		SetupUI();
		
		t1_txt.text = "TEAM 1: " + t1_score;
		t2_txt.text = "TEAM 2: " + t2_score;
	}
	
	#region connection
	private void ReadyChanged(bool _Old, bool _New)
	{
		//deactivates the ready button
		if(ready)
		{
			btn_ready.gameObject.SetActive(false);
			GameStart();
		}
		
		//updates ready UI settings
		SetupUI();
	}
	
	public void ReadyButton()
	{
		if(isServer)
		{
			if(NetworkServer.connections.Count >= 1)//!!!change to 2 later
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
	public void SetupUI()
	{
		//so only the host can start the game
		if(isServer) btn_ready.interactable = true;
	}
	
	//starts the game
	public void GameStart()
	{
		if(local_PC != null)
		{
			local_PC.can_move = true;
		}
		else Debug.LogError("Local PlayerControl script is null.");

		if(isServer)
        {
			StartSpawn();

			//enables the hole triggers
			foreach(Collider col in HoleArray)
            {
				col.enabled = true;
            }
        }
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
