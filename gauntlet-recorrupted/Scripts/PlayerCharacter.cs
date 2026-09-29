using Godot;
using System;

public partial class PlayerCharacter : CharacterBody2D
{

	[Export] public float Speed = 300.0f;
	[Export] public PackedScene BulletScene;
	[Export] public Marker2D MuzzlePoint;
	[Export] public float ShootCooldown = 0.5f;
	[Export] public int MagSize = 6;
	[Export] public float ReloadTime = 2.0f;
	[Export] public int MaxHealth = 100;
	[Export] public int Health = 100;

	[Export] public Marker2D SpawnPoint;
	private float _reloadSpeedMultiplier = 1.0f;

	private float _shootTimer = 0;
	private bool _canShoot = true;

	public override void _Ready()
	{
		GlobalPosition = SpawnPoint.GlobalPosition;
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
			GD.Print("Dig");
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

	public void TakeDmg(int damage)
	{
		Health -= damage;
		if (Health <= 0)
		{
			GD.Print("Player is dead");
			// TODO: Handle death
		}
	}
}