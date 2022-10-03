using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuOptions : MonoBehaviour
{
    [SerializeField] private AudioMixer aMix;

    [SerializeField] Slider sld_graphics, sld_master, sld_music, sld_sfx;
    
    public void StartOptions()
    {
        //player prefs get
    }

    public void Graphics(int vl)
    {
        QualitySettings.SetQualityLevel(vl);
        StaticVars.graphics = vl;
        Save.SetGraphics();
    }

    private float ValueToVolume(float vl)
    {
        float value = Mathf.Clamp(vl, 0.0001f, 1f);
        value = Mathf.Log10(value) * 20;
        return value;
    }
    public void Master(float vl)
    {
        StaticVars.volume_master = vl;
        Save.SetVMaster();

        //aMix.SetFloat("", ValueToVolume(vl));
    }
    public void Music(float vl)
    {
        StaticVars.volume_music = vl;
        Save.SetVMusic();

        aMix.SetFloat("Music", ValueToVolume(vl));
    }
    public void Sfx(float vl)
    {
        StaticVars.volume_sfx = vl;
        Save.SetVSfx();

        aMix.SetFloat("SFX", ValueToVolume(vl));
    }
}
