using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[ExportGroup("Stats")]

	[Export(PropertyHint.Range, "1, 100, 1")]
	private float _health;

	[Export(PropertyHint.Range, "10, 400, 10")]
	private float _maxSpeed;

	[Export(PropertyHint.Range, "200, 1500, 100")]
	private float _detectionRange;

	[Export(PropertyHint.Range, "1, 100, 1")]
	private int _dmg;

	[Export(PropertyHint.Range, "100, 400, 10")]
	private float _attackRange;
	[Export] private EnemyType _enemyType = EnemyType.Stupid;

	[ExportGroup("Nodes")]
	[Export] private Brain _brain;
	[Export] private AnimatedSprite2D _sprite;
	[Export] private Area2D _detectionArea;
	[Export] private DamageArea _dmgArea;
	[Export] private CollisionShape2D _detectionShape;
	[Export] private NavigationAgent2D _agent;

	[ExportGroup("Animations")]
	[Export] private int _attackFrame;

	private bool _isAlive = true;

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

	public PlayerCharacter Player
	{
		private set;
		get;
	}
	public enum EnemyType
	{
		Patrol,
		Stupid,
		Worm
	}
	public enum State
	{
		Spawning,
		Idle,
		Notice,
		Patrol,
		Chase,
		Attack,
		Damaged,
		Die
	}

	public State CurrentState = State.Idle;
	public Vector2 PlayerPosition
	{
		get { return Player.GlobalPosition; }
	}

	// Called when the node enters the scene tree for the first time.
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
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		StateMachine();
		UpdateAnimation();
	}


	public void StateMachine()
	{
		switch(CurrentState)
		{
			case State.Chase:
				UpdateChase();
				break;

			case State.Attack:
				UpdateAttack();
				break;
		}
	}

	public void UpdateChase()
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

		Navigation();
		LookAtPlayer();
	}

	public void Navigation()
	{
		SetPlayerTarget();

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
		if ( _sprite == null)
		{
			GD.PrintErr("Animation sprite cant be found");
			return;
		}
		switch (CurrentState)
		{
			case State.Idle:
				_sprite.Play("Idle");
				break;

			case State.Notice:
				_sprite.Play("Notice");
				break;

			case State.Chase:
				_sprite.Play("Walk");
				break;

			case State.Attack:
				_sprite.Play("Attack");
				break;

			case State.Damaged:
				_sprite.Play("TakeDmg");
				break;

			case State.Die:
				_sprite.Play("Defeat");
				break;
		}
	}

	public void TakeDmg(int dmg)
	{
		CurrentState = State.Damaged;

		if (IsAlive)
		{
			_health -= dmg;
		}

		if (Health <= 0)
		{
			Die();
		}
	}

	public void Die()
	{
		CurrentState = State.Die;
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
	}
}
