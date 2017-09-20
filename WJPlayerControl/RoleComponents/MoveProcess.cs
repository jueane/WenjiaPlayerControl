using UnityEngine;
using System.Collections;

public class MoveProcess : MonoBehaviour, GameManagerRoleListener
{
    PlayerControl role;
    Transform model;
    public MoveDetect movDct;

    public bool faceLeft = false;

    //平移速度
    public float moveSpeed = 5;
    //水平输入速度
    public float horizontalInputSpeed = 0;
    //水平外部施加速度
    public float horizontalExternalSpeed = 0;

    //上一帧角色的移动方向
    public Vector3 lastFrameSpeedVector;
    //空中跳跃的水平惯性
    public float inertiaInput = 5f;
    //滑落的惯性
    public float inertiaSlide = 5f;

    //空中转向次数
    public int turnCount = 0;
    //空中转向每次损耗
    public float turnLoss = 0.3f;
    //在空中的累计时间
    public float StaySkyTime = 0;
    //惯性的阻力系数
    public float inertiaDrag = 5;

    public void init()
    {
        GameManager.Instance.AddRoleListener(this);

        role = GetComponent<PlayerControl>();
        movDct = GetComponent<MoveDetect>();

        model = transform.Find("body").Find("model");
    }

    public void UpdateByParent()
    {
        StaySkyTime += Time.deltaTime;
        movDct.UpdateByParent();
        CalculateMoving();
    }

    //帧结束时，设置惯性。
    void LateUpdate()
    {
        if (role.isFloating || GameManager.Instance.playerIsDead)
        {
            ResetInertia();
            return;
        }

        if (role.groundDct.IsOnGround())
        {
            lastFrameSpeedVector = Vector3.zero;
        }
        else if (role.groundDct.isClosedGround && role.groundDct.disGround <= 0.2f)
        {
            //要判断陷入地面的情况。
            lastFrameSpeedVector = Vector3.zero;
        }
        else if (role.groundDct.isClosedGround == false || (role.groundDct.isClosedGround && role.groundDct.disGround > 0.2f))
        {
            //输入力造成的惯性
            if (horizontalInputSpeed != 0)
            {
                //区分左右
                if (horizontalInputSpeed > 0)
                {
                    if (lastFrameSpeedVector.x < 0)
                    {
                        turnCount++;
                    }
                }
                else if (horizontalInputSpeed < 0)
                {
                    if (lastFrameSpeedVector.x > 0)
                    {
                        turnCount++;
                    }
                }

                //输入速度接近满值才有惯性。
                if (Mathf.Abs(horizontalInputSpeed) >= 0.9f)
                {
                    lastFrameSpeedVector.x = horizontalInputSpeed * inertiaInput / (turnCount * turnLoss + 1);

                    if (lastFrameSpeedVector.x < 0)
                    {
                        lastFrameSpeedVector.x += StaySkyTime * inertiaDrag;
                        if (lastFrameSpeedVector.x > 0)
                        {
                            lastFrameSpeedVector.x = 0;
                        }
                    }
                    else if (lastFrameSpeedVector.x > 0)
                    {
                        lastFrameSpeedVector.x -= StaySkyTime * inertiaDrag;
                        if (lastFrameSpeedVector.x < 0)
                        {
                            lastFrameSpeedVector.x = 0;
                        }
                    }
                }
            }

            //（在斜面、冰面）滑落造成的惯性
            if (horizontalInputSpeed == 0 && lastFrameSpeedVector.x == 0)
            {
                //要判断角色的朝向和滑落方向是否一致。（若不判断，会出现对着斜面跳起后，被反弹回来的情况）
                if (role.moveProc.faceLeft && role.slideProc.slideVector.x < 0)
                {
                    lastFrameSpeedVector.x = role.slideProc.slideVector.x * inertiaSlide;
                }
                else if (role.moveProc.faceLeft == false && role.slideProc.slideVector.x > 0)
                {
                    lastFrameSpeedVector.x = role.slideProc.slideVector.x * inertiaSlide;
                }
            }
        }
    }

    void CalculateMoving()
    {
        //转向---------------------------begin
        if (horizontalInputSpeed == 0)
        {
            AutoFace();
        }
        else if (horizontalInputSpeed > 0)
        {
            TurnRight();
        }
        else if (horizontalInputSpeed < 0)
        {
            TurnLeft();
        }
        //转向---------------------------end

        //计算移动距离（如果按左右方向键，则设置速度，否则速度递减）
        if (horizontalInputSpeed != 0)
        {
            //获取脚向量
            Vector3 footVector = role.groundDct.footVector;
            //归一是为了防止特别长的移动向量。
            footVector.Normalize();

            //在空中时的必要条件！！！
            if (role.groundDct.isClosedGround && role.groundDct.disGround > 0.1f)
            {
                //1.将x轴赋值为1，是为了防止平移时的卡顿（比如之前的bug:从倾斜的台子落下，向量会从水平变为斜向下，水平速度会突然变慢）         
                //2.但是在斜面上跑的时候会很快。应该判断是否在空中，在空中才设置成1。
                footVector.x = 1;
                //如果在空中，则忽视Y轴。不能用isonground是因为太贴近地面时会滑行，必须在接近地面时就把y设为0.
                footVector.y = 0;
            }

            //转向损耗
            float speedLoss = (turnCount * turnLoss + 1);
            float _moveDis = moveSpeed * role.deltaTime * horizontalInputSpeed / speedLoss;

            Vector3 moveVector = footVector * _moveDis;
            Moving(moveVector);
        }
        else if (lastFrameSpeedVector != Vector3.zero)
        {
            //惯性移动（冰面、大斜面、跳跃到空中）
            if (role.groundDct.IsOnGround() == false)
            {
                //注意：惯性不能使用Y值。否则会影响跳跃高度，甚至穿越地面。
                Vector3 inertiaVector = new Vector3(lastFrameSpeedVector.x, 0, 0) * role.deltaTime;
                Moving(inertiaVector);
            }
        }

    }

    void Moving(Vector3 moveVector)
    {
        //判断该方向是否可以移动
        if ((moveVector.x < 0 && movDct.moveToLeft) || (moveVector.x > 0 && movDct.moveToRight))
        {
            transform.Translate(moveVector);
        }
    }

    //自动调整朝向参数
    void AutoFace()
    {
        if (model.transform.rotation.eulerAngles.y < 90)
        {
            faceLeft = false;
        }
        if (model.transform.rotation.eulerAngles.y > 90)
        {
            faceLeft = true;
        }
    }
    public void TurnLeft()
    {
        //print("向左转");
        faceLeft = true;
        if (model.transform.rotation.eulerAngles.y < 90)
        {
            model.transform.Rotate(Vector3.up, 180);
        }
    }
    public void TurnRight()
    {
        //print("向右转");
        faceLeft = false;
        if (model.transform.rotation.eulerAngles.y > 90)
        {
            model.transform.Rotate(Vector3.down, 180);
        }
    }

    //重置惯性
    public void ResetInertia()
    {
        lastFrameSpeedVector = Vector3.zero;
        turnCount = 0;
        StaySkyTime = 0;
    }

    public void PlayerDead()
    {
    }

    public void PlayerRespawn()
    {
        horizontalInputSpeed = 0;
        horizontalExternalSpeed = 0;
        //重置惯性
        ResetInertia();
    }
}
