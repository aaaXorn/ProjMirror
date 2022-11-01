using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThrowIndicator : MonoBehaviour
{
    public static ThrowIndicator Instance;

    public Transform target;

    float pos_y;

    [SerializeField] Image img;
    [SerializeField] Color[] colors;
    
    void Awake()
    {
        Instance = this;

        pos_y = transform.position.y;
    }

    void Update()
    {
        if(target != null)
        {
            SetPos(target.position);
            //transform.rotation = target.rotation;
        }
    }

    public void SetPos(Vector3 pos)
    {
        Vector3 go_to = new Vector3(pos.x, pos_y, pos.z);
        transform.position = go_to;
    }

    public void SetColor(bool red)
    {
        int team = red ? 0 : 1;
        img.color = colors[team];
    }
}
