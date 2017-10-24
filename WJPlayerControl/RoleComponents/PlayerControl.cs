using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControl : MonoBehaviour
{
    public float deltaTime;

    public GroundDetect groundDct;

    public FallProcess fallProc;

    public SlideProcess slideProc;

    public MoveProcess moveProc;

    public JumpProcess jumpProc;

    public CliffDetect cliffDct;

    //角色状态
    public RoleState state;

    Vector3 ColliderSize;
    public bool isFloating;

    public float maxFrictionSlope = 0.5f;

    //传送门的速度大小
    public float transforDoorSpeed = 5;

    public void Init()
    {
        ColliderSize = transform.Find("body").Find("collider").lossyScale;

        state = RoleState.Falling;

        groundDct = GetComponent<GroundDetect>();

        jumpProc = GetComponent<JumpProcess>();
        jumpProc.Init();

        fallProc = GetComponent<FallProcess>();

        slideProc = GetComponent<SlideProcess>();

        moveProc = GetComponent<MoveProcess>();
        moveProc.init();

        cliffDct = GetComponent<CliffDetect>();
    }

    void BeforeUpdate()
    {
        deltaTime = Time.deltaTime;
        if (deltaTime > 0.06)
        {
            deltaTime = 0.06f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        BeforeUpdate();

        if (isFloating == false)
        {
            moveProc.UpdateByParent();

            jumpProc.UpdateByParent();
        }


        //处理既下落又滑落的情况。
        bool hasFall = false;
        bool hasSlide = false;
        Vector3 posBegin;
        posBegin = transform.position;
        //下落
        if (fallProc.isActiveAndEnabled && isFloating == false)
        {
            fallProc.UpdateByParent();
            if (transform.position != posBegin)
            {
                hasFall = true;
            }
        }
        Vector3 posMid = transform.position;
        //滑落
        if (slideProc.isActiveAndEnabled)
        {
            slideProc.UpdateByParent();
            if (transform.position != posMid)
            {
                hasSlide = true;
            }
        }
        Vector3 posEnd = transform.position;
        if (hasFall && hasSlide && groundDct.IsOnIceground())
        {
            transform.position = Vector3.Lerp(posBegin, posEnd, 0.7f);
        }

        groundDct.UpdateByParent();
    }

    //输入力
    public void SetMove(float value)
    {
        //更新输入速度
        moveProc.horizontalInputSpeed = value;
    }

    //外部瞬间力（落地会消失，输入会消失）
    public void SetExternalMove(float value)
    {
        //重置为0（因为进传门时会禁用输入，保留进之前的水平input值，必须归0）
        moveProc.horizontalInputSpeed = 0;
        //设置惯性
        moveProc.lastFrameSpeedVector.x = value * transforDoorSpeed;
        //重置转向损耗
        moveProc.ResetTurnLoss();
    }

    public void Kill()
    {
        jumpProc.jumpInstantSpeed = 0;
        this.transform.parent = null;
        this.gameObject.SetActive(false);
    }
}
