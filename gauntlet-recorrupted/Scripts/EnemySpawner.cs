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
	/// <summary>
	/// Can be set 0,0. If you want enemy to go spawn position, then set this.
	/// </summary>
	[Export] private Marker2D _spawnPoint;
	[Export] private int _spawnFrame = 0;

	public enum State
	{
		Idle,
		Spawn,
		Damaged,
		Defeat
	}

	public State CurrentState = State.Idle;

	public bool IsAlive = true;

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
			_sprite.FrameChanged += OnFrameChanged;
		}
	}

    public override void _ExitTree()
    {
        _timer.Timeout -= OnTimedOut;

		if (!_timer.IsStopped())
		{
			_timer.Stop();
		}

		if (_sprite != null)
		{
			_sprite.AnimationFinished -= OnAnimationFinished;
			_sprite.FrameChanged -= OnFrameChanged;
		}
    }

    private void OnFrameChanged()
    {
		// Spawn only on one frame.
		// If needed, spawning can be also checked in _Process.
        if (_sprite.Animation == "Spawn")
		{
			if (_sprite.Frame == _spawnFrame)
			{
				Enemy enemy = _enemyScene.Instantiate<Enemy>();

				GetParent().AddChild(enemy);
				enemy.GlobalTransform = GlobalTransform;

				if (_spawnPoint != null)
				{
					enemy.SpawnPoint = _spawnPoint.GlobalPosition;
				}
			}
		}
    }

    public override void _Process(double delta)
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

		if (!IsAlive)
		{
			return;
		}

		if (Level.Current.CurrentTotalEnemyCount >= Level.Current.MaxTotalEnemyCount)
		{
			return;
		}

		CurrentState = State.Spawn;

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

		if (_health <= 0)
		{
			Destroy();
		}

		if (CurrentState != State.Spawn && IsAlive)
		{
			CurrentState = State.Damaged;
		}
	}

    private void Destroy()
    {
		IsAlive = false;
        CurrentState = State.Defeat;
    }

	private void UpdateAnimation()
	{
		if (_sprite == null)
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

			case State.Damaged:
				_sprite.Play("Damage");
				break;

			case State.Defeat:
				_sprite.Play("Defeat");
				break;
		}
	}

	private void OnAnimationFinished()
    {
        if (_sprite.Animation == "Spawn")
		{
			CurrentState = State.Idle;
		}

		if (_sprite.Animation == "Damage")
		{
			CurrentState = State.Idle;
		}

		if (_sprite.Animation == "Defeat")
		{
			QueueFree();
		}
    }
}