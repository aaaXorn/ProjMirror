using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    //instance global reference
    public static Manager Instance { get; private set; }

    //material colors
    public Color[] mat;

    private void Awake()
    {
        //sets global reference
        if (Instance == null) Instance = this;
        //if there's  already an instance, remove this
        else Destroy(gameObject);
    }
}
