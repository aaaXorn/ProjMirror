using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Chat : NetworkBehaviour
{
	public static Chat Instance {get; private set;}
	
	[HideInInspector]
	public PlayerName PN;
	
	[SerializeField] private GameObject ChatBox;
    [SerializeField] private Text txt;
	[SerializeField] private InputField iField;
	
	private static event Action<string> OnMessage;
	
	private void Awake()
	{
        if(Instance == null) Instance = this;
		else
		{
			Destroy(Instance);
			Instance = this;
		}
	}
	
	public override void OnStartAuthority()
	{
		ChatBox.SetActive(true);
		
		OnMessage += HandleNewMessage;
	}
	
	[ClientCallback]
	private void OnDestroy()
	{
		if(!hasAuthority) return;
		
		OnMessage -= HandleNewMessage;
	}
	
	private void HandleNewMessage(string msg)
	{
		string s = PN != null ? PN.nickname : "meucu";
		txt.text += s + ": "+ msg;
	}
	
	[Client]
	public void Send(string msg)
	{
		if(!Input.GetKeyDown(KeyCode.Return)) return;
		if(string.IsNullOrWhiteSpace(msg)) return;
		
		Cmd_SendMessage(iField.text);
		
		iField.text = string.Empty;
	}
	
	[Command]
	private void Cmd_SendMessage(string msg)
	{
		Rpc_HandleMessage($"[{connectionToClient.connectionId}]: {msg}");
	}
	
	[ClientRpc]
	private void Rpc_HandleMessage(string msg)
	{
		OnMessage?.Invoke($"\n{msg}");
	}
}
