using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AIName : NetworkBehaviour
{
    private AIControl AIC;
	
	[SerializeField]
    private TextMesh txt_nickname;
    [SerializeField]
    private GameObject FloatingInfo;
	
	[SyncVar(hook = nameof(OnNameChanged))]
    public string nickname;
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
	
	public override void OnStartServer()
	{
		base.OnStartServer();
		
		AIC = GetComponent<AIControl>();
		
		nickname = "CPU";
        playerColor = (AIC.team == 1 ? team1Color : team2Color);
	}
	
	private void Update()
	{
		FloatingInfo.transform.LookAt(Camera.main.transform);
	}
}
