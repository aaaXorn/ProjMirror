using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class Chat : NetworkBehaviour
{
	public static Chat Instance {get; private set;}
	
	[SerializeField] ScrollRect sRect;
	
	[HideInInspector]
	public PlayerName PN;
	[HideInInspector]
	public PlayerControl PC;
	
	[SerializeField] private GameObject ChatBox;
    [SerializeField] private TMP_Text txt;
	[SerializeField] private InputField iField;
	
	[HideInInspector] public Canvas canvas;

	[SerializeField] Color[] name_color;

	private void Awake()
	{
        if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	
	private void Start()
	{
		canvas = GetComponent<Canvas>();
	}

	/*public override void OnStartAuthority()
	{
		canvas = GetComponent<Canvas>();
		ChatBox.SetActive(true);
	}*/
	
	private void HandleNewMessage(string msg, string name, int team)
	{
		//if(txt.isTextOverflowing) txt.text = "";

		string s = name != null ? name : "debugmeucu";
		string clr = "#" + ColorUtility.ToHtmlStringRGBA(name_color[PC.team-1]);
		string clr_base = "#" + ColorUtility.ToHtmlStringRGBA(name_color[2]);
		txt.text += "\n" + "<color=" + clr + ">" + s + ": "+ "</color>" + "<color=" + clr_base + ">" + msg + "</color>";
		Canvas.ForceUpdateCanvases();
		sRect.verticalNormalizedPosition = 0f;
	}
	
	void Update()
	{
		if(Input.GetButtonDown("Cancel")) ChangeCanvasEnabled();
	}

	public void ChangeCanvasEnabled()
	{
		canvas.enabled = !canvas.enabled;
	}

	[Client]
	public void Send(string msg)
	{
		if(!Input.GetKeyDown(KeyCode.Return)) return;
		if(string.IsNullOrWhiteSpace(msg)) msg = iField.text;
		
		if(!isServer) Cmd_SendMessage(iField.text, PN.nickname, PC.team); else SrvSendMessage(iField.text, PN.nickname, PC.team);
		
		iField.text = string.Empty;
	}
	
	[Command(requiresAuthority = false)]
	private void Cmd_SendMessage(string msg, string name, int team)
	{
		Rpc_HandleMessage(msg, name, team);
	}

	private void SrvSendMessage(string msg, string name, int team)
	{
		Rpc_HandleMessage(msg, name, team);
	}
	
	[ClientRpc]
	private void Rpc_HandleMessage(string msg, string name, int team)
	{
		HandleNewMessage(msg, name, team);
	}
}
