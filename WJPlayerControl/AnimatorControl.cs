using UnityEngine;
using System.Collections;

public class AnimatorControl : MonoBehaviour
{

    private Animator _animator;
    private float Horizontal;
    private PlayerControl _player;
    public bool canUpdate;
    public bool transferDoorJump;
    public GameObject jumpEffect;
    public Vector3 savePos;
    public bool CanSave = false;
    public bool CarrySoul = false;
    public bool VerticalInput = false;
    public bool FloatingInput = false;
    // 空闲时间
    public int idleActionNum;
    public float firstIdleTime = 5f;
    public float secondIdleTime = 10f;
    float idleTimeCount;
    bool isFirstIdleTime = true;

    int idleActionIndex = 0;
    int lastIdleActionIndex = 0;

    // 跳跃随机数
    public int jumpRandomNum = 3;

    // Use this for initialization
    void Start()
    {

        _animator = GameManager.Instance._animator;
        _player = GameManager.Instance.Player;
        canUpdate = true;
        transferDoorJump = false;
    }

    // Update is called once per frame
    void Update()
    {
        //        if (transferDoorJump && Input.GetButtonDown("Jump") && _player.multijump == false)
        if (transferDoorJump && InputMgr.Instance.catInput.KeyA.WasPressed && _player.jumpProc.multijump == false)
        {
            _animator.SetBool("transferDoorJump", true);
        }
        //}
        UpdateAnimation();
    }


    void UpdateAnimation()
    {
        bool closedGround = _player.groundDct.isClosedGround && _player.groundDct.disGround < 0.3f;
        _animator.SetBool("Falling", (_player.state == RoleState.Falling && closedGround == false));
        _animator.SetBool("Ground", _player.groundDct.IsOnGround() && _player.state != RoleState.Raising);
        _animator.SetBool("Raising", _player.state == RoleState.Raising);
        _animator.SetBool("Floating", _player.isFloating);
        _animator.SetBool("VerticalInput", this.VerticalInput);
        if (canUpdate)
        {
            _animator.SetFloat("InputH", Mathf.Abs(_player.moveProc.horizontalInputSpeed));
        }
        else
        {
            _animator.SetFloat("InputH", Mathf.Abs(Horizontal));
        }



        bool idle = _player.groundDct.IsOnGround() && _player.groundDct.slope < _player.maxFrictionSlope && _player.moveProc.lastFrameSpeedVector == Vector3.zero && _player.moveProc.horizontalExternalSpeed == 0 && _player.moveProc.horizontalInputSpeed == 0;

        idleActionIndex = 0;
        if (idle)
        {
            //			print ("猫空闲中");
            idleTimeCount += _player.deltaTime;
            if (isFirstIdleTime)
            {
                if (idleTimeCount >= firstIdleTime)
                {
                    // 从运动到静止第一次休闲动作
                    // print("第一次休闲");
                    idleTimeCount = 0;
                    isFirstIdleTime = false;
                    RandomIdleAction();
                }

            }
            else
            {
                if (idleTimeCount >= secondIdleTime)
                {
                    // 从运动到静止第二次及以后休闲动作
                    // print("第二次休闲");
                    idleTimeCount = 0;
                    RandomIdleAction();
                }
            }
        }
        else
        {
            idleTimeCount = 0;
            isFirstIdleTime = true;
            lastIdleActionIndex = 0;
        }
        _animator.SetInteger("IdleActionIndex", idleActionIndex);

        //爬墙判定 （玩家不能左移且面向左 或 玩家不能右移且面向右) 且 玩家倾斜小于0.3（90 * 0.3 = 27°）
        if (_player == null)
        {
            print("player is null");
        }
        else if (_player.moveProc == null)
        {
            print("moveProc is null");
        }
        else if (_player.groundDct == null)
        {
            print("groundDct is null");
        }
        else if (_player.moveProc.movDct == null)
        {
            print("movDct is null");
        }
        _animator.SetBool("AgainstWall", (((_player.moveProc.movDct.moveToLeft == false && _player.moveProc.faceLeft) || (_player.moveProc.movDct.moveToRight == false && _player.moveProc.faceLeft == false)) && _player.groundDct.slope <= 0.3));
        _animator.SetFloat("DisToGround", _player.groundDct.disGround);
        _animator.SetBool("OnIceGround", _player.groundDct.isHitIce && _player.groundDct.isClosedGround && _player.groundDct.disGround < 0.5f);
        _animator.SetBool("IsDoubleJump", _player.jumpProc.multijump);
        _animator.SetBool("OnJumper", Eject.onJumper);
        _animator.SetBool("JumpingOnJumper", Eject.jumpingOnJumper);
        _animator.SetInteger("DoubleJumpNum", _player.jumpProc.remainJumpTimes);
        //判断猫在浮空时候是否有操作
        if (_animator.GetBool("Floating"))
        {
            if (_animator.GetFloat("InputH") > 0 || _animator.GetBool("VerticalInput") == true)
            {
                _animator.SetBool("FloatingInput", true);
            }
            else
            {
                _animator.SetBool("FloatingInput", false);
            }
        }

        if (_player.groundDct.IsOnGround())
        {
            _animator.SetBool("transferDoorJump", false);
            transferDoorJump = false;
        }
    }

    public void setHorizontal(float value)
    {
        this.Horizontal = value;
    }

    public void DoubleJumpEffect(Vector3 pos)
    {
        _animator.Play("DoubleJump");
        if (jumpEffect != null)
        {
            GameObject o = Instantiate(jumpEffect, pos + new Vector3(0, -0.4f, 0), Quaternion.identity) as GameObject;
            o.transform.SetParent(GameManager.Instance.temp.transform);

            AudioManager.Instance.PlayDoubleJumpSoundRandom(o.transform);
            GameObject.Destroy(o, 2f);
        }
    }

    void RandomIdleAction()
    {
        int num = Random.Range(1, idleActionNum + 1);
        //		print ("随机动作编号："+num);
        if (idleActionNum > 1)
        {
            while (num == lastIdleActionIndex)
            {
                //				print ("再次随机："+num);
                num = Random.Range(1, idleActionNum + 1);
            }
        }

        idleActionIndex = num;
        lastIdleActionIndex = num;
    }

    // 在JumpProcess中，执行跳的时候调用
    public void RandomJumpAction()
    {
        int num = Random.Range(0, jumpRandomNum);
        print("跳跃随机：" + num);
        _animator.SetFloat("JumpRandomIndex", num);
    }

}
