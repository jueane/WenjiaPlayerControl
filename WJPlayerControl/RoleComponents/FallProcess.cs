using UnityEngine;
using System.Collections;

public class FallProcess : MonoBehaviour
{
    //可以忽略的[与地面的间隙]。在这个距离即视为在地面。
    public static float ignoreDisGround = 0.01f;

    PlayerControl role;

    GroundDetect groundDct;

    //下落速度
    public float fallSpeed = 0;
    //下落加速度
    public float fallAccelerateSpeed = 25;
    //最大下落速度
    public float maxFallSpeed = 18;

    // Use this for initialization
    void Start()
    {
        role = GetComponent<PlayerControl>();
        groundDct = GetComponent<GroundDetect>();
    }

    public void UpdateByParent()
    {
        if (isActiveAndEnabled == false)
        {
            return;
        }

        //下落前检查并设置状态
        CheckBeforeFalling();

        Falling();
    }

    void CheckBeforeFalling()
    {
        //非上升阶段，处理状态
        if (role.state != RoleState.Raising)
        {
            //在地面以上
            if (groundDct.isClosedGround == false || (groundDct.isClosedGround && groundDct.disGround > 0))
            {
                //如果还是ground状态（从平台跌落。还可以跳2次。）
                if (role.state == RoleState.Grounded || role.groundDct.IsOnIceground())
                {
                    //记录滑落方向
                    role.moveProc.lastSlideVector = role.slideProc.slideVector;

                    if (role.jumpProc.multijump)
                    {
                        role.jumpProc.remainJumpTimes = 2;
                    }
                    else
                    {
                        role.jumpProc.remainJumpTimes = 1;
                    }
                }
                role.state = RoleState.Falling;
            }
        }
    }

    void Falling()
    {
        float disToGround = groundDct.actualDisGround;
        //在地面的情况时候，Y轴位置偏差微调。
        if (role.state == RoleState.Grounded)
        {
            if (groundDct.isClosedGround && disToGround < 0.1f)
            {
                if (disToGround < 0)
                {
                    transform.position += Vector3.down * disToGround;
                }
                else
                {
                    transform.position += Vector3.down * disToGround;
                }
            }
        }
        //下落。
        if (role.state == RoleState.Falling)
        {
            //当前帧预计下落距离
            float fallDis = fallSpeed * role.deltaTime;
            //正常下落
            if (groundDct.isClosedGround == false || (groundDct.isClosedGround && disToGround >= fallDis))
            {
                transform.position += Vector3.down * fallDis;

                //如果在sliding状态，则设置一个较小的fall速度;
                if (role.slideProc.isSliding)
                {
                    fallSpeed = 4;
                }

                //加速度
                if (fallSpeed < maxFallSpeed)
                {
                    fallSpeed += fallAccelerateSpeed * role.deltaTime;
                }
            }
            //下落距离不足，直接落地。
            if (groundDct.isClosedGround && disToGround < fallDis)
            {
                if (disToGround < 0)
                {
                    transform.position += Vector3.down * (disToGround);
                }
                else if (disToGround > 0)
                {
                    transform.position += Vector3.down * (disToGround);
                }
            }


        }
    }

}
