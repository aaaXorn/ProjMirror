using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MainAudio : MonoBehaviour
{
    void Start()
    {
        AudioListener.volume = Save.GetVMain();
    }
}
