using Godot;

public partial class Projectile : Area2D
{
	[ExportCategory("Bullet Stats")]
	[Export(PropertyHint.Range, "0,10000,100")]
    public float Speed = 800.0f;

	[Export(PropertyHint.Range, "0,100,1")]
    public int RevolverBulletDamage = 10;
	[Export(PropertyHint.Range, "0,100,1")]
    public int Damage = 5;
	[Export(PropertyHint.Range, "0,10,0.5")]

    public float Lifetime = 5.0f;
	private float _timeAlive = 0.0f;
	private Vector2 _direction = Vector2.Right;

	public void Initialize(Vector2 direction)
	{
		_direction = direction.Normalized();

		Rotation = _direction.Angle();
	}

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

    public override void _ExitTree()
    {
       BodyEntered -= OnBodyEntered;
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
        if (body is PlayerCharacter player)
        {
            player.TakeDmg(Damage);
        }

		QueueFree();
	}
}