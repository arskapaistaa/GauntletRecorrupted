using Godot;
using System;

public partial class EnemySpawner : StaticBody2D
{
	[Export] private PackedScene _enemyScene = null;

	[Export(PropertyHint.Range, "1, 180.0, 1.0")]
	private float _spawningCooldown = 1.0f;
	[Export] private Timer _timer;
	[Export] private int _health;
	[Export] private AnimatedSprite2D _sprite;

	public enum State
	{
		Idle,
		Spawn
	}

	public State CurrentState = State.Idle;

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

		if (_sprite != null)
		{
			_sprite.AnimationFinished += OnAnimationFinished;
		}
	}

    public override void _PhysicsProcess(double delta)
    {
        UpdateAnimation();
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

		if (Level.Current.CurrentTotalEnemyCount >= Level.Current.MaxTotalEnemyCount)
		{
			return;
		}

		CurrentState = State.Spawn;

		Enemy enemy = _enemyScene.Instantiate<Enemy>();

		GetParent().AddChild(enemy);
		enemy.GlobalTransform = GlobalTransform;

		Level.Current.ChangeEnemyCount(1);
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

	private void UpdateAnimation()
	{
		if (_sprite != null)
		{
			return;
		}

		switch (CurrentState)
		{
			case State.Idle:
				_sprite.Play("Idle");
				break;

			case State.Spawn:
				_sprite.Play("Spawn");
				break;
		}
	}

	private void OnAnimationFinished()
    {
        if (_sprite.Animation == "Spawn")
		{
			CurrentState = State.Idle;
		}
    }
}