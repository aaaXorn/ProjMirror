using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuOptions : MonoBehaviour
{
    [SerializeField] private AudioMixer aMix;

    [SerializeField] Slider sld_graphics, sld_master, sld_music, sld_sfx;
    
    void Start()
    {
        StartOptions();
    }

    public void StartOptions()
    {
        //player prefs get
        int g = (int)Save.GetGraphics();
        sld_graphics.value = g;
        Graphics(g);
        float v = Save.GetVMain();
        sld_master.value = v;
        Master(v);
        v = Save.GetVMusic();
        sld_music.value = v;
        Music(v);
        v = Save.GetVSfx();
        sld_sfx.value = v;
        Sfx(v);
    }

    public void Graphics(float vl)
    {
        QualitySettings.SetQualityLevel((int)vl);
        StaticVars.graphics = (int)vl;
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
