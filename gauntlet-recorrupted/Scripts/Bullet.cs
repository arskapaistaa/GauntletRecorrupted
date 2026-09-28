using Godot;

public partial class Bullet : Area2D
{
	[Export] public float Speed = 800.0f;
	[Export] public float Lifetime = 2.0f;

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
		// tähän vielä damagen händläys
		QueueFree();
	}
}