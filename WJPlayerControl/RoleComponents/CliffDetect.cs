using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//悬崖检测
public class CliffDetect : MonoBehaviour
{
    public bool isDebug;
    PlayerControl cat;

    //检测深度
    public float deep = 1;

    //检测提前量距离
    public float front = 1;
    //是否存在悬崖
    public bool isCliff;

    //关注的层
    public int layer;

    public GameObject obj;

    // Use this for initialization
    void Start()
    {
        cat = GetComponent<PlayerControl>();
        layer = LayerMask.GetMask(LayerName.ground, LayerName.Danger, LayerName.Platform);
    }

    // Update is called once per frame
    void Update()
    {
        //检测
        Vector3 origin = transform.position;
        if (cat.moveProc.faceLeft)
        {
            origin -= Vector3.right * front;
        }
        else
        {
            origin += Vector3.right * front;
        }

        RaycastHit hit;

        bool isHit = Physics.Raycast(origin, Vector2.down, out hit, deep, layer);

        if (isDebug)
        {
            Debug.DrawRay(origin, Vector2.down * deep);
        }

        if (isHit)
        {
            obj = hit.collider.gameObject;
            isCliff = false;
        }
        else
        {
            isCliff = true;
        }

    }
}
