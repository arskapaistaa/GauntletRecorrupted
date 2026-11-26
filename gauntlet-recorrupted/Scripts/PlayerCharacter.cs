using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{
	[ExportCategory("Stats")]
	[ExportGroup("Player Stats")]
	[Export(PropertyHint.Range, "0,1000,10")] private float Speed = 300.0f;
	[Export(PropertyHint.Range, "0,100,1")] private int Health = 100;
	[Export(PropertyHint.Range, "0,100,1")] private int MeleeDamage = 10;
	[Export(PropertyHint.Range, "0,5,0.1")] private float MeleeCooldown = 0.5f;
	[Export(PropertyHint.Range, "0,1000,10")] private float DigReach = 100.0f;
	[ExportGroup("Weapon Stats")]
	[Export(PropertyHint.Range, "0,10,0.1")] private float RevolverCooldown = 0.5f;
	[Export(PropertyHint.Range, "0,10,0.1")] private float RevolverReloadTime = 2.0f;
	[Export(PropertyHint.Range, "0,10,0.1")] private float ShotgunCooldown = 1.0f;
	[Export(PropertyHint.Range, "0,70,1.0")] private float ShotgunSpreadAngle = 15.0f;
	[Export(PropertyHint.Range, "0,50,1")] private int ShotgunPelletCount = 10;
	[Export(PropertyHint.Range, "0,10,0.01")] private float ShotgunReloadTime = 1.25f;
	[Export(PropertyHint.Range, "0,50000,500")] private float RecoilForce = 1000.0f;
	[Export(PropertyHint.Range, "0,50000,100")] private float KnockbackDecay = 12500.0f;
	[Export(PropertyHint.Enum, "Revolver,Shotgun")] private int WeaponType = 0;
	[ExportGroup("Particles")]
	[Export(PropertyHint.Range, "0.0,1.0,0.1")] public float BaseParticleAmountRatio = 0.1f;

	[ExportCategory("Node References")]
	[Export] private PackedScene BulletScene;
	[Export] private DiggableTiles DiggableTiles;
	[Export] private Marker2D MuzzlePoint;
	[Export] private PackedScene BreakEffectScene;
	[Export] private AnimatedSprite2D PlayerSprite;
	[Export] private Area2D MeleeArea;
	[Export] private Cursor CustomCursor;
	[Export] private AnimatedSprite2D AimArrow;
	[Export] private RayCast2D DigRay;
	[Export] private AudioListener2D AudioListener;
	[Export] private GpuParticles2D _dustParticles;

	[ExportGroup("Audios")]
	[Export] private AudioStreamPlayer2D _movementSFX = null;
	[Export] private AudioStreamPlayer2D _diggingSFX = null;
	[Export] private AudioStreamPlayer2D _meleeSFX = null;
	[Export] private AudioStreamPlayer2D _revolverSFX = null;
	[Export] private AudioStreamPlayer2D _shotgunSFX = null;
	[Export] private AudioStreamPlayer2D _reloadRevolverSFX = null;
	[Export] private AudioStreamPlayer2D _reloadShotgunSFX = null;
	[Export] private AudioStreamPlayer2D _switchToRevolverSFX = null;
	[Export] private AudioStreamPlayer2D _switchToShotgunSFX = null;

	// Signals
	[Signal] public delegate void WeaponSwitchedEventHandler(int newWeaponType);
	[Signal] public delegate void UpdateRevolverAmmoEventHandler(int currentAmmo);
	[Signal] public delegate void UpdateShotgunAmmoEventHandler(int currentAmmo);
	[Signal] public delegate void ReloadEventHandler(int weaponType);
	[Signal] public delegate void ShotCameraShakeEventHandler(int weaponType);
	[Signal] public delegate void PeekCameraMovementEventHandler(bool isPeeking);
	[Signal] public delegate void PlayerHealthChangedEventHandler();
	private Vector2 _knockback = Vector2.Zero;
	private Vector2 _moveVelocity = Vector2.Zero;
	private float ReloadTime;
	private int _maxAmmoRevolver = 6;
	private int _maxAmmoShotgun = 2;
	private int _currentAmmoRevolver;
	private int _currentAmmoShotgun;

	private float _reloadSpeedMultiplier = 1.0f;

	private float _shootTimer = 0;
	private bool _canShoot = true;
	private int _maxHealth = 100;
	private bool _isReloading = false;
	private bool _isLiving = true;
	private float _meleeCooldown = 0;
	private bool _isMoving = false;
	private bool _isDigging = false;
	private bool _isShooting = false;
	private int _forceRevolverReloadCounter = 0;
	private int _forceShotgunReloadCounter = 0;
	private const String ANIMATION_DIG = "Dig";
	private const String ANIMATION_REVOLVER_IDLE = "RevolverIdle";
	private const String ANIMATION_REVOLVER_WALK = "RevolverWalk";
	private const String ANIMATION_REVOLVER_SHOOT = "RevolverShoot";
	private const String ANIMATION_REVOLVER_RELOAD = "RevolverReload";
	private const String ANIMATION_SHOTGUN_IDLE = "ShotgunIdle";
	private const String ANIMATION_SHOTGUN_WALK = "ShotgunWalk";
	private const String ANIMATION_SHOTGUN_SHOOT = "ShotgunShoot";
	private const String ANIMATION_SHOTGUN_RELOAD = "ShotgunReload";

	public enum FacingDirection
	{
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest,
		North,
		NorthEast
	}

	public override void _Ready()
	{
		PlayerSprite.AnimationFinished += OnAnimationFinished;
		if (WeaponType == 0)
		{
			ReloadTime = RevolverReloadTime;
		}
		else if (WeaponType == 1)
		{
			ReloadTime = ShotgunReloadTime;
		}
		else
		{
			GD.PrintErr($"Unknown weapon type: {WeaponType}");
			return;
		}
		_currentAmmoRevolver = _maxAmmoRevolver;
		_currentAmmoShotgun = _maxAmmoShotgun;

		SetDustParticlesEmittingAmountRatio(BaseParticleAmountRatio);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.Instance.IsUsingController())
			AimArrow.Visible = true;
		else
			AimArrow.Visible = false;

		if (WeaponType == 0)
		{
			MuzzlePoint.Position = new Vector2(156, 4);
		}
		else if (WeaponType == 1)
		{
			MuzzlePoint.Position = new Vector2(124, 12);
		}
		HandleTimers(delta);
		HandleInputs();
		HandleMovement(delta);
		HandleAnimations();
		UpdateAudio();
		UpdateAimArrow();
		ForceShootingAnimationEnd();

		// To fix audio problem.
		if (AudioListener != null)
		{
			AudioListener.GlobalRotation = 0;
		}
	}

	private void HandleTimers(double delta)
	{
		if (_meleeCooldown > 0)
		{
			_meleeCooldown -= (float)delta;
		}
		else
		{
			_meleeCooldown = 0;
		}

		if (_shootTimer > 0)
		{
			_shootTimer -= (float)delta;
		}
		else
		{
			_shootTimer = 0;
			if (_isReloading)
			{
				FinishReload();
			}
		}
	}

	private void HandleMovement(double delta)
	{
		if (_isLiving)
		{
			if (GameManager.Instance.IsUsingController())
			{
				Vector2 aimInput = Input.GetVector("AimLeft", "AimRight", "AimUp", "AimDown", 0.5f);
				if (aimInput != Vector2.Zero)
				{
					GlobalRotation = aimInput.Angle();
				}
			}
			else
			{
				LookAt(GetGlobalMousePosition());
			}
		}
		Vector2 velocity = _moveVelocity;
		float currentSpeed = Speed * _reloadSpeedMultiplier;


		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			_isMoving = true;
			velocity = direction * currentSpeed;
		}
		else
		{
			_isMoving = false;
			velocity = velocity.MoveToward(Vector2.Zero, currentSpeed);
		}
		_moveVelocity = velocity;
		Velocity = _moveVelocity + _knockback;
		_knockback = _knockback.MoveToward(Vector2.Zero, KnockbackDecay * (float)delta);
		if (_isLiving)
		{
			MoveAndSlide();
		}
	}

	private void OnAnimationFinished()
	{
		if (PlayerSprite.Animation == ANIMATION_DIG)
		{
			_isDigging = false;
			if (WeaponType == 0)
				PlayerSprite.Play(ANIMATION_REVOLVER_IDLE);
			else if (WeaponType == 1)
				PlayerSprite.Play(ANIMATION_SHOTGUN_IDLE);
		}
		if (PlayerSprite.Animation == ANIMATION_REVOLVER_SHOOT || PlayerSprite.Animation == ANIMATION_SHOTGUN_SHOOT)
		{
			_isShooting = false;
			if (WeaponType == 0)
				PlayerSprite.Play(ANIMATION_REVOLVER_IDLE);
			else if (WeaponType == 1)
				PlayerSprite.Play(ANIMATION_SHOTGUN_IDLE);
		}
	}

	public void HandleInputs()
	{
		if (Input.IsActionJustPressed("WeaponSwitch"))
		{
			if (!_isLiving)
			{
				GD.Print("Cannot switch weapons while dead");
				return;
			}
			if (_isReloading)
			{
				CancelReload();
			}
			if (WeaponType == 0)
			{
				WeaponType = 1;
				ReloadTime = ShotgunReloadTime;
				GD.Print("Switched to Shotgun");
			}
			else
			{
				WeaponType = 0;
				ReloadTime = RevolverReloadTime;
				GD.Print("Switched to Revolver");
			}
			PlayWeaponSwitchSfx(WeaponType);
			EmitSignal(SignalName.WeaponSwitched, WeaponType);
		}

		if (Input.IsActionJustPressed("Shoot") && _shootTimer <= 0)
		{
			if (!_isLiving)
			{
				GD.Print("Cannot shoot while dead");
				return;
			}
			if ((WeaponType == 0 && _currentAmmoRevolver <= 0) || (WeaponType == 1 && _currentAmmoShotgun <= 0))
			{
				StartReload();
				return;
			}

			Shoot();
			EmitSignal(SignalName.ShotCameraShake, WeaponType);

			if (WeaponType == 0)
			{
				_currentAmmoRevolver--;
				EmitSignal(SignalName.UpdateRevolverAmmo, _currentAmmoRevolver);
				_shootTimer = RevolverCooldown;
			}
			else
			{
				_currentAmmoShotgun--;
				EmitSignal(SignalName.UpdateShotgunAmmo, _currentAmmoShotgun);
				_shootTimer = ShotgunCooldown;
			}
		}

		if (Input.IsActionJustPressed("Reload"))
		{
			if (!_isLiving)
			{
				GD.Print("Cannot reload while dead");
				return;
			}
			if (_isReloading)
				return;
			if (WeaponType == 0)
			{
				if (_currentAmmoRevolver == _maxAmmoRevolver)
				{
					GD.Print("Revolver already full");
					return;
				}
			}
			else
			{
				if (_currentAmmoShotgun == _maxAmmoShotgun)
				{
					GD.Print("Shotgun already full");
					return;
				}
			}
			StartReload();
		}


		if (Input.IsActionJustPressed("Dig"))
		{
			if (!_isLiving)
			{
				GD.Print("Cannot dig while dead");
				return;
			}
			if (_isReloading)
				return;
			_isDigging = true;
			if (_meleeCooldown > 0)
			{
				foreach (Node2D node in MeleeArea.GetOverlappingBodies())
				{
					if (node is Enemy e)
					{
						PlayMeleeSfx();
						return;
					}
				}
				Dig();
				return;
			}
			bool hitEnemy = false;
			foreach (Node2D node in MeleeArea.GetOverlappingBodies())
			{
				if (node is Enemy e)
				{
					e.TakeDmg(MeleeDamage);
					hitEnemy = true;
					GD.Print("Hit enemy with melee attack");
				}
				else if (node is EnemySpawner es)
				{
					es.TakeDmg(MeleeDamage);
					hitEnemy = true;
					GD.Print("Hit enemy spawner with melee attack");
				}
			}
			if (hitEnemy)
			{
				PlayMeleeSfx();
				_meleeCooldown = MeleeCooldown;
			}
			else
			{
				Dig();
			}
		}

		if (Input.IsActionJustPressed("Peek"))
		{
			EmitSignal(SignalName.PeekCameraMovement, true);
		}
		if (Input.IsActionJustReleased("Peek"))
		{
			EmitSignal(SignalName.PeekCameraMovement, false);
		}
	}

	private void HandleAnimations()
	{
		if (!_isLiving)
		{
			// Handle dead animation
			return;
		}
		if (_isDigging)
		{
			PlayerSprite.Play(ANIMATION_DIG);
		}
		else if (_isShooting)
		{
			if (WeaponType == 0)
			{
				PlayerSprite.Play(ANIMATION_REVOLVER_SHOOT);
			}
			else if (WeaponType == 1)
			{
				PlayerSprite.Play(ANIMATION_SHOTGUN_SHOOT);
			}
		}
		else if (_isReloading)
		{
			if (WeaponType == 0)
			{
				PlayerSprite.Play(ANIMATION_REVOLVER_RELOAD);
				AimArrow.Play("RevolverReload");
			}
			else if (WeaponType == 1)
			{
				PlayerSprite.Play(ANIMATION_SHOTGUN_RELOAD);
				AimArrow.Play("ShotgunReload");
			}
		}
		else if (_isMoving)
		{
			if (WeaponType == 0)
				PlayerSprite.Play(ANIMATION_REVOLVER_WALK);
			else if (WeaponType == 1)
				PlayerSprite.Play(ANIMATION_SHOTGUN_WALK);
		}

		else
		{
			if (WeaponType == 0)
				PlayerSprite.Play(ANIMATION_REVOLVER_IDLE);
			else if (WeaponType == 1)
				PlayerSprite.Play(ANIMATION_SHOTGUN_IDLE);
		}
	}

	private void Shoot()
	{
		if (BulletScene == null || MuzzlePoint == null)
		{
			return;
		}

		Vector2 aimDirection;
		if (GameManager.Instance.IsUsingController())
		{
			aimDirection = GlobalTransform.X.Normalized();
		}
		else
		{
			aimDirection = (GetGlobalMousePosition() - MuzzlePoint.GlobalPosition).Normalized();
		}

		if (WeaponType == 0)
		{
			Input.StartJoyVibration(0, 0.5f, 0.7f, 0.13f);
			SpawnBullet(aimDirection);
		}
		else if (WeaponType == 1)
		{
			Input.StartJoyVibration(0, 1.0f, 1.0f, 0.2f);
			for (int i = 0; i < ShotgunPelletCount; i++)
			{
				float spreadDegrees = (float)GD.RandRange(-ShotgunSpreadAngle, ShotgunSpreadAngle);
					float spreadRadians = Mathf.DegToRad(spreadDegrees);
					Vector2 shotDirection = aimDirection.Rotated(spreadRadians);
					SpawnBullet(shotDirection);
			}
			_knockback = -aimDirection * RecoilForce;
		}
		_isShooting = true;

		PlayGunSfx(WeaponType);
		UpdateAimArrow();

		// Tell GameManager that player shot.
		GameManager.Instance.OnShotsFiredChanged();
	}

	private void SpawnBullet(Vector2 direction)
	{
		Bullet bullet = BulletScene.Instantiate<Bullet>();
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = MuzzlePoint.GlobalPosition;
		bullet.Initialize(direction, WeaponType);
	}

	private void StartReload()
	{
		if (WeaponType == 0)
			_reloadSpeedMultiplier = 0.5f;
		else if (WeaponType == 1)
			_reloadSpeedMultiplier = 0.15f;
		_shootTimer = ReloadTime;
		_isReloading = true;
		PlayReloadSfx(WeaponType);
		EmitSignal(SignalName.Reload, WeaponType);
	}

	private void FinishReload()
	{
		if (WeaponType == 0)
		{
			_currentAmmoRevolver = _maxAmmoRevolver;
			EmitSignal(SignalName.UpdateRevolverAmmo, _currentAmmoRevolver);
		}
		else if (WeaponType == 1)
		{
			_currentAmmoShotgun = _maxAmmoShotgun;
			EmitSignal(SignalName.UpdateShotgunAmmo, _currentAmmoShotgun);
		}
		_isReloading = false;
		_reloadSpeedMultiplier = 1.0f;
		UpdateAimArrow();
	}

	private void CancelReload()
	{
		_isReloading = false;
		_shootTimer = 0;
		_reloadSpeedMultiplier = 1.0f;
		if (_reloadRevolverSFX.IsPlaying())
			_reloadRevolverSFX.Stop();
		if (_reloadShotgunSFX.IsPlaying())
			_reloadShotgunSFX.Stop();
	}

	private void ForceShootingAnimationEnd()
	{
		if ((PlayerSprite.Animation == ANIMATION_REVOLVER_SHOOT || PlayerSprite.Animation == ANIMATION_SHOTGUN_SHOOT) && PlayerSprite.Frame == PlayerSprite.SpriteFrames.GetFrameCount(PlayerSprite.Animation) - 1)
		{
			if (WeaponType == 0)
				PlayerSprite.Play(ANIMATION_REVOLVER_IDLE);
			else if (WeaponType == 1)
				PlayerSprite.Play(ANIMATION_SHOTGUN_IDLE);
			_isShooting = false;
		}
	}

	private void Dig()
	{
		DigRay.TargetPosition = new Vector2(DigReach, 0);
		DigRay.ForceRaycastUpdate();

		if (!DigRay.IsColliding() || DigRay.GetCollider() != DiggableTiles)
			return;

		// Ray hits the edge of the tile, so we move slightly inside the tile to get the correct target position.
		Vector2 targetTileInside = DigRay.GetCollisionPoint() - DigRay.GetCollisionNormal() * 0.2f;
		// Convert the target position to the cell in the tilemap
		Vector2I targetCell = DiggableTiles.LocalToMap(DiggableTiles.ToLocal(targetTileInside));

		if (DiggableTiles.GetCellTileData(targetCell) != null)
		{
			DiggableTiles.DeleteCell(targetCell);
			PlayDiggingSfx();
		}
	}


	public void TakeDmg(int damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			Health = 0;
			GD.Print("Player is dead");
			_isLiving = false;

			// Tell Game Manager that player is dead -> Track deaths.
			GameManager.Instance.OnDeathsChanged();
		}
		FlashSprite();

		EmitSignal(SignalName.PlayerHealthChanged);
	}

	public int GetHealth()
	{
		return Health;
	}

	private FacingDirection GetFacingDirection()
	{
		// Current facing direction
		Vector2 direction = GlobalTransform.X;
		// Angle of the facing direction in radians
		float angle = direction.Angle();
		// Circle is divided into 8 slices
		float sliceSize = Mathf.Tau / 8f;
		// Determine which slice the angle falls into
		int sliceIndex = Mathf.RoundToInt(angle / sliceSize);
		// Wrap the slice index so it is always between 0 and 7
		sliceIndex = Mathf.PosMod(sliceIndex, 8);
		return (FacingDirection)sliceIndex;
	}

	private FacingDirection GetNormalFacingDirection()
	{
		Vector2 direction = GlobalTransform.X;
		if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
		{
			return direction.X > 0 ? FacingDirection.East : FacingDirection.West;
		}
		else
		{
			return direction.Y > 0 ? FacingDirection.South : FacingDirection.North;
		}
	}


	private void FlashSprite()
	{
		Tween tween = CreateTween();
		for (int i = 0; i < 3; i++)
		{
			tween.TweenProperty(PlayerSprite, "visible", false, 0.0);
			tween.TweenInterval(0.08);
			tween.TweenProperty(PlayerSprite, "visible", true, 0.0);
			tween.TweenInterval(0.08);
		}
	}

	private void UpdateAimArrow()
	{
		if (AimArrow == null || _isReloading)
			return;

		if (WeaponType == 0)
		{
			AimArrow.Play("RevolverAmmo");
			AimArrow.Frame = GetRevolverAmmo();
		}
		else if (WeaponType == 1)
		{
			AimArrow.Play("ShotgunAmmo");
			AimArrow.Frame = GetShotgunAmmo();
		}
	}

	private void UpdateAudio()
	{
		if (_movementSFX == null)
		{
			GD.PrintErr("Player is missing Walking sfx");
			return;
		}

		if (_isMoving && _movementSFX.Playing == false)
		{
			_movementSFX.Play();
		}
	}

	public void SetDustParticlesEmittingAmountRatio(float amount)
	{
		if (_dustParticles != null)
		{
			_dustParticles.AmountRatio = amount;
		}
	}

	private void PlayGunSfx(int weaponType)
	{
		if (weaponType == 0)
		{
			if (_revolverSFX != null)
			{
				_revolverSFX.Play();
			}
		}
		else
		{
			if (_shotgunSFX != null)
			{
				_shotgunSFX.Play();
			}
		}
	}

	private void PlayReloadSfx(int weaponType)
	{
		if (weaponType == 0)
		{
			if (_reloadRevolverSFX != null)
			{
				_reloadRevolverSFX.Play();
			}
		}
		else
		{
			if (_reloadShotgunSFX != null)
			{
				_reloadShotgunSFX.Play();
			}
		}
	}

	private void PlayWeaponSwitchSfx(int weaponType)
	{
		if (weaponType == 0)
		{
			if (_switchToRevolverSFX != null)
			{
				_switchToRevolverSFX.Play();
			}
		}
		else
		{
			if (_switchToShotgunSFX != null)
			{
				_switchToShotgunSFX.Play();
			}
		}
	}

	private void PlayDiggingSfx()
	{
		if (_diggingSFX != null)
		{
			_diggingSFX.PitchScale = (float)GD.RandRange(0.9f, 1.1f);
			_diggingSFX.Play();
		}
	}
	private void PlayMeleeSfx()
	{
		if (_meleeSFX != null)
		{
			_meleeSFX.Play();
		}
	}
	public int GetRevolverAmmo()
	{
		return _currentAmmoRevolver;
	}

	public int GetRevolverMaxAmmo()
	{
		return _maxAmmoRevolver;
	}

	public float GetRevolverReloadTime()
	{
		return RevolverReloadTime;
	}

	public float GetShotgunReloadTime()
	{
		return ShotgunReloadTime;
	}

	public int GetShotgunAmmo()
	{
		return _currentAmmoShotgun;
	}

	public int GetShotgunMaxAmmo()
	{
		return _maxAmmoShotgun;
	}

	public int GetWeaponType()
	{
		return WeaponType;
	}

	public bool IsAlive()
	{
		return _isLiving;
	}
}
