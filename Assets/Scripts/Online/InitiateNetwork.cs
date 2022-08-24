using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitiateNetwork : MonoBehaviour
{
    [Tooltip("NetworkManager Prefab")]
    [SerializeField]
    private GameObject P_NetworkManager;

    //static instance, used to stay persistent between scenes
    private static GameObject NetworkManager;
    
    private void Awake()
    {
        //if this is the first time the scene is loaded, and no NetworkManager is in DontDestroyOnLoad
        if(NetworkManager == null)
        {
            //initializes the NetworkManager
            NetworkManager = Instantiate(P_NetworkManager);
        }

        //destroys this object, since it's no longer useful
        Destroy(gameObject);
    }
}
