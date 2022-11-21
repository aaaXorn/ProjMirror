using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionColor : MonoBehaviour
{
    [SerializeField] ParticleSystem[] _particles;

    [SerializeField] Color _color;

    public void ChangeParticles(int team)
    {
        if(team == 2)
        {
            foreach(ParticleSystem ps in _particles)
            {
                ParticleSystem.MainModule settings = ps.main;
                settings.startColor = new ParticleSystem.MinMaxGradient( _color );
            }
        }
    }
}
