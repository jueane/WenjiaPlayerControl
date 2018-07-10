using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//悬崖检测
public class CliffDetect : MonoBehaviour
{
    public bool isDebug;
    PlayerControl cat;

    //检测深度
    public float deep = 3;

    //检测偏移
    public Vector3 offset = new Vector3(1, 2, 0);
    //是否存在悬崖
    public bool isCliff;

    //关注的层
    public int layer;

    public GameObject obj;

    Transform catTransform;

    //间隔
    public float maxWait = 2;
    float wait = 0;


    // Use this for initialization
    void Start()
    {
        cat = GetComponent<PlayerControl>();
        catTransform = cat.transform;
        layer = LayerMask.GetMask(LayerName.Ground, LayerName.Danger, LayerName.Platform);
    }

    // Update is called once per frame
    void Update()
    {
        wait += Time.deltaTime;
        if (maxWait > 1)
        {
            wait = 0;

            //执行检测
            DetectDown();
            if (isCliff)
            {
                DetectFront();
            }
        }
    }

    //检测前方
    void DetectFront()
    {
        Vector3 origin = catTransform.position + Vector3.up * offset.y;
        Vector3 direction;
        if (cat.moveProc.faceLeft)
        {
            direction = -Vector3.right;
        }
        else
        {
            direction = Vector3.right;
        }

        RaycastHit hit;

        bool isHit = Physics.Raycast(origin, direction, out hit, offset.x, layer);

        if (isDebug)
        {
            Debug.DrawRay(origin, direction * offset.x);
        }

        if (isHit)
        {
            obj = hit.collider.gameObject;
            isCliff = false;
        }
        else
        {
            obj = null;
            isCliff = true;
        }
    }

    //检测下方
    void DetectDown()
    {
        //检测
        Vector3 origin = catTransform.position + Vector3.up * offset.y;
        if (cat.moveProc.faceLeft)
        {
            origin -= Vector3.right * offset.x;
        }
        else
        {
            origin += Vector3.right * offset.x;
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
            obj = null;
            isCliff = true;
        }
    }
}
