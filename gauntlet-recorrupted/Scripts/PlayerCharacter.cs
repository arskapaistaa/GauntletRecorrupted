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
	[Export] public int Health = 10;

	private float _shootTimer = 0;

	public override void _PhysicsProcess(double delta)
	{
		LookAt(GetGlobalMousePosition());
		Vector2 velocity = Velocity;

		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
		}
		else
		{
			velocity = velocity.MoveToward(Vector2.Zero, Speed);
		}
		Velocity = velocity;
		MoveAndSlide();

		if (_shootTimer > 0)
		{
			_shootTimer -= (float)delta;
		}
		HandleInputs();
	}

	public void HandleInputs()
	{
		if (Input.IsActionJustPressed("Shoot") && _shootTimer <= 0)
		{
			Shoot();
			MagSize--;
			GD.Print($"MagSize: {MagSize}");
			_shootTimer = ShootCooldown;
			if (MagSize <= 0)
			{
				GD.Print("Reloading...");
				_shootTimer = ReloadTime;
				MagSize = 6;
				GD.Print($"MagSize: {MagSize}");
			}
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