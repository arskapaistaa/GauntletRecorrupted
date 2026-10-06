using Godot;
using System;
using System.Text.RegularExpressions;


public partial class Enemy : CharacterBody2D
{
	[ExportGroup("Stats")]

	[Export(PropertyHint.Range, "1, 1000, 1")]
	private float _health;

	[Export(PropertyHint.Range, "100, 800, 10")]
	private float _maxSpeed;

	[Export(PropertyHint.Range, "200, 1500, 100")]
	private float _detectionRange;

	[Export(PropertyHint.Range, "1, 100, 1")]
	private int _dmg;

	[Export(PropertyHint.Range, "100, 400, 10")]
	private float _attackRange;

	[Export(PropertyHint.Range, "300, 10000, 100")]
	private float _alarmRange;

	[ExportGroup("Node References")]
	[Export] private Brain _brain;
	[Export] private AnimatedSprite2D _sprite;
	[Export] private AnimatedSprite2D _shadowSprite;
	[Export] private Area2D _detectionArea;
	[Export] private DamageArea _dmgArea;
	[Export] private CollisionShape2D _detectionShape;
	[Export] private NavigationAgent2D _agent;

	[ExportGroup("Animations")]
	[Export] private int _attackFrame;

	private bool _isAlive = true;

	public bool HasRoute = false;
	private float _navTimer = 0;
	private float _navInterval = 0.25f;

	public float Health
	{
		get { return _health; }

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

	[Export] public Vector2 SpawnPoint;

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
				UpdateAttack();
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

		Navigation(delta);
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

		Navigation(delta);
		LookAtPosition(_agent.TargetPosition);
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

		Navigation(delta);
		LookAtPosition(SpawnPoint);
	}

	public void Navigation(double delta)
	{
		if (_brain.CurrentState == Brain.State.Chase)
		{
			NavUpdate(delta);
		}

		Vector2 currentPosition = GlobalPosition;

		Vector2 nextPathPosition = _agent.GetNextPathPosition();
		Vector2 direction = (nextPathPosition - currentPosition).Normalized();

		Vector2 velocity = direction * _maxSpeed;

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
		_agent.TargetPosition = GlobalPosition + RandomPosition(-1000.0f, 1000.0f);

		HasRoute = true;
	}

	public void LookAtPosition(Vector2 position)
	{
		LookAt(position);
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

	public void UpdateAttack()
	{
		if (_sprite.Animation == "Attack")
        {
            if (_sprite.Frame == _attackFrame)
			{
				if (_dmgArea != null)
				{
					_dmgArea.Monitoring = true;
				}
			}
			else
			{
				_dmgArea.Monitoring = false;
			}
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
