using Godot;

public partial class Bullet : Area2D
{
	[ExportCategory("Bullet Stats")]
	[Export(PropertyHint.Range, "0,10000,100")] public float RevolverBulletSpeed = 800.0f;
	[Export(PropertyHint.Range, "0,10000,100")] public float ShotgunBulletSpeed = 600.0f;
	[Export(PropertyHint.Range, "0,100,1")] public int RevolverBulletDamage = 10;
	[Export(PropertyHint.Range, "0,100,1")] public int ShotgunBulletDamage = 2;
	[Export(PropertyHint.Range, "0,10,0.5")] public float RevolverBulletLifetime = 2.0f;
	[Export(PropertyHint.Range, "0,10,0.5")] public float ShotgunBulletLifetime = 1.0f;

	private float Speed;
	private int Damage;
	private float Lifetime;
	private float _timeAlive = 0.0f;

	public int WeaponType { get; private set; } = 0; // 0 for revolver, 1 for shotgun
	private Vector2 _direction = Vector2.Right;

	public void Initialize(Vector2 direction, int weaponType)
	{
		_direction = direction.Normalized();
		WeaponType = weaponType;

		switch (WeaponType)
		{
			case 0: // Revolver
				Speed = RevolverBulletSpeed;
				Damage = RevolverBulletDamage;
				Lifetime = RevolverBulletLifetime;
				break;
			case 1: // Shotgun
				Speed = ShotgunBulletSpeed;
				Damage = ShotgunBulletDamage;
				Lifetime = ShotgunBulletLifetime;
				break;
			default:
				GD.PrintErr($"Unknown weapon type: {WeaponType}");
				QueueFree();
				return;
		}

		Rotation = _direction.Angle();
	}

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		_timeAlive += (float)delta;
		if (_timeAlive >= Lifetime)
		{
			QueueFree();
			return;
		}

		GlobalPosition += _direction * Speed * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDmg(Damage);
		}
		else if (body is EnemySpawner spawner)
		{
			spawner.TakeDmg(Damage);
		}

		QueueFree();
	}
}