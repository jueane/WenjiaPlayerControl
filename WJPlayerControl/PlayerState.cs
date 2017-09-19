using UnityEngine;
using System.Collections;

/// <summary>
/// 角色状态
/// </summary>
public class PlayerState {

	public bool IsDead{get;set;}
	public bool CanJump{get;set;}	
	public bool OnGround{get;set;}
	public bool OnMovingPlatform { get; set; }
    public bool IsFaceGround { get; set; }
	public bool IsFloating{ get; set;}
    public bool IsEdeg{get;set;}
    public bool Jumping;
	public bool StartJump;
	public bool CanMoveFreely;
	public bool IsOnJumper{get;set;}
    public bool CanSave { get; set; }
	public bool CarrySoul { get; set; }
    public bool OnIce { get; set; }
//    public bool CanDoubleJump { get; set; }

	public void Init()
	{

		IsDead = false;
		CanJump = true;	
		OnGround = false;
		CanMoveFreely = true;	
		StartJump = false;
		OnMovingPlatform = false;
		IsFloating = false;
        IsFaceGround = false;
        IsEdeg = false;
		IsOnJumper = false;
        CanSave = false;
		CarrySoul = false;
        OnIce = false;
//		CanDoubleJump = false;
    }
}
