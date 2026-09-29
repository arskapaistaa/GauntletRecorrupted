using Godot;
using System;

public partial class EnemySpawner : AnimatedSprite2D
{
	[Export] private PackedScene _enemyScene = null;

	[Export(PropertyHint.Range, "1, 180.0, 1.0")]
	private float _spawningCooldown = 1.0f;
	[Export] private Timer _timer;
	[Export] private int _health;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_timer == null)
		{
			GD.PrintErr("Spawner has no timer");
			return;
		}

		_timer.Timeout += OnTimedOut;

		if (_timer.IsStopped())
		{
			_timer.WaitTime = _spawningCooldown;
			_timer.Start();
		}
	}

    private void OnTimedOut()
    {
        Spawn();
    }

	private void Spawn()
	{
		if (_enemyScene == null)
		{
			GD.PrintErr("Spawner is missing enemy scene");
			return;
		}

		Enemy enemy = _enemyScene.Instantiate<Enemy>();

		GetParent().AddChild(enemy);
		enemy.GlobalTransform = GlobalTransform;
	}

	/// <summary>
	/// Call this from Bullet etc.
	/// </summary>
	/// <param name="dmg">Amount of damage</param>
	public void TakeDmg(int dmg)
	{
		if (_health > 0)
		{
			_health -= dmg;
		}

		if (_health < 0)
		{
			Destroy();
		}
	}

    private void Destroy()
    {
        QueueFree();
    }
}