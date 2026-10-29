using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] private EnemyType _enemyType = EnemyType.Roach;
	[Export] private AttackType _attackType = AttackType.Sting;

	[ExportGroup("Stats")]

	[Export(PropertyHint.Range, "1,1000,1")]
	private float _health;

	[Export(PropertyHint.Range, "100,800,10,suffix:Pixels/s")]
	private float _maxSpeed;

	[Export(PropertyHint.Range, "0,1600,10,suffix:Pixels/s")]
	private float _attackSpeed;

	[Export(PropertyHint.Range, "0,1500,100,suffix:Pixels")]
	private float _detectionRange;

	[Export(PropertyHint.Range, "1, 100, 1")]
	private int _dmg;

	[Export(PropertyHint.Range, "100,600,10,suffix:Pixels")]
	private float _attackRange;

	[Export(PropertyHint.Range, "0,10000,100,suffix:Pixels")]
	private float _alarmRange;

	[Export(PropertyHint.Range, "0,5000,100,suffix:Pixels")]
	private float _patrolDistance;

	[Export(PropertyHint.Range, "0,5000,100,suffix:Pixels")]
	private float _patrolAtleastDistance;

	[ExportGroup("Perks")]
	[ExportSubgroup("Damaged movement")]
	[Export] private bool _canMoveWhileDamaged = false;
	[Export(PropertyHint.Range, "50,1000,10,suffix:Sixels/s")]
	private float _damagedSpeed = 100.0f;

	[ExportSubgroup("Alarmed")]
	[Export] private bool _canBeAlarmed = true;

	[ExportGroup("Node References")]
	[Export] private Brain _brain = null;
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private AnimatedSprite2D _shadowSprite = null;
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

	[ExportGroup("Animations")]
	[Export] private int[] _attackMovementFrames;
	[Export] private int _attackStartFrame;
	[Export] private int _attackEndFrames;

	// PRIVATE, NO EXPORT
	private bool _isAlive = true;
	private float _navTimer = 0;
	private float _navInterval = 0.25f;
	private EnemySpawner _homeSpawner = null;

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
		Worm
	}

	public enum AttackType
	{
		Sting,
		Leech,
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

			case Brain.State.Damaged:
				UpdateDamaged(delta);
				break;
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

		Navigation(delta, _maxSpeed);
		LookAtPlayer();
	}

	private void UpdatePatrol(double delta)
    {
        if (_agent == null)
		{
			return;
		}

		if (!HasRoute)
		{
			SetPatrolTarget();
		}


		if (_agent.IsNavigationFinished())
		{
			return;
		}

		Navigation(delta, _maxSpeed);
		LookAtPosition();
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

		Navigation(delta, _maxSpeed);
		LookAtPosition();
	}

	public void Navigation(double delta, float speed)
	{
		if (_brain.CurrentState == Brain.State.Chase)
		{
			NavUpdate(delta);
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
		// while (!isAllowed)
		_agent.TargetPosition = GlobalPosition + RandomPosition(_patrolDistance, _patrolAtleastDistance);

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

	public void NavUpdate(double delta)
	{
		if (_navTimer <= 0.0f)
		{
			SetPlayerTarget();
			_navTimer = _navInterval;
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

		if (ThisAttackType == AttackType.Leech)
		{
			if (CheckAttackAnimation())
			{
				AttackMovement(_attackSpeed);
			}

			if (_sprite.Animation == "AttackFail")
			{
				AttackMovement(_maxSpeed);
			}
		}

		if (_brain.CurrentState == Brain.State.Attack)
		{
			if (_attackSfx.Playing == false)
			{
				_attackSfx.Play();
			}
		}
	}

	public void AttackMovement(float speed)
	{
		Vector2 velocity = AttackDirection * speed;

		Velocity = velocity;
		MoveAndSlide();
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

	private void UpdateDamaged(double delta)
	{
		if (_canMoveWhileDamaged && _brain.CurrentCombatState == Brain.CombatState.InCombat)
		{
			Navigation(delta, _damagedSpeed);
		}
	}

	private void UpdateAnimation()
	{
		if (_sprite == null)
		{
			GD.PrintErr("Animation sprite cant be found");
			return;
		}

		if (_shadowSprite == null)
		{
			GD.PrintErr("Shadow animation sprite cant be found");
			return;
		}

		switch (_brain.CurrentState)
		{
			case Brain.State.Spawning:
				_sprite.Play("Spawn");
				_shadowSprite.Play("Spawn");
				break;

			case Brain.State.Idle:
				_sprite.Play("Idle");
				_shadowSprite.Play("Idle");
				break;

			case Brain.State.Chase:
				_sprite.Play("Walk");
				_shadowSprite.Play("Walk");
				break;

			case Brain.State.Patrol:
				_sprite.Play("Walk");
				_shadowSprite.Play("Walk");
				break;

			case Brain.State.Notice:
				_sprite.Play("Notice");
				_shadowSprite.Play("Notice");
				break;

			case Brain.State.Attack:
				if (AttackSucceed)
				{
					_sprite.Play("AttackHit");
					_shadowSprite.Play("AttackHit");
				}

				if (_sprite.Animation != "AttackFail" && _sprite.Animation != "AttackHit")
				{
					_sprite.Play("Attack");
					_shadowSprite.Play("Attack");
				}
				break;

			case Brain.State.Damaged:
				_sprite.Play("TakeDmg");
				_shadowSprite.Play("TakeDmg");
				break;

			case Brain.State.Defeat:
				_sprite.Play("Defeat");
				_shadowSprite.Play("Defeat");
				break;
		}
	}

	public void UpdateAudio()
	{
		if (_brain.CurrentState == Brain.State.Chase || _brain.CurrentState == Brain.State.Patrol)
		{
			if (_movementSfx.Playing == false)
			{
				_movementSfx.Playing = true;
			}

		}
		else
		{
			_movementSfx.Playing = false;
		}
	}

	public void TakeDmg(int dmg)
	{
		_brain.Damaged();

		if (IsAlive)
		{
			_health -= dmg;
		}

		// Defeat
		if (Health <= 0)
		{
			Defeat();
		}

		if (_damagedSfx != null)
		{
			_damagedSfx.Play();
		}

		HasRoute = false;


	}

	private void Defeat()
	{
		if (_defeatSfx != null)
		{
			_defeatSfx.Play();
		}

		if (_characterCollision != null)
		{
			_characterCollision.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		}

		if (_homeSpawner != null)
		{
			_homeSpawner.DecreaseActiveEnemiesCount();
		}

		_brain.Defeat();

		// Stop monitoring the damage area to prevent bugs.
		if (ThisEnemyType != EnemyType.Worm)
		{
			_dmgArea.Monitoring = false;
		}
	}

	public void AlarmBuddies()
	{
		// THIS CAN BE CHANGED THAT IT ALARM EVERY ONE WITH SAME SPAWNER
		// OR IN SAME "FACTION"
		foreach (Node enemy in GetTree().GetNodesInGroup("Enemy"))
		{
			if (enemy is Enemy enemyNode && enemy != this)
			{
				if (GlobalPosition.DistanceTo(enemyNode.GlobalPosition) <= _alarmRange)
				{
					enemyNode.Alarmed();
				}
			}
		}

		if (_noticeSfx != null)
		{
			_noticeSfx.Play();
		}
	}

	private void Alarmed()
	{
		if (_canBeAlarmed)
		{
			_brain.Alarmed();
		}
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
        if (body is PlayerCharacter player)
		{
			//TODO: IsAlive etc.
			Player = player;
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
	public void Reset()
	{
		if (_detectionShape == null)
		{
			return;
		}

		if (_detectionShape.Shape is CircleShape2D circle)
		{
			circle.Radius = _detectionRange;
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
	}
}
