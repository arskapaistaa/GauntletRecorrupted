using Godot;
using System;

public partial class Cursor : CanvasLayer
{
	[Export] private AnimatedSprite2D _cursorSprite;
	[Export] private PlayerCharacter PlayerCharacter;
	public override void _Ready()
	{
		if (PlayerCharacter.getWeaponType() == 0)
		{
			_cursorSprite.Animation = "revolverAmmo";
			_cursorSprite.Frame = PlayerCharacter.getRevolverAmmo();
		}
		else if (PlayerCharacter.getWeaponType() == 1)
		{
			_cursorSprite.Animation = "shotgunAmmo";
			_cursorSprite.Frame = PlayerCharacter.getShotgunAmmo();
		}
		PlayerCharacter.WeaponSwitched += OnWeaponSwitched;
		PlayerCharacter.UpdateRevolverAmmo += OnUpdateRevolverAmmo;
		PlayerCharacter.UpdateShotgunAmmo += OnUpdateShotgunAmmo;
		_cursorSprite.AnimationFinished += OnAnimationFinished;
		PlayerCharacter.Reload += OnReload;
		Input.MouseMode = Input.MouseModeEnum.Hidden;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_cursorSprite.GlobalPosition = GetViewport().GetMousePosition();
	}

	private void OnWeaponSwitched(int newWeaponType)
	{
		switch (newWeaponType)
		{
			case 0: // Revolver
				_cursorSprite.Animation = "revolverAmmo";
				_cursorSprite.Frame = PlayerCharacter.getRevolverAmmo();
				break;
			case 1: // Shotgun
				_cursorSprite.Animation = "shotgunAmmo";
				_cursorSprite.Frame = PlayerCharacter.getShotgunAmmo();
				break;
			default:
				GD.PrintErr($"Unknown weapon type: {newWeaponType}");
				break;
		}
	}

	private void OnUpdateRevolverAmmo(int currentAmmo)
	{
		if (_cursorSprite.Animation == "revolverAmmo")
		{
			_cursorSprite.Frame = currentAmmo;
		}
	}

	private void OnUpdateShotgunAmmo(int currentAmmo)
	{
		if (_cursorSprite.Animation == "shotgunAmmo")
		{
			_cursorSprite.Frame = currentAmmo;
		}
	}

	private void OnReload(int weaponType)
	{
		switch (weaponType)
		{
			case 0: // Revolver
				_cursorSprite.Play("revolverReload");
				break;
			case 1: // Shotgun
				_cursorSprite.Play("shotgunReload");
				break;
			default:
				GD.PrintErr($"Unknown weapon type: {weaponType}");
				break;
		}
	}

	private void OnAnimationFinished()
	{
		if (_cursorSprite.Animation == "revolverReload")
		{
			_cursorSprite.Animation = "revolverAmmo";
			_cursorSprite.Frame = PlayerCharacter.getRevolverAmmo();
		}
		else if (_cursorSprite.Animation == "shotgunReload")
		{
			_cursorSprite.Animation = "shotgunAmmo";
			_cursorSprite.Frame = PlayerCharacter.getShotgunAmmo();
		}
	}
}
