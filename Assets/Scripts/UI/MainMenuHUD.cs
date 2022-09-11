using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenuHUD : MonoBehaviour
{
    [SerializeField]
    private GameObject[] ConnectObjs, MainObjs, ModelObjs;

    [SerializeField]
    private Button btn_online, btn_host, btn_server, btn_client, btn_quit,
				   btn_back_online, btn_avatar_left, btn_avatar_right;
	/*[SerializeField]
	private Slider sld_avatar;*/
    [SerializeField]
    private InputField iField_address, iField_username;

    private int curr_avatar, total_avatar = 4;

    private void Start()
    {
        if(NetworkManager.singleton.networkAddress != "localhost")
            iField_address.text = NetworkManager.singleton.networkAddress;
        else iField_address.text = "localhost";

		if(iField_username.text == null) iField_username.text = "username";

        //invokes OnInputFieldChanged() whenever iField_address is changed
        iField_address.onValueChanged.AddListener(delegate
        {
            OnInputFieldChanged();
        });
		iField_username.onValueChanged.AddListener(delegate
		{
			OnUsernameChanged();
		});
		
		/*sld_avatar.onValueChanged.AddListener(delegate
		{
			OnAvatarChanged(1);
		});*/

        curr_avatar = 0;
        total_avatar--;
        ChangeModel(curr_avatar);

        btn_online.onClick.AddListener(ButtonOnline);
			btn_back_online.onClick.AddListener(ButtonOnline);
        btn_host.onClick.AddListener(ButtonHost);
        btn_server.onClick.AddListener(ButtonServer);
        btn_client.onClick.AddListener(ButtonClient);
        btn_quit.onClick.AddListener(ButtonQuit);
        btn_avatar_left.onClick.AddListener(delegate
        {
            OnAvatarChanged(-1);
        });
        btn_avatar_right.onClick.AddListener(delegate
        {
            OnAvatarChanged(1);
        });

        
        foreach(GameObject objC in ConnectObjs)
            objC.SetActive(false);
        //ConnectPanel.SetActive(false);
    }
    /*
	public void OnAvatarChanged()
	{
		NetworkClient.avatar = (int)sld_avatar.value;
	}
    */
    public void OnAvatarChanged(int dir)
    {
        if(dir < 0 && curr_avatar == 0)
            curr_avatar = total_avatar;
        else if(dir > 0 && curr_avatar >= total_avatar)
            curr_avatar = 0;
        else
            curr_avatar += dir;
        
        NetworkClient.avatar = curr_avatar;

        ChangeModel(curr_avatar);
    }
    private void ChangeModel(int avtr)
    {
        for(int i = 0; i < ModelObjs.Length; i++)
        {
            ModelObjs[i].SetActive(i == avtr);
        }
    }


    public void OnInputFieldChanged()
    {
        NetworkManager.singleton.networkAddress = iField_address.text;
    }
	public void OnUsernameChanged()
	{
		StaticVars.username = iField_username.text;
		print(StaticVars.username);
	}

    public void ButtonOnline()
    {
        foreach(GameObject objC in ConnectObjs)
            objC.SetActive(!objC.activeSelf);
        foreach(GameObject objM in MainObjs)
            objM.SetActive(!objM.activeSelf);
        //ConnectPanel.SetActive(!ConnectPanel.activeSelf);
        //MainPanel.SetActive(!MainPanel.activeSelf);
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
