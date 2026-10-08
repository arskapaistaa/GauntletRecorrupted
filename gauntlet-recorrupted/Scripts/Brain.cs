using Godot;
using System;

public partial class Brain : Node
{
	[Export] private EnemyType _enemyType = EnemyType.Roach;
	[Export] private AttackType _attackType = AttackType.Sting;

	[ExportCategory("States")]
	[Export] public State StartingState = State.Idle;

	[ExportGroup("Interaction with Player")]
	[Export] public State EntersAreaState = State.None;

	[ExportGroup("After Damaged")]
	[Export] public State InCombatState = State.None;
	[Export] public State OutOfCombatState = State.None;

	[ExportGroup("Notice")]
	[Export] public State AfterNoticeState = State.None;

	[ExportGroup("Attack")]
	[Export] public State AfterAttackState = State.None;
	[Export] public CombatState AfterAttackCombatState = CombatState.Combat;

	[ExportGroup("Patrol")]
	[Export] public State NavigationFinishedState = State.None;

	[ExportGroup("Spawn")]
	[Export] public State AfterSpawnState = State.None;

	[ExportGroup("Alarmed")]
	[Export] public State AfterAlarmedState = State.None;

	private Enemy _enemy;
	public enum State
	{
		None,
		Spawning,
		Patrol,
		Idle,
		Notice,
		Chase,
		Attack,
		Damaged,
		Defeat
	}
	public enum CombatState
	{
		None,
		Combat
	}

	public enum EnemyType
	{
		Roach,
		Tick,
		Worm
	}

	public enum AttackType
	{
		Sting,
		Leech
	}

	public EnemyType ThisEnemyType
	{
		get { return _enemyType; }
	}

	public AttackType ThisAttackState
	{
		get { return _attackType; }
	}

	public State CurrentState = State.Idle;
	public CombatState CurrentCombatState = CombatState.None;
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();

		CurrentState = StartingState;

	}

	public override void _Process(double delta)
	{
		Check();
	}

	public void Check()
	{
		switch(CurrentState)
		{
		case State.Chase:
			// If enemy is in attack range and can see player, start Attacking.
			if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) < _enemy.AttackRange && _enemy.HasLineOfSight())
			{
				CurrentState = State.Attack;
			}

			break;
		}
	}

	public void Damaged()
	{
		CurrentState = State.Damaged;
	}

	public void Defeat()
	{
		CurrentState = State.Defeat;

		Level.Current.ChangeEnemyCount(-1);

		GameManager.Instance.OnKillsChanged();
	}

	public void AfterDamaged()
	{
		if (_enemy.Player == null)
		{
			SetCurrentState(OutOfCombatState);

			// Maybe change this logic
			_enemy.Player = Level.Current.Player;
			_enemy.SetPlayerTarget();
		}
		else if (_enemy.Player != null)
		{
			SetCurrentState(InCombatState);
			_enemy.SetPlayerTarget();
		}
	}

	public void AfterNotice()
	{
		CurrentCombatState = CombatState.Combat;

		_enemy.AlarmBuddies();

		if (_enemy.Player != null)
		{
			SetCurrentState(AfterNoticeState);
		}
		else
		{
			CurrentState = State.Idle;
		}
	}

	public void AfterAttack()
	{
		if (_enemy.Player != null)
		{
			SetCurrentState(AfterAttackState);

			CurrentCombatState = AfterAttackCombatState;
		}
	}

	public void AfterSpawn()
	{
		SetCurrentState(AfterSpawnState);
	}

	public void Alarmed()
	{
		if (CurrentCombatState == CombatState.None)
		{
			SetCurrentState(AfterAlarmedState);

			// Maybe change this logic
			_enemy.Player = Level.Current.Player;
			_enemy.SetPlayerTarget();
		}
	}

	public void AnimationFinished()
	{
		if (_enemy.Sprite.Animation == "Spawn")
		{
			AfterSpawn();
		}

		if (_enemy.Sprite.Animation == "Defeat")
		{
			_enemy.QueueFree();
		}

		if (_enemy.Sprite.Animation == "Notice")
		{
			AfterNotice();
		}

		if (_enemy.Sprite.Animation == "TakeDmg")
		{
			AfterDamaged();
		}

		if (_enemy.Sprite.Animation == "Attack")
		{
			AfterAttack();
		}
	}

	public void NavigationFinished()
	{
		if (CurrentState == State.Patrol)
		{
			SetCurrentState(NavigationFinishedState);
			_enemy.HasRoute = false;
		}
	}

	public void SetCurrentState(State state)
	{
		if (state != State.None)
		{
			CurrentState = state;
		}
	}

	public void PlayerEntered()
	{
		if (CurrentCombatState == CombatState.None)
		{
			if (EntersAreaState != State.None)
			{
				CurrentState = EntersAreaState;
			}

			_enemy.SetPlayerTarget();
		}
	}
}
