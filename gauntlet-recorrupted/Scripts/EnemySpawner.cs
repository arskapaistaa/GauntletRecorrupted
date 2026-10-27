using Godot;
using System;

public partial class EnemySpawner : StaticBody2D
{
	[ExportGroup("Stats")]
	[Export] private PackedScene _enemyScene = null;
	[Export(PropertyHint.Range, "1,180,1,suffix:seconds")]
	private float _spawningCooldown = 1.0f;
	[Export] private int _health;
	/// <summary>
	/// How many enemies can be active at the same time, spawned from this spawner.
	/// </summary>
	[Export(PropertyHint.Range, "1,20,suffix:at the same time")]
	private int _maxActiveEnemiesCount = 5;

	[ExportGroup("Node references")]
	[Export] private Timer _timer;
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private Marker2D _spawnPoint = null; // Can be left null. If you want enemy to go spawn position, then set this.

	[ExportGroup("Animations")]
	[Export] private int _spawnFrame = 0;

	[Signal] public delegate void SpawnerDamagedEventHandler();

	public enum State
	{
		Idle,
		Spawn,
		Damaged,
		Defeat
	}

	public State CurrentState = State.Idle;
	public bool IsAlive = true;
	private int _currentActiveEnemiesCount = 0;

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

	private void OnTimedOut()
    {
        StartSpawning();
    }

    private void OnFrameChanged()
    {
		// Spawn only on one frame.
		// If needed, spawning can be also checked in _Process.
        if (_sprite.Animation == "Spawn")
		{
			if (_sprite.Frame == _spawnFrame)
			{
				Spawn();
			}
		}
    }

    public override void _Process(double delta)
    {
        UpdateAnimation();
    }

	/// <summary>
	/// Method that starts the spawning event.
	/// </summary>
	private void StartSpawning()
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

		if (_currentActiveEnemiesCount >= _maxActiveEnemiesCount)
		{
			return;
		}

		CurrentState = State.Spawn;

		IncreaseActiveEnemiesCount();
		Level.Current.ChangeEnemyCount(1);
	}

	/// <summary>
	/// Method to that actually spawns the enemy.
	/// Call this when the animation is at the correct frame.
	/// </summary>
	private void Spawn()
	{
		Enemy enemy = _enemyScene.Instantiate<Enemy>();

		GetParent().AddChild(enemy);
		enemy.GlobalTransform = GlobalTransform;

		if (_spawnPoint != null)
		{
			enemy.SpawnPoint = _spawnPoint.GlobalPosition;
		}

		enemy.SetHomeSpawner(this);
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

		if (_health <= 0 && IsAlive)
		{
			Destroy();
		}

		if (CurrentState != State.Spawn && IsAlive)
		{
			CurrentState = State.Damaged;
		}

		EmitSignal(SignalName.SpawnerDamaged);
	}

    private void Destroy()
    {
		IsAlive = false;
        CurrentState = State.Defeat;
		Level.Current.ChangeSpawnerCount();
    }

	public void IncreaseActiveEnemiesCount()
	{
		_currentActiveEnemiesCount++;
	}

	public void DecreaseActiveEnemiesCount()
	{
		_currentActiveEnemiesCount--;
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