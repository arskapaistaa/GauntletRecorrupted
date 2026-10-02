using Godot;

public partial class Bullet : Area2D
{
	[ExportCategory("Bullet Stats")]
	[Export(PropertyHint.Range, "0,10000,100")] public float Speed = 800.0f;
	[Export(PropertyHint.Range, "0,100,1")] public int Damage = 10;
	[Export(PropertyHint.Range, "0,10,0.5")] public float Lifetime = 2.0f;

	private Vector2 _direction = Vector2.Right;

	public void Initialize(Vector2 direction)
	{
		_direction = direction.Normalized();
		Rotation = _direction.Angle();
	}

	public override void _Ready()
	{
		GetTree().CreateTimer(Lifetime).Timeout += QueueFree;
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition += _direction * Speed * (float)delta;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Enemy enemy)
		{
			enemy.TakeDmg(Damage);
		}
		QueueFree();
	}
}