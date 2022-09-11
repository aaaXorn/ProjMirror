using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Manager : NetworkBehaviour
{
    //instance global reference
    public static Manager Instance { get; private set; }
	
	[SerializeField]
	private GameObject AIPrefab;
	private GameObject AIObject;
	
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
	[SerializeField]
	private GameObject ReadyWindow;
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
	private GameObject PiecePrefab, HolePrefab;
	
	//active pieces, not placed in a hole yet
	public List<GameObject> ActPieceList = new List<GameObject>();
	//all pieces
	public List<GameObject> PieceList = new List<GameObject>();
	private List<Collider> HoleList = new List<Collider>();
	//players
	public List<PlayerControl> PlayerList = new List<PlayerControl>();
	public Dictionary<NetworkConnectionToClient, PlayerControl> PlayerDict =
		   new Dictionary<NetworkConnectionToClient, PlayerControl>();
	//field size
	private int lines = 10, columns = 10;
	//distance between holes
	private float dist = 2f;
	private float spawn_y = 0;

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
	
	[SerializeField]
	private Text end_txt;

	[SerializeField]
	private GameObject SetupPrefab;

	//game end condition
	private void OnCurrPieces(int _Old, int _New)
	{
		if(curr_pieces >= total_pieces)
		{
			StartCoroutine("ResetGame");
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
		if(isServer)
		{
			btn_ready.onClick.AddListener(ReadyButton);

			SetupHoles();
			
			//creates the piece
			GameObject obj = Instantiate(SetupPrefab,
										 transform.position,
										 Quaternion.identity);
			//makes the piece spawn on all the clients
			NetworkServer.Spawn(obj, connectionToServer);
		}

		//total_pieces = HoleArray.Length;
		total_pieces = lines * columns;

		SetupUI();
		
		t1_txt.text = "TEAM 1: " + t1_score;
		t2_txt.text = "TEAM 2: " + t2_score;
	}

	private void SetupHoles()
    {
		float start_x = dist/2 + (lines/2 - 1) * dist;
		float start_z = dist/2 + (columns/2 -1) * dist;

		for(int l = 0; l < lines; l++)
		{
			for(int c = 0; c < columns; c++)
			{
				GameObject hole = Instantiate(HolePrefab, transform.position + new Vector3(start_x, spawn_y, start_z), Quaternion.identity);
				HoleList.Add(hole.GetComponent<Collider>());

				start_x -= dist;
			}
			start_z -= dist;
			start_x = dist/2 + (lines/2 - 1) * dist;
		}
	}
	
	#region connection
	private void ReadyChanged(bool _Old, bool _New)
	{
		//deactivates the ready button
		if(ready)
		{
			ReadyWindow.SetActive(false);
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
		else
		{
			Debug.LogError("Local PlayerControl script is null.");
			NetworkManager.singleton.StopClient();//temporary fix
		}

		if(isServer)
        {
			StartSpawn();

			//enables the hole triggers
			foreach(Collider col in HoleList)
            {
				col.enabled = true;
            }
			
			//AI
			if((float)OnlineManager.Instance.no_player % 2 != 0)
			{
				AIObject = Instantiate(AIPrefab,
									   SpawnPosition(),
									   Quaternion.identity);
				AIObject.GetComponent<AIControl>().team = 2;
				NetworkServer.Spawn(AIObject);
			}
        }
	}
	#endregion
	
	#region piece
	public Vector3 SpawnPosition()
	{
		//pos X
		float sign = (Random.Range(0, 2) == 0 ? -1 : 1);
		float x = SpawnCenter.x + Random.Range(min_dist, max_dist) * sign;
		//pos Z
		sign = (Random.Range(0, 2) == 0 ? -1 : 1);
		float z = SpawnCenter.z + Random.Range(min_dist, max_dist) * sign;
		
		Vector3 pos = new Vector3(x, SpawnCenter.y, z);
		
		return pos;
	}
	
	public void SpawnPiece()
	{
		//creates the piece
		GameObject obj = Instantiate(PiecePrefab,
									 SpawnPosition(),
									 Quaternion.identity);
		//makes the piece spawn on all the clients
		NetworkServer.Spawn(obj);

		//adds to list
		//PieceList.Add(obj);
		ActPieceList.Add(obj);
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

	private IEnumerator ResetGame()
	{
		end_txt.gameObject.SetActive(true);

		if(t1_score > t2_score)
		{
			end_txt.text = "TEAM 1 VICTORY";
		}
		else if(t2_score > t1_score)
		{
			end_txt.text = "TEAM 2 VICTORY";
		}
		else//tie
		{
			end_txt.text = "TIE";
		}

		yield return new WaitForSecondsRealtime(3);
		
		end_txt.gameObject.SetActive(false);

		ready = false;
		ReadyWindow.SetActive(true);

		if(local_PC != null)
		{
			local_PC.can_move = false;
			local_PC.transform.position = local_PC.spawn_pos;
			local_PC.transform.rotation = local_PC.spawn_rot;
			local_PC.rigid.velocity = Vector3.zero;
		}

		if(isServer)
		{
			curr_pieces = 0;
			
			t1_score = 0;
			t2_score = 0;
			
			foreach(GameObject obj in PieceList)
			{
				Destroy(obj);
			}
			PieceList.Clear();
			ActPieceList.Clear();

			foreach(Collider col in HoleList)
			{
				col.enabled = true;
			}
			HoleList.Clear();
			
			if(AIObject != null)
			{
				Destroy(AIObject);
			}
		}
		yield break;
	}
}
