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

	[Export(PropertyHint.Range, "10, 200, 1")]
	private float _attackRange;
	[Export] private EnemyType _enemyType = EnemyType.Stupid;

	[ExportGroup("Nodes")]
	[Export] private Brain _brain;
	[Export] private Sprite2D _sprite;
	[Export] private Area2D _detectionArea;
	[Export] private CollisionShape2D _detectionShape;
	[Export] private NavigationAgent2D _agent;

	public float Health
	{
		get { return _health; }

	}

	public float AttackRange
	{
		get { return _attackRange; }
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
		Battlecry,
		Patrol,
		Chase,
		Attack,
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
	}

	public override void _ExitTree()
    {
        if (_detectionArea != null)
		{
			_detectionArea.BodyEntered -= OnBodyEntered;
			_detectionArea.BodyExited -= OnBodyExited;
		}
    }
	public override void _PhysicsProcess(double delta)
	{
		StateMachine();
		GD.Print(CurrentState);
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

		Velocity = velocity;
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
		// Play animation.
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
	}
}
