using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class PlayerName : NetworkBehaviour
{
    [SerializeField]
    private TextMesh txt_nickname;
    [SerializeField]
    private GameObject FloatingInfo;

    [SyncVar(hook = nameof(OnNameChanged))]
    public string nickname;

    //fazer baseado no time
    [SyncVar(hook = nameof(OnColorChanged))]
    public Color playerColor = Color.white;

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
        
        string name = "Player" + Random.Range(100, 999);
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        CmdSetupPlayer(name, color);
    }
    
    [Command]
    public void CmdSetupPlayer(string _name, Color _col)
    {
        nickname = _name;
        playerColor = _col;
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
