using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Manager : NetworkBehaviour
{
	[HideInInspector]
    //instance global reference
    public static Manager Instance { get; private set; }
	
	[SerializeField] Transform _transf_listener;

	[SerializeField]
	private GameObject AIPrefab;
	private GameObject AIObject;
	[SerializeField] private Transform AI_spawn;
	
	#region connection
	//local player script
	[HideInInspector]
	public PlayerControl local_PC;
	
	//if the game is ready
	[HideInInspector]
	[SyncVar(hook = nameof(ReadyChanged))]
	public bool ready;
	
	//how many holes must be filled until the game resets
	[HideInInspector]
	[SyncVar(hook = nameof(OnCurrPieces))]
	public int curr_pieces;
	private	int total_pieces;
	
	[SerializeField]
	private Button btn_ready;
	[SerializeField]
	private GameObject[] ReadyObjs;
	#endregion

	#region piece
	//score
	[SerializeField]
	private Text t1_txt, t2_txt;
	[HideInInspector]
	[SyncVar(hook = nameof(OnTeam1Score))]
	public float t1_score;
	[HideInInspector]
	[SyncVar(hook = nameof(OnTeam2Score))]
	public float t2_score;
	[SerializeField]
	private Image img_scr1, img_scr2;

	[SerializeField]
	private GameObject PiecePrefab, HolePrefab;
	
	//active pieces, not placed in a hole yet
	[HideInInspector]
	public List<GameObject> ActPieceList = new List<GameObject>();
	//all pieces
	[HideInInspector]
	public List<GameObject> PieceList = new List<GameObject>();
	private List<Collider> HoleList = new List<Collider>();
	//players
	[HideInInspector]
	public List<PlayerControl> PlayerList = new List<PlayerControl>();
	public Dictionary<NetworkConnectionToClient, PlayerControl> PlayerDict =
		   new Dictionary<NetworkConnectionToClient, PlayerControl>();
	[Tooltip("numero de linhas e colunas do tabuleiro")]
	[SerializeField]
	//field size
	private int lines = 10, columns = 10;
	//distance between holes
	private float dist = 2f;
	private float spawn_y = 6.68f;

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
	
	private bool reset, timer;
	[SerializeField]
	private float win_score;

	[SerializeField]
	private AudioSource audioS_WL, audioS_1min;
	[SerializeField]
	private AudioClip aClip_win, aClip_lose;
	
	[SerializeField]
	private float match_time;
	
	string game_time;

	[SerializeField] Text txt_MatchTime;

	[SerializeField] Color[] clr_MatchTime;

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
		if(_New >= win_score)
		{
			img_scr1.fillAmount = 1f;

			if(isServer)
			{
				StopCoroutine("MatchTimer");
			}

			StartCoroutine("ResetGame");
		}
		else
			img_scr1.fillAmount = _New / win_score;
	}
	private void OnTeam2Score(float _Old, float _New)
	{
		t2_txt.text = "TEAM 2: " + _New;
		if(_New >= win_score)
		{
			img_scr2.fillAmount = 1f;

			if(isServer)
			{
				StopCoroutine("MatchTimer");
			}

			StartCoroutine("ResetGame");
		}
		else
			img_scr2.fillAmount = _New / win_score;
	}
	
    private void Awake()
    {
        //sets global reference
        if (Instance == null) Instance = this;
        //if there's  already an instance, remove this
        else Destroy(gameObject);

		QuitBtn = QuitBtn_Reference;
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

		txt_MatchTime.text = match_time.ToString();

		if(ready) Chat.Instance.ChangeCanvasEnabled();
	}

	private void SetupHoles()
    {
		float start_x = dist/2 + (lines/2 - 1) * dist;
		float start_z = dist/2 + (columns/2 -1) * dist;

		for(int l = 0; l < lines; l++)
		{
			for(int c = 0; c < columns; c++)
			{
				GameObject hole = Instantiate(HolePrefab, transform.position + new Vector3(start_x, spawn_y, start_z - 0.84f), Quaternion.identity);
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
		print("ready" + _New);
		//deactivates the ready button
		if(ready)
		{
			foreach(GameObject obj in ReadyObjs)
				obj.SetActive(false);
			GameStart();
		}
		else
		{
			foreach(GameObject obj in ReadyObjs)
				obj.SetActive(true);
		}
		
		//updates ready UI settings
		SetupUI();
	}
	
	public void ReadyButton()
	{
		if(isServer)
		{
			if(NetworkServer.connections.Count >= 1)
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
	
	[SerializeField]
	GameObject QuitBtn_Reference;
	public static GameObject QuitBtn;

	//starts the game
	public void GameStart()
	{
		if(Chat.Instance != null && Chat.Instance.canvas.enabled)
		{
			Chat.Instance.canvas.enabled = false;
			QuitBtn.SetActive(false);
		}

		if(local_PC != null)
		{
			local_PC.can_move = true;
		}
		else
		{
			//Debug.LogError("Local PlayerControl script is null.");
			//NetworkManager.singleton.StopClient();//temporary fix
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
									   AI_spawn.position,
									   Quaternion.identity);
				AIObject.GetComponent<AIControl>().team = 2;
				NetworkServer.Spawn(AIObject);
			}

			//timer
			StopCoroutine("MatchTimer");
			StartCoroutine("MatchTimer");
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
		yield return new WaitForSecondsRealtime(1.25f);

		if(reset) yield break;
		reset = true;
		timer = false;
		
		end_txt.gameObject.SetActive(true);

		if(t1_score > t2_score)
		{
			end_txt.text = local_PC.team == 1 ? "VICTORY" : "DEFEAT";
			end_txt.color = local_PC.team == 1 ? clr_MatchTime[1] : clr_MatchTime[2];
			audioS_WL.volume = local_PC.team == 1 ? 0.4f : 1f;
			audioS_WL.clip = local_PC.team == 1 ? aClip_win : aClip_lose;
			audioS_WL.Play();

			if(isServer && AIObject != null) AIObject.GetComponent<AIControl>().EndEmote(false);
			
			if(local_PC != null) local_PC.Cmd_EndEmote(1);
		}
		else if(t2_score > t1_score)
		{
			end_txt.text = local_PC.team == 2 ? "VICTORY" : "DEFEAT";
			end_txt.color = local_PC.team == 1 ? clr_MatchTime[1] : clr_MatchTime[2];
			audioS_WL.volume = local_PC.team == 2 ? 0.4f : 1f;
			audioS_WL.clip = local_PC.team == 2 ? aClip_win : aClip_lose;
			audioS_WL.Play();

			if(isServer && AIObject != null) AIObject.GetComponent<AIControl>().EndEmote(true);

			if(local_PC != null) local_PC.Cmd_EndEmote(2);
		}
		else//tie
		{
			end_txt.text = "TIE";
			end_txt.color = clr_MatchTime[0];
			audioS_WL.volume = 1f;
			audioS_WL.clip = aClip_lose;
			audioS_WL.Play();

			if(isServer) AIObject.GetComponent<AIControl>().EndEmote(false);
			
			if(local_PC != null) local_PC.Cmd_EndEmote(3);
		}

		yield return new WaitForSecondsRealtime(3);
		
		end_txt.gameObject.SetActive(false);

		if(Chat.Instance != null && !Chat.Instance.canvas.enabled)
			Chat.Instance.canvas.enabled = true;

		if(local_PC != null)
		{
			local_PC.can_move = false;
			local_PC.transform.position = local_PC.spawn_pos;
			local_PC.transform.rotation = local_PC.spawn_rot;
			local_PC.rigid.velocity = Vector3.zero;
		}

		if(isServer)
		{
			ready = false;

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
				AIObject.GetComponent<AIControl>().StopAllCoroutines();
				Destroy(AIObject);
			}

			Rpc_TimerUpdate(match_time);
		}
		reset = false;
		yield break;
	}
	
	private IEnumerator MatchTimer()
	{
		timer = true;
		float time = 0;
		
		while(timer)
		{
			yield return new WaitForSeconds(1f);
			time += 1;
			
			if(time >= match_time)
			{
				StartCoroutine("ResetGame");
				timer = false;
			}
			else if(time == match_time - 60)
				audioS_1min.Play();
			
			Rpc_TimerUpdate(match_time - time);
		}
		
		yield break;
	}
	[ClientRpc]
	void Rpc_TimerUpdate(float time)
	{
		txt_MatchTime.text = game_time + time.ToString();

		if(t1_score > t2_score)
			txt_MatchTime.color = clr_MatchTime[1];
		else if(t2_score > t1_score)
			txt_MatchTime.color = clr_MatchTime[2];
		else
			txt_MatchTime.color = clr_MatchTime[0];
	}

	public void SetListenerParent(Transform transf)
	{
		_transf_listener.SetParent(transf);
		_transf_listener.position = transf.position;
	}
}
