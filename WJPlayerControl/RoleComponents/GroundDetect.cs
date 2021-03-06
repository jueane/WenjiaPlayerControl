﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundDetect : MonoBehaviour
{
    public bool isDebug;

    //打线起点
    Vector3 origin;

    //打线起点与中心点的偏移
    float offsetY = 0.2f;

    //检测线长度（腿的长度）
    float legLength = 1.4f;

    PlayerControl role;

    Transform body;

    RaycastHit hitLeft;
    RaycastHit hitRight;

    public bool isHitLeft;
    public bool isHitRight;

    Vector3 pL;
    Vector3 pR;

    //检测到的地面向量
    public Vector3 footVector;

    public Vector3 midNormal;

    public float slopeLeft;
    public float slopeRight;

    public float slope;

    public bool isClosedGround;
    //与地面的距离
    public float actualDisGround;
    public float disGround;

    public bool isHitIce;

    //在地面的累积时间
    public float timeStand = 0;
    public float minTimeStand = 0.1f;

    //旋转调整
    public bool processBalance;
    //落地旋转速度
    public float rotationSpeedFalling = 8;
    //上升旋转速度
    public float rotationSpeedRaising = 15;

    // Use this for initialization
    void Start()
    {
        body = transform.Find("body");
        role = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    void Update()
    {
        BalanceAdjust();
    }

    public void UpdateByParent()
    {
        DetectingGround();

        CalculateDisGround();

        //设置站立了多久
        if (disGround == 0)
        {
            timeStand += Time.deltaTime;
        }
        else
        {
            timeStand = 0;
        }
    }

    void DetectingGround()
    {
        origin = transform.position + new Vector3(0, offsetY, 0);

        //初始化数值
        footVector = Vector3.zero;
        midNormal = Vector3.zero;
        slopeLeft = 0;
        slopeRight = 0;
        slope = 0;
        actualDisGround = 0;
        disGround = 0;

        //Vector3 size = new Vector3(0.002f, 0.2f, 0);

        //isHitLeft = Physics.BoxCast(transform.position + new Vector3(-0.3f, 0.2f, 0), size / 2, Vector3.down, out hitLeft, Quaternion.identity, 1.2f, LayerMask.GetMask(LayerName.ground), QueryTriggerInteraction.Collide);

        //isHitRight = Physics.BoxCast(transform.position + new Vector3(0.3f, 0.2f, 0), size / 2, Vector3.down, out hitRight, Quaternion.identity, 1.2f, LayerMask.GetMask(LayerName.ground), QueryTriggerInteraction.Collide);


        Vector3 originLeft = origin + new Vector3(-0.3f, 0, 0);
        Vector3 originRight = origin + new Vector3(0.3f, 0, 0);


        isHitLeft = SingleFootDetect(originLeft, out hitLeft);
        isHitRight = SingleFootDetect(originRight, out hitRight);

        //isHitLeft = Physics.Raycast(originLeft, Vector3.down, out hitLeft, legLength, LayerMask.GetMask(LayerName.ground, LayerName.Platform), QueryTriggerInteraction.Collide);

        //isHitRight = Physics.Raycast(originRight, Vector3.down, out hitRight, legLength, LayerMask.GetMask(LayerName.ground, LayerName.Platform), QueryTriggerInteraction.Collide);



        float a, b;
        a = hitLeft.normal.x;
        b = hitRight.normal.x;

        if (a < 0 && b > 0)
        {
            float absA = Mathf.Abs(a);
            float absB = Mathf.Abs(b);

            if (absA < absB)
            {
                isHitRight = Physics.Raycast(origin, Vector3.down, out hitRight, legLength, LayerMask.GetMask(LayerName.Ground, LayerName.Platform), QueryTriggerInteraction.Collide);
            }
            if (absA > absB)
            {
                isHitLeft = Physics.Raycast(origin, Vector3.down, out hitLeft, legLength, LayerMask.GetMask(LayerName.Ground, LayerName.Platform), QueryTriggerInteraction.Collide);
            }

        }

        //Debug.DrawRay(origin + new Vector3(0.3f, 0, 0), Vector3.down, Color.red);


        pL = hitLeft.point;
        pR = hitRight.point;

        pL.z = origin.z;
        pR.z = origin.z;

        //是否命中冰面
        isHitIce = (isHitLeft && TagName.IceGround.Equals(hitLeft.transform.tag)) || (isHitRight && TagName.IceGround.Equals(hitRight.transform.tag));

        //移动方向
        if (isHitLeft && isHitRight)
        {
            Debug.DrawLine(pL, pR, Color.cyan);
            //检测到的与地面平行的向量
            footVector = pR - pL;
            midNormal = new Vector3(-footVector.y, footVector.x, 0);
        }

        //一个命中，一个未命中
        if (isHitLeft != isHitRight)
        {
            if (isHitLeft)
            {
                //单脚命中的正常情况。（命中侧水平或向上倾斜）
                footVector = new Vector3(hitLeft.normal.y, -hitLeft.normal.x, 0);
                Debug.DrawRay(pL, footVector, Color.red);
                midNormal = hitLeft.normal;
            }
            else if (isHitRight)
            {
                //单脚命中的正常情况。（命中侧水平或向上倾斜）
                footVector = new Vector3(hitRight.normal.y, -hitRight.normal.x, 0);
                Debug.DrawRay(pR, footVector, Color.cyan);
                midNormal = hitRight.normal;


                ////单侧命中，且命中一侧的角度向下倾斜，则另一侧算做最长命中。
                //if (hitRight.normal.x > 0)
                //{
                //}
                //else
                //{
                //    //单脚命中的正常情况。（命中侧水平或向上倾斜）
                //    groundVector = new Vector3(hitRight.normal.y, -hitRight.normal.x, 0);
                //    Debug.DrawRay(pR, groundVector, Color.cyan);
                //    midNormal = hitRight.normal;
                //}

            }
        }
        //全未命中
        if (isHitLeft == false && isHitRight == false)
        {
            footVector = Vector3.right;
            midNormal = Vector3.up;
            isClosedGround = false;
        }
        else
        {
            isClosedGround = true;
        }
        footVector.Normalize();

        //检测到的地面方向
        //Debug.DrawRay(transform.position, groundVector * 5, Color.green);
        //Debug.DrawRay(transform.position, midNormal, Color.blue);

        //左右脚斜率
        slopeLeft = GetSlope(hitLeft.normal);
        slopeRight = GetSlope(hitRight.normal);

        //算出斜率
        slope = GetSlope(midNormal);

    }

    void CalculateDisGround()
    {
        if (isHitLeft && isHitRight)
        {
            Vector3 centerGround = (pL + pR) / 2;
            //Debug.DrawLine(origin, centerGround, Color.red);
            disGround = origin.y - centerGround.y;
        }
        else if (isHitLeft)
        {
            Vector3 vecL = pL - origin;
            float angle = Vector2.Angle(-midNormal, Vector3.down);
            Vector3 Ver = Vector3.Project(vecL, -midNormal);
            disGround = Ver.magnitude / Mathf.Cos(angle * Mathf.Deg2Rad);
        }
        else if (isHitRight)
        {
            Vector3 vecR = pR - origin;
            float angle = Vector2.Angle(-midNormal, Vector3.down);
            Vector3 Ver = Vector3.Project(vecR, -midNormal);
            disGround = Ver.magnitude / Mathf.Cos(angle * Mathf.Deg2Rad);
            //print("值:" + disGround + "," + Ver);
        }
        //至此已算出垂直距离。

        //减去自身尺寸，同时减去因倾斜造成的尺寸增加。
        disGround -= 0.5f + 0.1f * slope;

        actualDisGround = disGround;
        if (Mathf.Abs(disGround) < 0.02)
        {
            disGround = 0;
        }


        //平稳落于地面（在不可站立的斜面时，属于Falling状态）
        if (IsStandable() && isClosedGround && disGround == 0)
        {
            //事件通知：平稳落于地面
            if (role.state != RoleState.Grounded)
            {
                //print("站稳于地面");
                role.state = RoleState.Grounded;
                role.groundDct.OnStandGround();

                //设置二段跳
                if (role.jumpProc.multijump)
                {
                    role.jumpProc.remainJumpTimes = 1;
                }
                else
                {
                    role.jumpProc.remainJumpTimes = 0;
                }
            }
        }

    }

    void BalanceAdjust()
    {
        if (processBalance == false)
        {
            return;
        }

        float maxRotation = 0.7f;

        //旋转
        if (role.state != RoleState.Raising && role.isFloating == false && isHitLeft && isHitRight && slope < maxRotation && ((isHitLeft && slopeLeft < maxRotation) || (isHitRight && slopeRight < maxRotation)))
        {
            //慢慢旋转
            Vector3 to = Vector3.Lerp(body.right, footVector, rotationSpeedFalling * role.deltaTime);
            to = new Vector3(to.x, to.y, 0);
            body.right = to;

            //角色体向上的法线
            midNormal.x = -footVector.y;
            midNormal.y = footVector.x;
        }
        else
        {
            //恢复水平
            float beforeAngle = Vector3.Angle(body.right, Vector3.right);

            Vector3 to = Vector3.Lerp(body.right, Vector3.right, rotationSpeedRaising * role.deltaTime);
            to = new Vector3(to.x, to.y, 0);
            body.right = to;

            float afterAngle = Vector3.Angle(body.right, Vector3.right);
            if (beforeAngle - afterAngle > 0 && Mathf.Abs(beforeAngle - afterAngle) < 0.1f)
            {
                body.right = Vector3.right;
            }
        }

    }

    public bool IsOnGround()
    {
        if (isClosedGround && disGround == 0)
        {
            return true;
        }
        return false;
    }

    public bool IsOnIceground()
    {
        if (isHitIce && isClosedGround && disGround < 0.3f)
        {
            return true;
        }
        //if (isHitIce && IsOnGround())
        //{
        //    return true;
        //}
        if (isHitIce && role.state == RoleState.Falling && disGround < 0.3f)
        {
            return true;
        }
        return false;
    }

    //普通跳条件
    public bool IsStandable()
    {
        if (isHitLeft == false || isHitRight == false)
        {
            if (slope >= role.maxFrictionSlope)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        if (isHitLeft && isHitRight)
        {
            if (slope < role.maxFrictionSlope)
            {
                if (slopeLeft < role.maxFrictionSlope && slopeRight < role.maxFrictionSlope)
                {
                    return true;
                }


                if (slopeLeft >= role.maxFrictionSlope)
                {
                    if (slope > slopeRight + 0.1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                if (slopeRight >= role.maxFrictionSlope)
                {
                    if (slope > slopeLeft + 0.1)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //当站稳于地面时调一次此方法。
    public void OnStandGround()
    {
        role.moveProc.ResetInertia();
        role.moveProc.ResetTurnLoss();

        //重置下落速度
        role.fallProc.fallSpeed = 0;
    }

    bool SingleFootDetect(Vector3 originPos, out RaycastHit hitinfo)
    {
        int instLayer = LayerMask.GetMask(LayerName.Ground, LayerName.Platform);
        float width = 0.1f / 5;

        List<RaycastHit> hitList = new List<RaycastHit>();

        int footWidth = 5;

        for (int i = -footWidth; i <= footWidth; i++)
        {
            RaycastHit hitTemp;
            bool isHitTemp = Physics.Raycast(originPos + new Vector3(i * width, 0, 0), Vector3.down, out hitTemp, legLength, instLayer, QueryTriggerInteraction.Collide);
            if (isDebug)
            {
                Debug.DrawRay(originPos + new Vector3(i * width, 0, 0), Vector3.down * legLength);
            }

            if (isHitTemp)
            {
                hitList.Add(hitTemp);
            }
        }

        if (hitList.Count == 0)
        {
            //未命中
            hitinfo = new RaycastHit();
            return false;
        }
        else
        {
            //命中
            RaycastHit hitTemp = hitList[0];
            for (int i = 0; i < hitList.Count - 1; i++)
            {
                if (hitList[i].distance > hitList[i + 1].distance)
                {
                    hitTemp = hitList[i + 1];
                }
            }

            hitinfo = hitTemp;
            return true;
        }

    }

    float GetSlope(Vector3 vec)
    {
        float angle = Vector3.Angle(vec, Vector3.up);
        if (vec.x == 0)
        {
            angle = 0;
        }
        float slopeT = angle / 90;
        return slopeT;
    }
}
