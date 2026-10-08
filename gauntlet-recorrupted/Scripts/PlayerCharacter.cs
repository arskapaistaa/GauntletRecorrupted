using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{

	[ExportCategory("Player Stats")]
	[Export(PropertyHint.Range, "0,1000,10")] public float Speed = 300.0f;
	[Export(PropertyHint.Range, "0,10,1")] public int MagSize = 6;
	[Export(PropertyHint.Range, "0,100,1")] public int Health = 100;
	[Export(PropertyHint.Range, "0,5,0.1")] public float ShootCooldown = 0.5f;
	[Export(PropertyHint.Range, "0,10,0.1")] public float ReloadTime = 2.0f;

	[ExportCategory("Node References")]
	[Export] public PackedScene BulletScene;
	[Export] public TileMapLayer DiggableTiles;
	[Export] public Marker2D MuzzlePoint;

	private float _reloadSpeedMultiplier = 1.0f;

	private float _shootTimer = 0;
	private bool _canShoot = true;
	private int _maxHealth = 100;
	private Vector2I playerCell;

	public enum FacingDirection
	{
		North,
		East,
		South,
		West
	}

	public override void _PhysicsProcess(double delta)
	{
		LookAt(GetGlobalMousePosition());
		Vector2 velocity = Velocity;
		float currentSpeed = Speed * _reloadSpeedMultiplier;

		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity = direction * currentSpeed;
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, currentSpeed);
		}
		Velocity = velocity;
		MoveAndSlide();

		if (_shootTimer > 0)
		{
			_shootTimer -= (float)delta;
		}
		else
		{
			_shootTimer = 0;
			_reloadSpeedMultiplier = 1.0f;
		}
		playerCell = DiggableTiles.LocalToMap(DiggableTiles.ToLocal(GlobalPosition));
		HandleInputs();
	}

	public void HandleInputs()
	{
		if (Input.IsActionJustPressed("Shoot") && _shootTimer <= 0)
		{
			if (!_canShoot)
			{
				GD.Print("Out of ammo");
				return;
			}
			Shoot();
			MagSize--;
			GD.Print($"MagSize: {MagSize}");
			_shootTimer = ShootCooldown;
			if (MagSize <= 0)
			{
				_canShoot = false;
			}
		}

		if (Input.IsActionJustPressed("Reload"))
		{
			_reloadSpeedMultiplier = 0.5f;
			MagSize = 6;
			_shootTimer = ReloadTime;
			_canShoot = true;
			GD.Print("Reloading");
		}


		if (Input.IsActionJustPressed("Dig"))
		{
			Dig();
		}
	}

	private void Shoot()
	{
		if (BulletScene == null || MuzzlePoint == null)
		{
			return;
		}

		Bullet bullet = BulletScene.Instantiate<Bullet>();
		GetTree().CurrentScene.AddChild(bullet);
		bullet.GlobalPosition = MuzzlePoint.GlobalPosition;

		Vector2 shootDirection = (GetGlobalMousePosition() - MuzzlePoint.GlobalPosition).Normalized();
		bullet.Initialize(shootDirection);
	}

	private void Dig()
	{
		GD.Print($"Player's position in grid: {playerCell}");
		GD.Print($"Player is facing: {GetFacingDirection()}");
		Vector2I targetCell;
		switch (GetFacingDirection())
		{
			case FacingDirection.North:
				targetCell = playerCell + new Vector2I(0, -1);
				break;
			case FacingDirection.East:
				targetCell = playerCell + new Vector2I(1, 0);
				break;
			case FacingDirection.South:
				targetCell = playerCell + new Vector2I(0, 1);
				break;
			case FacingDirection.West:
				targetCell = playerCell + new Vector2I(-1, 0);
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
			// TODO: Handle death
		}
	}

	private FacingDirection GetFacingDirection()
    {
        Vector2 direction = GlobalTransform.X;

        if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
        {
            // More horizontal than vertical
            if (direction.X > 0)
            {
                return FacingDirection.East;
            }
            return FacingDirection.West;
        }

        // More vertical than horizontal
        if (direction.Y > 0)
        {
            return FacingDirection.South;
        }
        return FacingDirection.North;
    }
}