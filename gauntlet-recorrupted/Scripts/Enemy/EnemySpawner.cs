using Godot;
using System;

public partial class EnemySpawner : StaticBody2D
{
	[ExportGroup("Stats")]
	[Export] private PackedScene _enemyScene = null;
	[Export(PropertyHint.Range, "1,180,1,suffix:seconds")] private float _spawningCooldown = 1.0f;
	[Export(PropertyHint.Range, "10,500,10")] private int _health = 100;

	/// <summary>
	/// How many enemies can be active at the same time, spawned from this spawner.
	/// </summary>
	[Export(PropertyHint.Range, "1,20,suffix:at the same time")] private int _maxActiveEnemiesCount = 5;

	/// <summary>
	/// How close player have to be to start emitting "clue" particles.
	/// </summary>
	[Export(PropertyHint.Range, "100,5000,100:suffix:pixels")] private float _playerDistanceTreshold = 1000.0f;

	[ExportGroup("Node references")]
	[Export] private Timer _spawnTimer = null;
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private GpuParticles2D _takeDmgParticles = null;
	[Export] private AudioStreamPlayer2D _takeDmgSFX = null;
	[Export] private Marker2D _spawnPoint = null; // Can be left null. If you want enemy to go spawn position, then set this.
	[Export] private Area2D _playerDetectionArea = null;

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
	public float CurrentSpawningCooldown;
	public bool IsAlive = true;
	private int _currentActiveEnemiesCount = 0;
	private bool _playerEnteredArea = false;
	private double _playerCheckInterval = 1.0;
	private double _playerCheckTimer = 0.0;


	public override void _Ready()
	{
		if (_spawnTimer == null)
		{
			GD.PrintErr("Spawner has no timer");
			return;
		}

		_spawnTimer.Timeout += OnSpawnTimerTimeout;

		if (_spawnTimer.IsStopped())
		{
			CurrentSpawningCooldown = _spawningCooldown;

			_spawnTimer.WaitTime = CurrentSpawningCooldown;
			_spawnTimer.Start();
		}

		if (_sprite != null)
		{
			_sprite.AnimationFinished += OnAnimationFinished;
			_sprite.FrameChanged += OnFrameChanged;
		}

		if (_playerDetectionArea != null)
		{
			_playerDetectionArea.BodyEntered += OnBodyEntered;
			_playerDetectionArea.BodyExited += OnBodyExited;
		}

		_playerCheckTimer = _playerCheckInterval;
	}

    public override void _ExitTree()
    {
        _spawnTimer.Timeout -= OnSpawnTimerTimeout;

		if (!_spawnTimer.IsStopped())
		{
			_spawnTimer.Stop();
		}

		if (_sprite != null)
		{
			_sprite.AnimationFinished -= OnAnimationFinished;
			_sprite.FrameChanged -= OnFrameChanged;
		}

		if (_playerDetectionArea != null)
		{
			_playerDetectionArea.BodyEntered -= OnBodyEntered;
			_playerDetectionArea.BodyExited -= OnBodyExited;
		}
    }

    public override void _Process(double delta)
    {
        UpdateAnimation();

		if (_playerEnteredArea)
		{
			_playerCheckTimer -= delta;

			if (_playerCheckTimer <= 0)
			{
				CheckDistanceToPlayer();
				_playerCheckTimer = _playerCheckInterval;
			}
		}
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
		enemy.GlobalPosition = GlobalPosition;

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

		if (_takeDmgParticles != null)
		{
			_takeDmgParticles.Restart();
		}

		if (_takeDmgSFX != null)
		{
			_takeDmgSFX.Play();
		}

		EmitSignal(SignalName.SpawnerDamaged);
	}

    private void Destroy()
    {
		IsAlive = false;
        CurrentState = State.Defeat;

		SetParticleAmountRatio(Level.Current.Player.BaseParticleAmountRatio);

		_spawnTimer.QueueFree();

		Level.Current.ChangeSpawnerCount();
		GameManager.Instance.OnSpawnerDestroyedChanged();
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

	/// <summary>
	/// Checks current distance between this spawner and Player.
	/// </summary>
	private void CheckDistanceToPlayer()
	{
		float distance;

		if (Level.Current.Player != null)
		{
			distance = GlobalPosition.DistanceTo(Level.Current.Player.GlobalPosition);
			float multiplier = _playerDistanceTreshold / distance;

			if (distance < _playerDistanceTreshold)
			{
				float veryCloseDistance = 800.0f;
				if (distance < veryCloseDistance)
				{
					// Highest amount possible.
					float maxAmountRatio = 1.0f;
					SetParticleAmountRatio(maxAmountRatio);
					return;
				}
				// To make difference for particle amount ration, lets add 3x multiplier right away.
				int atleastMultiplier = 3;
				SetParticleAmountRatio(Level.Current.Player.BaseParticleAmountRatio * (multiplier + atleastMultiplier));
			}
		}
	}

	/// <summary>
	/// Set Player dust particle amount ratio.
	/// </summary>
	/// <param name="amount">Amount ratio/Multiplier</param>
	private void SetParticleAmountRatio(float amount)
	{
		Level.Current.Player.SetDustParticlesEmittingAmountRatio(amount);
	}

	private void OnSpawnTimerTimeout()
    {
        StartSpawning();
    }

	private void OnPlayerCheckTimerTimeout()
    {
        CheckDistanceToPlayer();
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

	private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			_playerEnteredArea = true;
		}
    }

	private void OnBodyExited(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			_playerEnteredArea = false;
			SetParticleAmountRatio(Level.Current.Player.BaseParticleAmountRatio);
		}
    }
}