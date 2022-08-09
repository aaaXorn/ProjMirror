using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test : MonoBehaviour
{
    [SerializeField]
    private string sceneName;

    private void Start()
    {
        Invoke("Load", 3f);
    }

    private void Load()
    {
        SceneManager.LoadScene(sceneName);
    }
}
