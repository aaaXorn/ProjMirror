using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H2P : MonoBehaviour
{
    int _curr_opt, _total_opt;
    [SerializeField]
    GameObject[] _ObjText, _ObjModel;

    void Start()
    {
        _total_opt = _ObjModel.Length;
    }

    public void ChangeModel(bool next)
    {
        int dir = next ? 1 : -1;
        int prev_opt = _curr_opt;

        _curr_opt += dir;
        if(_curr_opt >= _total_opt)
            _curr_opt = 0;
        else if(_curr_opt < 0)
            _curr_opt = _total_opt - 1;
        
        _ObjText[prev_opt].SetActive(false);
        _ObjModel[prev_opt].SetActive(false);
        _ObjText[_curr_opt].SetActive(true);
        _ObjModel[_curr_opt].SetActive(true);
    }
}
