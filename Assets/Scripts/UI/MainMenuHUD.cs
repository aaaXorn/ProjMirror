using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenuHUD : MonoBehaviour
{
    [SerializeField]
    private GameObject ConnectPanel, MainPanel;

    [SerializeField]
    private Button btn_online, btn_host, btn_server, btn_client, btn_quit;

    [SerializeField]
    private InputField iField_address;

    [SerializeField]
    private Text txt_server, txt_client;

    private void Start()
    {
        if(NetworkManager.singleton.networkAddress != "localhost")
            iField_address.text = NetworkManager.singleton.networkAddress;
        else iField_address.text = "localhost";


        //invokes OnInputFieldChanged() whenever iField_address is changed
        iField_address.onValueChanged.AddListener(delegate
        {
            OnInputFieldChanged();
        });

        btn_online.onClick.AddListener(ButtonOnline);
        btn_host.onClick.AddListener(ButtonHost);
        btn_server.onClick.AddListener(ButtonServer);
        btn_client.onClick.AddListener(ButtonClient);
        btn_quit.onClick.AddListener(ButtonQuit);
        ConnectPanel.SetActive(false);
    }

    public void OnInputFieldChanged()
    {
        NetworkManager.singleton.networkAddress = iField_address.text;
    }

    public void ButtonOnline()
    {
        ConnectPanel.SetActive(!ConnectPanel.activeSelf);
        MainPanel.SetActive(!MainPanel.activeSelf);
    }
    public void ButtonHost()
    {
        NetworkManager.singleton.StartHost();
    }
    public void ButtonServer()
    {
        NetworkManager.singleton.StartServer();
    }
    public void ButtonClient()
    {
        NetworkManager.singleton.StartClient();
    }
    public void ButtonQuit()
    {
        Application.Quit();
    }

    /*public void SetupCanvas()
    {
        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (NetworkClient.active)
            {
                ConnectPanel.SetActive(false);
                StatusPanel.SetActive(true);
                txt_client.text = "Connecting to " + NetworkManager.singleton.networkAddress + "..";
            }
            else
            {
                ConnectPanel.SetActive(true);
                StatusPanel.SetActive(false);
            }
        }
        else
        {
            ConnectPanel.SetActive(false);
            StatusPanel.SetActive(true);

            if (NetworkServer.active)
            {
                txt_server.text = "Server: active. Transport: " + Transport.activeTransport;
            }
            if (NetworkClient.isConnected)
            {
                txt_client.text = "Client: address=" + NetworkManager.singleton.networkAddress;
            }
        }
    }*/
}
