using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H2P_Throw : MonoBehaviour
{
    float _max_time = 1f, _time = 0f;

    [SerializeField]
    Transform _target;
    Rigidbody _rigid;

    [SerializeField]
    float _force;

    Vector3 _startPos;

    void Start()
    {
        _startPos = _target.transform.position;
        _rigid = _target.GetComponent<Rigidbody>();

        Yeet();
    }

    void Update()
    {
        _time += Time.deltaTime;

        if(_time >= _max_time)
        {
            _time = 0;

            _target.position = _startPos;
            Yeet();
        }
    }

    void Yeet()
    {
        _rigid.velocity = Vector3.zero;

        Vector3 force = (transform.forward + Vector3.up * 0.5f).normalized * _force;
        _rigid.AddForce(force);
    }
}
