using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerName : NetworkBehaviour
{
	private PlayerControl PC;
	
	[SerializeField]
    private TextMesh txt_nickname;
    [SerializeField]
    private GameObject FloatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string nickname;

    //fazer baseado no time
    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;
	[SerializeField]
	private Color team1Color, team2Color;
	
    private void OnNameChanged(string _Old, string _New)
    {
        txt_nickname.text = _New;
    }
    private void OnColorChanged(Color _Old, Color _New)
    {
        txt_nickname.color = _New;
    }
    
    public override void OnStartLocalPlayer()
    {
        /*
        Camera.main.transform.SetParent(transform);
        Camera.main.transform.localPosition = new Vector3(0, 0, 0);
        *//*    
        /*
        floatingInfo.transform.localPosition = new Vector3(0, -0.3f, 0.6f);
        floatingInfo.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        */
        
		PC = GetComponent<PlayerControl>();
		
        string name = (StaticVars.username != null ? StaticVars.username : "Human Being");
        Color color = (PC.team == 1 ? team1Color : team2Color);
		
        CmdSetupPlayer(name, color);
    }
    
    [Command]
    public void CmdSetupPlayer(string _name, Color _col)
    {
        nickname = _name;
        playerColor = _col;
    }

	private void Update()
	{
		FloatingInfo.transform.LookAt(Camera.main.transform);
	}
    /*
    void Update()
    {
        if (!isLocalPlayer)
        {
            // make non-local players run this
            floatingInfo.transform.LookAt(Camera.main.transform);
            return;
        }

        float moveX = Input.GetAxis("Horizontal") * Time.deltaTime * 110.0f;
        float moveZ = Input.GetAxis("Vertical") * Time.deltaTime * 4f;

        transform.Rotate(0, moveX, 0);
        transform.Translate(0, 0, moveZ);
    }
    */
}
