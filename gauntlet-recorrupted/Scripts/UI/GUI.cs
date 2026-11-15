using Godot;
using System;

public partial class GUI : Control
{
	[Export] private Label _fpsLabel = null;
	[Export] private Label _healthLabel = null;
	[Export] private AnimatedSprite2D _healthIndicator = null;
	[Export] private AnimatedSprite2D _exitIndicator = null;
	[Export] private TextureRect Revolver = null;
	[Export] private TextureRect Shotgun = null;
	[Export] private AnimatedSprite2D _revolverAmmoSprite = null;
	[Export] private AnimatedSprite2D _shotgunAmmoSprite = null;
	[Export] private GpuParticles2D _exitOpenParticles = null;
	[Export] private GpuParticles2D _exitOpenParticlesSmall = null;

	private PlayerCharacter _player = null;
	private Level _currentLevel = null;
	private float _currentReloadFraction = 0;
	private float _reloadFractionTimer = 0;
	private int _reloadFractionAmount = 0;
	private int _reloadFractionAmountMax = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_player == null)
		{
			_player = Level.Current.Player;
			_player.PlayerHealthChanged += UpdateHealthIndicators;
		}

		if (_currentLevel == null)
		{
			_currentLevel = Level.Current;
			_currentLevel.OnZeroSpawnerLeft += OnZeroSpawnerLeft;
		}

		if (_player != null && _healthLabel != null)
		{
			UpdateHealthIndicators();
		}

		if (_exitIndicator != null)
		{
			_exitIndicator.Play("Closed");
			_exitIndicator.AnimationFinished += OnExitIndicatorAnimationFinished;
		}

		if (_exitOpenParticles != null)
		{
			_exitOpenParticles.Finished += OnExitOpenParticlesFinished;
		}

		if (_revolverAmmoSprite != null)
		{
			_revolverAmmoSprite.Frame = 6;
			_player.UpdateRevolverAmmo += UpdateRevolverAmmo;
			_player.Reload += ReloadAnimation;
		}
		if (_shotgunAmmoSprite != null)
		{
			_shotgunAmmoSprite.Frame = 6;
			_player.UpdateShotgunAmmo += UpdateShotgunAmmo;
			_player.Reload += ReloadAnimation;
		}
	}



    public override void _ExitTree()
    {
        if (_player != null)
		{
			_player.PlayerHealthChanged -= UpdateHealthIndicators;
			_player.UpdateRevolverAmmo -= UpdateRevolverAmmo;
			_player.UpdateShotgunAmmo -= UpdateShotgunAmmo;
			_player.Reload -= ReloadAnimation;
		}

		if (_exitIndicator != null)
		{
			_exitIndicator.AnimationFinished -= OnExitIndicatorAnimationFinished;
		}

		if (_exitOpenParticles != null)
		{
			_exitOpenParticles.Finished -= OnExitOpenParticlesFinished;
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_fpsLabel != null)
		{
			_fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
		}
		if (_reloadFractionAmount > 0 && _reloadFractionTimer >= 0)
		{
			_reloadFractionTimer -= (float)delta;
			if (_reloadFractionTimer <= 0)
			{
				int ammo = _reloadFractionAmountMax;
				_reloadFractionAmount--;
				ammo -= _reloadFractionAmount;
				if (_player.GetWeaponType() == 0)
				{
					UpdateRevolverAmmo(ammo);
				}
				else if (_player.GetWeaponType() == 1)
				{
					UpdateShotgunAmmo(ammo);
				}
				_reloadFractionTimer = _currentReloadFraction;
			}
		}
		UpdateWeaponVisibility();
	}

	private void UpdateWeaponVisibility()
	{
		if (_player.GetWeaponType() == 0)
		{
			if (Revolver != null)
			{
				Revolver.Visible = true;
			}
			if (Shotgun != null)
			{
				Shotgun.Visible = false;
			}
		}
		else if (_player.GetWeaponType() == 1)
		{
			if (Revolver != null)
			{
				Revolver.Visible = false;
			}
			if (Shotgun != null)
			{
				Shotgun.Visible = true;
			}
		}
	}

	public void UpdateHealthIndicators()
	{
		if (_player != null && _healthLabel != null)
		{
			_healthLabel.Text = _player.GetHealth().ToString();
		}

		// Hardcoded shit
		if (_player != null && _healthIndicator != null)
		{
			if (_player.GetHealth() > 75)
			{
				_healthIndicator.Frame = 0;
				return;
			}
			else if (_player.GetHealth() > 50)
			{
				_healthIndicator.Frame = 1;
				return;
			}
			else if (_player.GetHealth() > 25)
			{
				_healthIndicator.Frame = 2;
				return;
			}
			else
			{
				_healthIndicator.Frame = 3;
			}
		}
	}

	private void ReloadAnimation(int weaponType)
	{
		if (_revolverAmmoSprite == null)
		{
			return;
		}
		float reloadTime = 0;
		int ammoAmount = 0;
		if (weaponType == 0)
		{
			reloadTime = _player.GetRevolverReloadTime();
			ammoAmount = _player.GetRevolverMaxAmmo();
		}
		else if (weaponType == 1)
		{
			reloadTime = _player.GetShotgunReloadTime();
			ammoAmount = _player.GetShotgunMaxAmmo();
		}
		_currentReloadFraction = reloadTime / ammoAmount;
		_reloadFractionTimer = _currentReloadFraction;
		_reloadFractionAmount = ammoAmount;
		_reloadFractionAmountMax = ammoAmount;
	}

	private void UpdateShotgunAmmo(int ammo)
	{
		if (_shotgunAmmoSprite != null && _player != null)
		{
			_shotgunAmmoSprite.Frame = ammo;
		}
	}

	private void UpdateRevolverAmmo(int ammo)
	{
		if (_revolverAmmoSprite != null && _player != null)
		{
			_revolverAmmoSprite.Frame = ammo;
		}
	}

	private void OnZeroSpawnerLeft()
    {
		if (_exitIndicator == null)
		{
			GD.PrintErr("GUI is missing Exit Indicator");
		}

		if (_exitOpenParticles == null)
		{
			GD.PrintErr("GUI is missing Exit particles");
		}

		_exitIndicator.Play("Opening");
		_exitOpenParticles.Restart();
    }

	private void OnExitOpenParticlesFinished()
    {
        if (_exitOpenParticlesSmall != null)
		{
			_exitOpenParticlesSmall.Emitting = true;
		}
    }

	private void OnExitIndicatorAnimationFinished()
    {
        if (_exitIndicator == null)
		{
			GD.PrintErr("GUI is missing Exit Indicator");
			return;
		}

		_exitIndicator.Play("Open");
    }
}
