using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[ExportGroup("Stats")]

	[Export(PropertyHint.Range, "1, 1000, 1")]
	private float _health;

	[Export(PropertyHint.Range, "100, 800, 10")]
	private float _maxSpeed;

	[Export(PropertyHint.Range, "100, 1600, 10")]
	private float _attackSpeed;

	[Export(PropertyHint.Range, "200, 1500, 100")]
	private float _detectionRange;

	[Export(PropertyHint.Range, "1, 100, 1")]
	private int _dmg;

	[Export(PropertyHint.Range, "100, 600, 10")]
	private float _attackRange;

	[Export(PropertyHint.Range, "300, 10000, 100")]
	private float _alarmRange;

	[ExportGroup("Node References")]
	[Export] private Brain _brain = null;
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private AnimatedSprite2D _shadowSprite = null;
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
	[Export] private int[] _attackFrames;

	// PRIVATE, NO EXPORT
	private bool _isAlive = true;
	private float _navTimer = 0;
	private float _navInterval = 0.25f;
	private float _minPatrolDistance;
	private float _maxPatrolDistance;

	// PUBLIC, NO EXPORT
	public bool HasRoute = false;

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
		get {return _sprite; }
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

		if (_detectionArea != null)
		{
			_detectionArea.BodyEntered += OnBodyEntered;
			_detectionArea.BodyExited += OnBodyExited;
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

    public override void _ExitTree()
    {
        if (_detectionArea != null)
		{
			_detectionArea.BodyEntered -= OnBodyEntered;
			_detectionArea.BodyExited -= OnBodyExited;
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
    }
	public override void _PhysicsProcess(double delta)
	{
		StateMachine(delta);
		UpdateAnimation();
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
			SetPositionTarget();
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
		_agent.TargetPosition = PlayerPosition;
	}

	public void SetPositionTarget()
	{
		// while (!isAllowed)
		_agent.TargetPosition = GlobalPosition + RandomPosition(_minPatrolDistance, _maxPatrolDistance);

		HasRoute = true;
	}

	public void LookAtPosition()
	{
		LookAt(_agent.GetNextPathPosition());
	}

	public Vector2 RandomPosition(float min, float max)
	{
		float x = (float)GD.RandRange(min, max);
		float y = (float)GD.RandRange(min, max);

		Vector2 position = new Vector2(x, y);

		return position;
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

		if (_brain.ThisAttackState == Brain.AttackType.Leech)
		{
			if (CheckAttackAnimation())
			{
				Navigation(delta, _attackSpeed);
			}
		}
	}

	/// <summary>
	/// Method that checks if the Attack animation is currently on attack position.
	/// </summary>
	private void UpdateDamageArea()
	{
		if (_sprite.Animation == "Attack")
        {
			for (int x = 0; x < _attackFrames.Length; x++)
			{
				if (_sprite.Frame == _attackFrames[x])
				{
					if (_dmgArea != null)
					{
						_dmgArea.Monitoring = true;
					}
					else
					{
						_dmgArea.Monitoring = false;
					}
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
			for (int x = 0; x < _attackFrames.Length; x++)
			{
				if (_sprite.Frame == _attackFrames[x])
				{
					return true;
				}
			}
			return false;
        }
		return false;

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
				_sprite.Play("Attack");
				_shadowSprite.Play("Attack");
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

	public void TakeDmg(int dmg)
	{
		_brain.Damaged();

		if (IsAlive)
		{
			_health -= dmg;
		}

		if (Health <= 0)
		{
			_brain.Defeat();
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
	}

	private void Alarmed()
	{
		_brain.Alarmed();
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

	private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter player)
		{
			//TODO: IsAlive etc.
			Player = player;
			_brain.PlayerEntered();
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
    }

	public void SetPatrolDistances(float min, float max)
	{
		_minPatrolDistance = min;
		_maxPatrolDistance = max;
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

		if (_dmgArea != null)
		{
			_dmgArea.Monitoring = false;
		}

		_navTimer = _navInterval;
	}
}
