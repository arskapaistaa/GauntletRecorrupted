using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] private EnemyType _enemyType = EnemyType.Roach;
	[Export] private AttackType _attackType = AttackType.Sting;

	[ExportGroup("Stats")]

	[Export(PropertyHint.Range, "1,1000,1")] private float _health;

	[Export(PropertyHint.Range, "100,800,10,suffix:Pixels/s")] private float _maxSpeed;

	[Export(PropertyHint.Range, "0,1600,10,suffix:Pixels/s")] private float _attackSpeed;

	[Export(PropertyHint.Range, "0,1500,100,suffix:Pixels")] public float DetectionRange;

	[Export(PropertyHint.Range, "1, 100, 1")] private int _dmg;

	[Export(PropertyHint.Range, "100,1000,10,suffix:Pixels")] private float _attackRange;

	[Export(PropertyHint.Range, "0,10000,100,suffix:Pixels")] private float _alarmRange;

	[Export(PropertyHint.Range, "0,5000,100,suffix:Pixels")] private float _patrolDistance;

	[Export(PropertyHint.Range, "0,5000,100,suffix:Pixels")] private float _patrolAtleastDistance;

	[ExportGroup("Perks")]
	[ExportSubgroup("Damaged movement")]
	[Export] private bool _canMoveWhileDamaged = false;
	[Export(PropertyHint.Range, "50,1000,10,suffix:Pixels/s")] private float _damagedSpeed = 100.0f;

	[ExportSubgroup("Normal patrol (No Tick)")]
	[Export] private bool _canPatrol = false;
	[Export(PropertyHint.Range, "1,20,1,suffix:seconds")] private float _minIdleTime = 1.0f;

	[Export(PropertyHint.Range, "2,20,1,suffix:seconds")] private float _maxIdleTime = 1.0f;

	[ExportSubgroup("Ranged Attack")]
	[Export] public bool CanRangedAttack = false;
	[Export(PropertyHint.Range, "0.1,5.0f,0.1,suffix:seconds")] private float _rangedCooldownTime = 1.0f;
	[Export] private PackedScene _projectileScene = null;
	[Export] private Marker2D _projectileSpawnPoint = null;

	[ExportSubgroup("Alarmed")]
	[Export] private bool _canBeAlarmed = true;
	[ExportSubgroup("Awake")]
	[Export] private bool _canAwake = false;
	[Export(PropertyHint.Range, "200,2000,100,suffix:pixels")] public float AwakeDetectionRange = 200.0f;
	[Export(PropertyHint.Range, "1,20,1,suffix:seconds")] private float _minAwakeIntervalTime = 1.0f;
	[Export(PropertyHint.Range, "2,30,1,suffix:seconds")] private float _maxAwakeIntervalTime = 1.0f;


	[ExportGroup("Node References")]
	[Export] private Brain _brain = null;
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private CollisionShape2D _characterCollision = null;
	[Export] private Area2D _detectionArea = null;
	[Export] private DamageArea _dmgArea = null;
	[Export] private CollisionShape2D _detectionShape = null;
	[Export] private NavigationAgent2D _agent = null;
	[Export] private RayCast2D _rayCast = null;

	[ExportSubgroup("Audios")]
	[Export] private AudioStreamPlayer2D _movementSfx = null;
	[Export] private AudioStreamPlayer2D _noticeSfx = null;
	[Export] private AudioStreamPlayer2D _attackSfx = null;
	[Export] private AudioStreamPlayer2D _damagedSfx = null;
	[Export] private AudioStreamPlayer2D _defeatSfx = null;

	[ExportSubgroup("Particles")]
	[Export] private GpuParticles2D _damagedParticles = null;

	[ExportSubgroup("Timers")]
	[Export] private Timer _destroyTimer = null;
	[Export] private Timer _fromIdleToTimer = null;
	[Export] private Timer _rangedCooldownTimer = null;


	[ExportGroup("Animations")]
	[Export] private int[] _attackMovementFrames;
	[Export] private int _attackStartFrame;
	[Export] private int _attackEndFrames;

	// PRIVATE, NO EXPORT
	private bool _isAlive = true;
	private float _navTimer = 0;
	private float _navInterval = 0.25f;
	private float _attackNavInterval = 0.5f;
	private EnemySpawner _homeSpawner = null;
	private bool _canShoot = true;

	// PUBLIC, NO EXPORT
	public bool HasRoute = false;
	public bool AttackSucceed = false;
	// For Leech attack
	public Vector2 AttackDirection;
	public enum EnemyType
	{
		Roach,
		Tick,
		Baby,
		Adult,
		Worm,
		Bear
	}

	public enum AttackType
	{
		Sting,
		Dash,
		Rush,
		Static
	}

	public EnemyType ThisEnemyType
	{
		get { return _enemyType; }
	}

	public AttackType ThisAttackType
	{
		get { return _attackType; }
	}

	public float Health
	{
		get { return _health; }

	}

	public float MaxSpeed
	{
		get {return _maxSpeed; }
	}

	public float AttackSpeed
	{
		get {return _attackSpeed; }
	}

	public int Dmg
	{
		get { return _dmg; }
	}

	public float AttackRange
	{
		get { return _attackRange; }
	}

	public bool IsAlive
	{
		get { return _isAlive; }
	}

	public AnimatedSprite2D Sprite
	{
		get { return _sprite; }
	}

	public Brain Brain
	{
		get { return _brain; }
	}

	public PlayerCharacter Player;

	public Vector2 PlayerPosition
	{
		get { return Player.GlobalPosition; }
	}

	public Vector2 SpawnPoint;

	public override void _Ready()
	{
		if (_brain == null)
		{
			GD.PrintErr("No brains, die biatch");
			QueueFree();
		}

		Reset();

		ConnectSignals();
	}

    public override void _ExitTree()
    {
        if (_detectionArea != null)
		{
			_detectionArea.BodyEntered -= OnBodyEntered;
			_detectionArea.BodyExited -= OnBodyExited;

			_detectionArea.AreaEntered -= OnAreaEntered;
		}

		if (_sprite != null)
		{
			_sprite.AnimationFinished -= OnAnimationFinished;
		}

		if (_agent != null)
		{
			_agent.VelocityComputed -= OnVelocityComputed;
			_agent.NavigationFinished -= OnNavigationFinished;
		}

		if (_homeSpawner != null)
		{
			_homeSpawner.SpawnerDamaged -= OnSpawnerDamaged;
		}

		if (_destroyTimer != null)
		{
			_destroyTimer.Timeout -= OnDestroyTimerTimedOut;
		}

		if (_fromIdleToTimer != null)
		{
			_fromIdleToTimer.Timeout -= OnIdleTimerTimedOut;
		}

		if (_rangedCooldownTimer != null)
		{
			_rangedCooldownTimer.Timeout -= OnRangedCooldownTimerTimedOut;
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		StateMachine(delta);

		UpdateAnimation();

		UpdateAudio();
	}

	public void StateMachine(double delta)
	{
		switch(_brain.CurrentState)
		{
			case Brain.State.Spawning:
				UpdateSpawn(delta);
				break;

			case Brain.State.Chase:
				UpdateChase(delta);
				break;

			case Brain.State.Patrol:
				UpdatePatrol(delta);
				break;

			case Brain.State.Attack:
				UpdateAttack(delta);
				break;

			case Brain.State.Ranged:
				UpdateRanged();
				break;

			case Brain.State.Damaged:
				UpdateDamaged(delta);
				break;
		}
	}

	public void EnterIdle()
	{
		if (_fromIdleToTimer != null)
		{
			if (_fromIdleToTimer.IsStopped() == true && _canPatrol)
			{
				_fromIdleToTimer.WaitTime = RandomWaitTime(_minIdleTime, _maxIdleTime);
				_fromIdleToTimer.Start();
			}
			else if (_fromIdleToTimer.IsStopped() == true && _canAwake)
			{
				_fromIdleToTimer.WaitTime = RandomWaitTime(_minAwakeIntervalTime, _maxAwakeIntervalTime);
				_fromIdleToTimer.Start();
			}
			else
			{
				return;
			}
		}
	}

    public void UpdateChase(double delta)
	{
		if (_agent == null)
		{
			return;
		}

		if (_agent.IsNavigationFinished())
		{
			// TODO: Call brain.
			return;
		}

		Navigation(delta, _maxSpeed, _navInterval);
		LookAtPlayer();
	}

	private void UpdatePatrol(double delta)
    {
        if (_agent == null)
		{
			return;
		}

		if (_agent.IsNavigationFinished())
		{
			return;
		}

		Navigation(delta, _maxSpeed, _navInterval);
		LookAtPosition();
    }

	public void EnterPatrol()
	{
		SetPatrolTarget();
	}

	private void UpdateSpawn(double delta)
	{
 		if (_agent == null)
		{
			return;
		}

		_agent.TargetPosition = SpawnPoint;

		if (_agent.IsNavigationFinished())
		{
			return;
		}

		Navigation(delta, _maxSpeed, _navInterval);
		LookAtPosition();
	}

	public void Navigation(double delta, float speed, float interval)
	{
		if (_brain.CurrentState == Brain.State.Chase || ThisAttackType == AttackType.Rush)
		{
			NavUpdate(delta, interval);
		}

		Vector2 currentPosition = GlobalPosition;

		Vector2 nextPathPosition = _agent.GetNextPathPosition();
		Vector2 direction = (nextPathPosition - currentPosition).Normalized();

		Vector2 velocity = direction * speed;

		if (_agent.AvoidanceEnabled)
		{
			Velocity = velocity;
		}
		else
		{
			OnVelocityComputed(velocity);
		}

		MoveAndSlide();
	}

	public void LookAtPlayer()
	{
		LookAt(PlayerPosition);
	}

	public void SetPlayerTarget()
	{
		if (Player == null)
		{
			Player = Level.Current.Player;
			Level.Current.IncreaseIntensity(Config.EnemyNoticeValue);
		}

		_agent.TargetPosition = PlayerPosition;

		if (!_agent.IsTargetReachable())
		{
			_brain.TargetIsNotReachable();
		}
	}

	public void SetAttackDirection()
	{
		if (Player == null)
		{
			Player = Level.Current.Player;
		}

		AttackDirection = (PlayerPosition - GlobalPosition).Normalized();
	}

	public void SetPatrolTarget()
	{
		// _agent.TargetPosition = GlobalPosition + RandomPosition(_patrolDistance, _patrolAtleastDistance);
		// Currently hard coded on Layer 1 so WURM wont work.
		_agent.TargetPosition = NavigationServer2D.MapGetRandomPoint(_agent.GetNavigationMap(), 1, true);

		HasRoute = true;

		if (!_agent.IsTargetReachable())
		{
			_brain.TargetIsNotReachable();
			HasRoute = false;
		}
	}

	public void LookAtPosition()
	{
		LookAt(_agent.GetNextPathPosition());
	}

	/// <summary>
	/// Helper method to get random position for patrolling.
	/// </summary>
	/// <param name="min">Min value</param>
	/// <param name="max">Max value</param>
	/// <param name="offset">If you want to have like move atleast 200px </param>
	/// <returns></returns>
	public Vector2 RandomPosition(float distance, float offset = default)
	{
		float x = (float)GD.RandRange(-distance - offset, distance + offset);
		float y = (float)GD.RandRange(-distance - offset, distance + offset);

		Vector2 position = new Vector2(x, y);

		// If random position is not on baked region, find closest point.
		Vector2 validPosition = NavigationServer2D.MapGetClosestPoint(_agent.GetNavigationMap(), position);

		return validPosition;
	}

	public void NavUpdate(double delta, float interval)
	{
		if (_navTimer <= 0.0f)
		{
			SetPlayerTarget();
			_navTimer = interval;
			return;
		}
		else
		{
			_navTimer -= (float)delta;
		}
	}

	public void UpdateAttack(double delta)
	{
		UpdateDamageArea();

		if (ThisAttackType == AttackType.Dash)
		{
			if (CheckAttackAnimation())
			{
				DashAttackMovement(_attackSpeed);
			}

			if (ThisEnemyType == EnemyType.Tick)
			{
				if (_sprite.Animation == "AttackFail")
				{
					DashAttackMovement(_maxSpeed);
				}
			}
		}

		if (ThisAttackType == AttackType.Rush)
		{
			if (CheckAttackAnimation())
			{
				RushAttackMovement(delta, AttackSpeed);
			}
		}

		if (_sprite.Animation == "Attack")
		{
			if (_attackSfx.Playing == false)
			{
				_attackSfx.Play();
			}
		}
	}

	public void DashAttackMovement(float speed)
	{
		Vector2 velocity = AttackDirection * speed;

		Velocity = velocity;
		MoveAndSlide();
	}

	public void RushAttackMovement(double delta, float speed)
	{
		if (_agent == null)
		{
			return;
		}

		if (_agent.IsNavigationFinished())
		{
			// TODO: Call brain.
			return;
		}

		Navigation(delta, speed, _attackNavInterval);
		LookAtPosition();
	}

	/// <summary>
	/// Method that checks if the Attack animation is currently on attack position.
	/// </summary>
	private void UpdateDamageArea()
	{
		if (_sprite.Animation == "Attack")
        {
			if (_sprite.Frame == _attackStartFrame)
			{
				if (_dmgArea != null)
				{
					_dmgArea.Monitoring = true;
				}
			}

			if (_sprite.Frame == _attackEndFrames)
			{
				if (_dmgArea != null)
				{
					_dmgArea.Monitoring = false;
				}
			}
        }
	}

	/// <summary>
	/// Helper method to check if attack animation is "damage" state.
	/// Good to use in attack navigation.
	/// </summary>
	/// <returns></returns>
	private bool CheckAttackAnimation()
	{
		if (_sprite.Animation == "Attack")
        {
			for (int x = 0; x < _attackMovementFrames.Length; x++)
			{
				if (_sprite.Frame == _attackMovementFrames[x])
				{
					return true;
				}
			}
			return false;
        }
		return false;

	}

	private void UpdateRanged()
	{
		LookAtPlayer();

		if (HasLineOfSight() && _canShoot)
		{
			Shoot();
			_canShoot = false;
		}
	}

	private void Shoot()
	{
		if (_projectileScene == null)
		{
			GD.PrintErr("There is no projectile reference on enemy");
			return;
		}

		if (_rangedCooldownTimer != null)
		{
			_rangedCooldownTimer.WaitTime = _rangedCooldownTime;
			_rangedCooldownTimer.Start();
		}

		Vector2 direction = GlobalTransform.X.Normalized();

		Projectile projectile = _projectileScene.Instantiate<Projectile>();
		GetTree().CurrentScene.AddChild(projectile);

		if (_projectileSpawnPoint != null)
		{
			projectile.GlobalPosition = _projectileSpawnPoint.GlobalPosition;
		}

		projectile.Initialize(direction);
	}

	private void UpdateDamaged(double delta)
	{
		if (_canMoveWhileDamaged && _brain.CurrentCombatState == Brain.CombatState.InCombat)
		{
			Navigation(delta, _damagedSpeed, _navInterval);
		}
	}

	private void UpdateAnimation()
	{
		if (_sprite == null)
		{
			GD.PrintErr("Animation sprite cant be found");
			return;
		}

		if (_sprite.Animation == "Defeat")
		{
			return;
		}

		switch (_brain.CurrentState)
		{
			case Brain.State.Spawning:
				_sprite.Play("Spawn");
				break;

			case Brain.State.Idle:
				_sprite.Play("Idle");
				break;

			case Brain.State.Chase:
				_sprite.Play("Walk");
				break;

			case Brain.State.Patrol:
				_sprite.Play("Walk");
				break;

			case Brain.State.Notice:
				_sprite.Play("Notice");
				break;

			case Brain.State.Awake:
				_sprite.Play("Awake");
				break;

			case Brain.State.Attack:
				if (AttackSucceed && ThisEnemyType == EnemyType.Tick)
				{
					_sprite.Play("AttackHit");
				}

				if (ThisEnemyType == EnemyType.Tick &&
					_sprite.Animation != "AttackFail" && _sprite.Animation != "AttackHit")
				{
					_sprite.Play("Attack");
				}
				else if (ThisEnemyType != EnemyType.Tick)
				{
					_sprite.Play("Attack");
				}
				break;

			case Brain.State.Ranged:
				if (CanRangedAttack)
				{
					_sprite.Play("Ranged");
				}
				break;

			case Brain.State.Damaged:
				_sprite.Play("TakeDmg");
				break;

			case Brain.State.Defeat:
				_sprite.Play("Defeat");
				break;
		}
	}

	public void UpdateAudio()
	{
		if (_brain.CurrentState == Brain.State.Chase || _brain.CurrentState == Brain.State.Patrol)
		{
			if (_movementSfx.Playing == false)
			{
				_movementSfx.PitchScale = (float)GD.RandRange(0.9f, 1.1f);
				_movementSfx.VolumeDb = (float)GD.RandRange(0.8f, 1.0f);
				_movementSfx.Play();
			}
		}

		switch (_sprite.Animation)
		{
			case "Attack":
				if (_attackSfx != null && _attackSfx.Playing == false)
				{
					_attackSfx.Play();
				}
				break;

			case "Notice":
				if (_noticeSfx != null && _noticeSfx.Playing == false)
				{
					_noticeSfx.Play();
				}
				break;
		}
	}

	public void TakeDmg(int dmg)
	{
		_brain.Damaged();

		if (_damagedParticles != null)
		{
			_damagedParticles.Restart();
		}

		if (IsAlive)
		{
			_health -= dmg;
		}

		// Defeat
		if (Health <= 0)
		{
			Defeat();
			return;
		}

		if (_damagedSfx != null)
		{
			_damagedSfx.Play();
		}

		HasRoute = false;
	}

	private void Defeat()
	{
		if (_characterCollision != null)
		{
			_characterCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		}

		if (_homeSpawner != null)
		{
			_homeSpawner.DecreaseActiveEnemiesCount();
		}

		if (_destroyTimer != null)
		{
			_destroyTimer.Start();
		}

		_brain.Defeat();

		// Stop monitoring the damage area to prevent bugs.
		if (ThisEnemyType != EnemyType.Worm)
		{
			_dmgArea.Monitoring = false;
		}

		if (Player != null)
		{
			Level.Current.DecreaseIntensity(Config.EnemyDeathValue);
		}

		if (_defeatSfx != null)
		{
			_defeatSfx.Play();
		}
	}

	public void PlaySingleAudio()
	{

	}

	public void AlarmBuddies()
	{
		// THIS CAN BE CHANGED THAT IT ALARM EVERY ONE WITH SAME SPAWNER
		// OR IN SAME "FACTION"
		foreach (Node enemy in GetTree().GetNodesInGroup("Enemy"))
		{
			if (enemy is Enemy enemyNode && enemy != this && enemyNode.ThisEnemyType == ThisEnemyType && enemyNode._canBeAlarmed)
			{
				if (GlobalPosition.DistanceTo(enemyNode.GlobalPosition) <= _alarmRange)
				{
					enemyNode.Alarmed();
				}
			}
		}
	}

	private void Alarmed()
	{
		_brain.Alarmed();
	}

	public void WakeUp()
	{
		_brain.GetWakedUp();
	}

	public bool HasLineOfSight()
	{
		if (_rayCast != null)
		{
			if (Player != null)
			{
				_rayCast.TargetPosition = _rayCast.ToLocal(PlayerPosition);
			}

			if (_rayCast.IsColliding())
			{
				GodotObject node = _rayCast.GetCollider();

				if (node == Player)
				{
					return true;
				}
			}
			else
			{
				return false;
			}

		}
		else
		{
			GD.PrintErr("Enemy is missing RayCast2D");
		}

		return false;
	}

	public void SetHomeSpawner(EnemySpawner spawner)
	{
		if (_homeSpawner == null)
		{
			_homeSpawner = spawner;
			_homeSpawner.SpawnerDamaged += OnSpawnerDamaged;
		}
	}

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			SetPlayerTarget();
			_brain.PlayerEntered();
		}
    }

	private void OnAreaEntered(Area2D area)
    {
        if (area is Bullet)
		{
			_brain.BulletDetected();
		}
    }

	private void OnBodyExited(Node2D body)
    {
		// Hmmm...
    }

	private void OnAnimationFinished()
    {
		_brain.AnimationFinished();
    }

	private void OnVelocityComputed(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
		MoveAndSlide();
    }

	private void OnNavigationFinished()
    {
        _brain.NavigationFinished();

		HasRoute = false;
    }

	private void OnSpawnerDamaged()
    {
        Alarmed();
    }

	private void OnAttackSucceed()
    {
        AttackSucceed = true;
    }

	// Helper method to change detecytion range.
	public void SetDetectionRange(float range)
	{
		if (_detectionShape.Shape is CircleShape2D circle)
		{
			circle.Radius = range;
		}
	}

	public float GetDetectionRange()
	{
		return DetectionRange;
	}
	public void Reset()
	{
		if (_detectionShape == null)
		{
			return;
		}

		if (_detectionShape.Shape is CircleShape2D circle)
		{
			circle.Radius = DetectionRange;
		}

		if (_dmgArea != null && ThisAttackType != AttackType.Static)
		{
			_dmgArea.Monitoring = false;
		}

		_navTimer = _navInterval;
	}

	private void ConnectSignals()
	{
		if (_detectionArea != null)
		{
			_detectionArea.BodyEntered += OnBodyEntered;
			_detectionArea.BodyExited += OnBodyExited;

			_detectionArea.AreaEntered += OnAreaEntered;
		}

		if (_dmgArea != null)
		{
			_dmgArea.AttackSucceed += OnAttackSucceed;
		}

		if (_sprite != null)
		{
			_sprite.AnimationFinished += OnAnimationFinished;
		}

		if (_agent != null)
		{
			_agent.VelocityComputed += OnVelocityComputed;
			_agent.NavigationFinished += OnNavigationFinished;
		}

		if (_destroyTimer != null)
		{
			_destroyTimer.Timeout += OnDestroyTimerTimedOut;
		}

		if (_fromIdleToTimer != null)
		{
			_fromIdleToTimer.Timeout += OnIdleTimerTimedOut;
		}

		if (_rangedCooldownTimer != null)
		{
			_rangedCooldownTimer.Timeout += OnRangedCooldownTimerTimedOut;
		}
	}

    private void OnIdleTimerTimedOut()
    {
		_brain.IdleTimerTimedOut();
    }

	private void OnRangedCooldownTimerTimedOut()
    {
        _canShoot = true;
		GD.Print("Can shoot");
    }

	private double RandomWaitTime(float min, float max)
	{
		double waitTime = GD.RandRange(min, max);
		return waitTime;
	}

    private void OnDestroyTimerTimedOut()
    {
        QueueFree();
    }
}
