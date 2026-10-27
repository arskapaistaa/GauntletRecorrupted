using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{
	[Export(PropertyHint.Enum, "Gravedigger,MilitaryGirl")] private int CharacterType = 0;
	[ExportCategory("Stats")]
	[ExportGroup("Player Stats")]
	[Export(PropertyHint.Range, "0,1000,10")] private float Speed = 300.0f;
	[Export(PropertyHint.Range, "0,100,1")] private int Health = 100;
	[Export(PropertyHint.Range, "0,100,1")] private int MeleeDamage = 10;
	[Export(PropertyHint.Range, "0,5,0.1")] private float MeleeCooldown = 0.5f;
	[ExportGroup("Weapon Stats")]
	[Export(PropertyHint.Range, "0,10,0.1")] private float RevolverCooldown = 0.5f;
	[Export(PropertyHint.Range, "0,10,0.1")] private float RevolverReloadTime = 2.0f;
	[Export(PropertyHint.Range, "0,10,0.1")] private float ShotgunCooldown = 1.0f;
	[Export(PropertyHint.Range, "0,70,1.0")] private float ShotgunSpreadAngle = 15.0f;
	[Export(PropertyHint.Range, "0,50,1")] private int ShotgunPelletCount = 10;
	[Export(PropertyHint.Range, "0,10,0.1")] private float ShotgunReloadTime = 1.0f;
	[Export(PropertyHint.Range, "0,50000,500")] private float RecoilForce = 1000.0f;
	[Export(PropertyHint.Range, "0,50000,100")] private float KnockbackDecay = 12500.0f;
	[Export(PropertyHint.Enum, "Revolver,Shotgun")] private int WeaponType = 0;

	[ExportCategory("Node References")]
	[Export] private PackedScene BulletScene;
	[Export] private TileMapLayer DiggableTiles;
	[Export] private Marker2D MuzzlePoint;
	[Export] private PackedScene BreakEffectScene;
	[Export] private AnimatedSprite2D PlayerSprite;
	[Export] private Area2D MeleeArea;
	[Export] private Cursor CustomCursor;

	[ExportGroup("Audios")]
	[Export] private AudioStreamPlayer2D _walkingSFX = null;
	[Export] private Sprite2D AimArrow;

	// Signals
	[Signal] public delegate void WeaponSwitchedEventHandler(int newWeaponType);
	[Signal] public delegate void UpdateRevolverAmmoEventHandler(int currentAmmo);
	[Signal] public delegate void UpdateShotgunAmmoEventHandler(int currentAmmo);
	[Signal] public delegate void ReloadEventHandler(int weaponType);
	[Signal] public delegate void ShotCameraShakeEventHandler(int weaponType);
	[Signal] public delegate void PeekCameraMovementEventHandler(bool isPeeking);
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
	private Vector2I playerCell;
	private bool isReloading = false;
	private bool isLiving = true;
	private float _meleeCooldown = 0;
	private bool _isMoving = false;
	private bool _isDigging = false;

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
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.Instance.IsUsingController())
			AimArrow.Visible = true;
		else
			AimArrow.Visible = false;
		
		playerCell = DiggableTiles.LocalToMap(DiggableTiles.ToLocal(GlobalPosition));
		HandleTimers(delta);
		HandleInputs();
		HandleMovement(delta);
		HandleAnimations(); 
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
			_reloadSpeedMultiplier = 1.0f;
			isReloading = false;
		}
	}

	private void HandleMovement(double delta)
	{
		if (isLiving)
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
		if (isLiving)
		{
			MoveAndSlide();
		}
	}
	
	private void OnAnimationFinished()
	{
		if (_isDigging)
		{
			_isDigging = false;
		}
	}

	public void HandleInputs()
	{
		if (Input.IsActionJustPressed("WeaponSwitch"))
		{
			if (!isLiving)
			{
				GD.Print("Cannot switch weapons while dead");
				return;
			}
			if (isReloading)
			{
				GD.Print("Cannot switch weapons while reloading");
				return;
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
			// Signal weapon switch event.
			EmitSignal(SignalName.WeaponSwitched, WeaponType);
		}

		if (Input.IsActionJustPressed("Shoot") && _shootTimer <= 0)
		{
			if (!isLiving)
			{
				GD.Print("Cannot shoot while dead");
				return;
			}
			if ((WeaponType == 0 && _currentAmmoRevolver <= 0) || (WeaponType == 1 && _currentAmmoShotgun <= 0))
			{
				if (WeaponType == 0)
				{
					GD.Print("Revolver out of ammo");
				}
				else
				{
					GD.Print("Shotgun out of ammo");
				}
				return;
			}

			Shoot();
			EmitSignal(SignalName.ShotCameraShake, WeaponType);

			if (WeaponType == 0)
			{
				_currentAmmoRevolver--;
				if (_currentAmmoRevolver < 0)
				{
					_currentAmmoRevolver = 0;
				}
				EmitSignal(SignalName.UpdateRevolverAmmo, _currentAmmoRevolver);
				_shootTimer = RevolverCooldown;
				GD.Print($"Revolver Ammo: {_currentAmmoRevolver}");
			}
			else
			{
				_currentAmmoShotgun--;
				if (_currentAmmoShotgun < 0)
				{
					_currentAmmoShotgun = 0;
				}
				EmitSignal(SignalName.UpdateShotgunAmmo, _currentAmmoShotgun);
				_shootTimer = ShotgunCooldown;
				GD.Print($"Shotgun Ammo: {_currentAmmoShotgun}");
			}
		}

		if (Input.IsActionJustPressed("Reload"))
		{
			if (!isLiving)
			{
				GD.Print("Cannot reload while dead");
				return;
			}
			_reloadSpeedMultiplier = 0.5f;
			if (WeaponType == 0)
			{
				if (_currentAmmoRevolver == _maxAmmoRevolver)
				{
					GD.Print("Revolver already full");
					return;
				}
				_currentAmmoRevolver = _maxAmmoRevolver;
				GD.Print("Reloading Revolver");
			}
			else
			{
				if (_currentAmmoShotgun == _maxAmmoShotgun)
				{
					GD.Print("Shotgun already full");
					return;
				}
				_currentAmmoShotgun = _maxAmmoShotgun;
				GD.Print("Reloading Shotgun");
			}
			EmitSignal(SignalName.Reload, WeaponType);
			_shootTimer = ReloadTime;
			isReloading = true;
		}


		if (Input.IsActionJustPressed("Dig"))
		{
			if (!isLiving)
			{
				GD.Print("Cannot dig while dead");
				return;
			}
			_isDigging = true;
			if (_meleeCooldown > 0)
			{
				foreach (Node2D node in MeleeArea.GetOverlappingBodies())
				{
					if (node is Enemy e)
					{
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
			}
			if (hitEnemy)
			{
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
		if (!isLiving)
		{
			// Handle dead animation
			return;
		}
		if (_isDigging)
		{
			PlayerSprite.Play("Dig");
		}
		else if (_isMoving)
		{
			PlayerSprite.Play("Walk");
		}
		else
		{
			PlayerSprite.Play("Idle");
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
				float angleOffset = Mathf.Lerp(-ShotgunSpreadAngle / 2, ShotgunSpreadAngle / 2, (float)i / (ShotgunPelletCount - 1));
				Vector2 pelletDirection = aimDirection.Rotated(Mathf.DegToRad(angleOffset));
				SpawnBullet(pelletDirection);
			}
			_knockback = -aimDirection * RecoilForce;
		}
	}

	private void SpawnBullet(Vector2 direction)
	{
		Bullet bullet = BulletScene.Instantiate<Bullet>();
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = MuzzlePoint.GlobalPosition;
		bullet.Initialize(direction, WeaponType);
	}

	private void Dig()
	{
		GD.Print($"Player's position in grid: {playerCell}");
		GD.Print($"Player is facing: {GetFacingDirection()}");
		Vector2I targetCell;
		// no diagonal digging if there are walls in front of the diagonal direction
		// so if there is wall north and east, you cannot dig northeast, etc.
		switch (GetFacingDirection())
		{
			case FacingDirection.East:
				targetCell = playerCell + new Vector2I(1, 0);
				break;
			case FacingDirection.SouthEast:
				if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, 1)) != null)
				{
					if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, 0)) == null || DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, 1)) == null)
						targetCell = playerCell + new Vector2I(1, 1);
					else if (GetNormalFacingDirection() == FacingDirection.East)
						targetCell = playerCell + new Vector2I(1, 0);
					else
						targetCell = playerCell + new Vector2I(0, 1);
				}
				else
				{
					if (GetNormalFacingDirection() == FacingDirection.East)
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, 0)) != null)
							targetCell = playerCell + new Vector2I(1, 0);
						else
							targetCell = playerCell + new Vector2I(0, 1);
					}
					else
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, 1)) != null)
							targetCell = playerCell + new Vector2I(0, 1);
						else
							targetCell = playerCell + new Vector2I(1, 0);
					}
				}
				break;
			case FacingDirection.South:
				targetCell = playerCell + new Vector2I(0, 1);
				break;
			case FacingDirection.SouthWest:
				if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, 1)) != null)
				{
					if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, 0)) == null || DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, 1)) == null)
						targetCell = playerCell + new Vector2I(-1, 1);
					else if (GetNormalFacingDirection() == FacingDirection.West)
						targetCell = playerCell + new Vector2I(-1, 0);
					else
						targetCell = playerCell + new Vector2I(0, 1);
				}
				else
				{
					if (GetNormalFacingDirection() == FacingDirection.West)
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, 0)) != null)
							targetCell = playerCell + new Vector2I(-1, 0);
						else
							targetCell = playerCell + new Vector2I(0, 1);
					}
					else
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, 1)) != null)
							targetCell = playerCell + new Vector2I(0, 1);
						else
							targetCell = playerCell + new Vector2I(-1, 0);
					}
				}
				break;
			case FacingDirection.West:
				targetCell = playerCell + new Vector2I(-1, 0);
				break;
			case FacingDirection.NorthWest:
				if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, -1)) != null)
				{
					if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, 0)) == null || DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, -1)) == null)
						targetCell = playerCell + new Vector2I(-1, -1);
					else if (GetNormalFacingDirection() == FacingDirection.West)
						targetCell = playerCell + new Vector2I(-1, 0);
					else
						targetCell = playerCell + new Vector2I(0, -1);
				}
				else
				{
					if (GetNormalFacingDirection() == FacingDirection.West)
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(-1, 0)) != null)
							targetCell = playerCell + new Vector2I(-1, 0);
						else
							targetCell = playerCell + new Vector2I(0, -1);
					}
					else
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, -1)) != null)
							targetCell = playerCell + new Vector2I(0, -1);
						else
							targetCell = playerCell + new Vector2I(-1, 0);
					}
				}
				break;
			case FacingDirection.North:
				targetCell = playerCell + new Vector2I(0, -1);
				break;
			case FacingDirection.NorthEast:
				if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, -1)) != null)
				{
					if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, 0)) == null || DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, -1)) == null)
						targetCell = playerCell + new Vector2I(1, -1);
					else if (GetNormalFacingDirection() == FacingDirection.East)
						targetCell = playerCell + new Vector2I(1, 0);
					else
						targetCell = playerCell + new Vector2I(0, -1);
				}
				else
				{
					if (GetNormalFacingDirection() == FacingDirection.East)
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(1, 0)) != null)
							targetCell = playerCell + new Vector2I(1, 0);
						else
							targetCell = playerCell + new Vector2I(0, -1);
					}
					else
					{
						if (DiggableTiles.GetCellTileData(playerCell + new Vector2I(0, -1)) != null)
							targetCell = playerCell + new Vector2I(0, -1);
						else
							targetCell = playerCell + new Vector2I(1, 0);
					}
				}
				break;
			default:
				return;
		}
		bool isEmpty = DiggableTiles.GetCellTileData(targetCell) == null;
		if (isEmpty)
		{
			GD.Print("Cannot dig that tile.");
			return;
		}
		SpawnBreakEffect(targetCell);
		DiggableTiles.EraseCell(targetCell);
		Level.Current.UpdateNavRegion();
		GD.Print($"Dug tile at {targetCell}");
	}

	public void TakeDmg(int damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			GD.Print("Player is dead");
			isLiving = false;
		}
		FlashSprite();
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

	private void SpawnBreakEffect(Vector2I cell)
	{
		if (BreakEffectScene == null)
		{
			return;
		}

		Node2D effect = BreakEffectScene.Instantiate<Node2D>();
		GetTree().CurrentScene.AddChild(effect);
		effect.GlobalPosition = DiggableTiles.ToGlobal(DiggableTiles.MapToLocal(cell));
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

	public int getRevolverAmmo()
	{
		return _currentAmmoRevolver;
	}

	public int getShotgunAmmo()
	{
		return _currentAmmoShotgun;
	}

	public int getWeaponType()
	{
		return WeaponType;
	}

	public bool isAlive()
	{
		return isLiving;
	}
}