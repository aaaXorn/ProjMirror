using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class MainMenuHUD : MonoBehaviour
{
    [SerializeField]
    private GameObject[] ConnectObjs, MainObjs, ModelObjs;
	[SerializeField] GameObject _obj_H2P, _obj_opt;

    [SerializeField]
    private Button btn_online, btn_host, btn_server, btn_client, btn_quit,
				   btn_back_online, btn_avatar_left, btn_avatar_right, btn_h2p;
	/*[SerializeField]
	private Slider sld_avatar;*/
    [SerializeField]
    private InputField iField_address, iField_username;

    private int curr_avatar, total_avatar = 4;

    MenuOptions menu_opt;

    private void Start()
    {
        menu_opt = GetComponent<MenuOptions>();
        menu_opt.StartOptions();

        if(OnlineManager.Instance.networkAddress != "localhost")
            iField_address.text = OnlineManager.Instance.networkAddress;
        else iField_address.text = "localhost";

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
		//btn_h2p.onClick.AddListener(ButtonH2P);
        
        foreach(GameObject objC in ConnectObjs)
            objC.SetActive(false);
        _obj_H2P.SetActive(false);
        _obj_opt.SetActive(false);
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
        OnlineManager.Instance.networkAddress = iField_address.text;
        print(OnlineManager.Instance.networkAddress);
    }
	public void OnUsernameChanged()
	{
		StaticVars.username = iField_username.text;
	}

    public void ButtonOnline()
    {
        foreach(GameObject objC in ConnectObjs)
            objC.SetActive(!objC.activeSelf);
        foreach(GameObject objM in MainObjs)
            objM.SetActive(!objM.activeSelf);
		if(_obj_H2P.activeSelf) _obj_H2P.SetActive(false);
        //ConnectPanel.SetActive(!ConnectPanel.activeSelf);
        //MainPanel.SetActive(!MainPanel.activeSelf);
    }
    public void ButtonHost()
    {
        OnlineManager.Instance.StartHost();
    }
    public void ButtonServer()
    {
        OnlineManager.Instance.StartServer();
    }
    public void ButtonClient()
    {
        print("hud client");
        OnlineManager.Instance.StartClient();
    }
    public void ButtonQuit()
    {
        Application.Quit();
    }
	public void ButtonH2P()
	{
		_obj_H2P.SetActive(!_obj_H2P.activeSelf);
		
		foreach(GameObject objM in MainObjs)
		{
			objM.SetActive(!objM.activeSelf);
		}
	}
    public void ButtonOptions()
	{
		_obj_opt.SetActive(!_obj_opt.activeSelf);
		
		foreach(GameObject objM in MainObjs)
		{
			objM.SetActive(!objM.activeSelf);
		}
	}
}
