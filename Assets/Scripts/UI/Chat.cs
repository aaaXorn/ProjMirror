using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Chat : NetworkBehaviour
{
	[SerializeField] private GameObject ChatBox;
    [SerializeField] private Text txt;
	
	private static event Action<string> OnMessage;
	
	public override void OnStartAuthority()
	{
		ChatBox.SetActive(true);
		
		//OnMessage += HandleNewMessage(msg);
	}
	
	[ClientCallback]
	private void OnDestroy()
	{
		if(!hasAuthority) return;
		
		OnMessage -= HandleNewMessage;
	}
	
	private void HandleNewMessage(string msg)
	{
		//txt.text += "\n" + + ": "+ msg;
	}
	
	[Client]
	public void Send(string msg)
	{
		
	}
}
