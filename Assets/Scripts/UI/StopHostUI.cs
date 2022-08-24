using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class StopHostUI : MonoBehaviour
{
    [SerializeField]
    private Button btn_stopHost;

    private void Start()
    {
        btn_stopHost.onClick.AddListener(ButtonStopHost);
    }

    public void ButtonStopHost()
    {
        //if host, stop hosting
        if(NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        //if client-only, disconnect
        else if(NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        //if server-only, stop server
        else if(NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
    }
}
