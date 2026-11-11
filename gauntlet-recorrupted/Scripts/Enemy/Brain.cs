using Godot;
using System;

public partial class Brain : Node
{
	[ExportCategory("States")]
	[Export] public State StartingState = State.Idle;

	[ExportGroup("Interaction with Player")]
	[Export] public State EntersAreaState = State.None;

	[ExportGroup("After Damaged")]
	[Export] public State InCombatState = State.None;
	[Export] public State OutOfCombatState = State.None;

	[ExportGroup("Notice")]
	[Export] public State AfterNoticeState = State.None;

	[ExportGroup("Awake")]
	[Export] public State AfterAwakeState = State.None;

	[ExportGroup("Attack")]
	[Export] public State AfterAttackState = State.None;
	[Export] public CombatState AfterAttackCombatState = CombatState.InCombat;

	[ExportGroup("Patrol")]
	[Export] public State NavigationFinishedState = State.None;

	[ExportGroup("Spawn")]
	[Export] public State AfterSpawnState = State.None;

	[ExportGroup("Alarmed")]
	[Export] public State AfterAlarmedState = State.None;
	[ExportGroup("Waked Up")]
	[Export] public State AfterWakedUp = State.None;
	[ExportGroup("Detect Bullet")]
	[Export] public State AfterBulledDetected = State.None;

	[ExportGroup("Can't Reach the Target")]
	[Export] public State CantReachTargetState = State.None;
	[ExportGroup("After Idle timer timeout")]
	[Export] public State FromIdleTo = State.None;

	[ExportGroup("Ranged")]
	[Export] public State AfterRangedState = State.None;

	private Enemy _enemy;
	public enum State
	{
		None,
		Spawning,
		Patrol,
		Idle,
		Notice,
		Awake,
		Chase,
		Attack,
		Ranged,
		Damaged,
		Defeat
	}
	public enum CombatState
	{
		OutOfCombat,
		InCombat
	}

	public State CurrentState = State.Idle;
	public CombatState CurrentCombatState = CombatState.OutOfCombat;
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();

		CurrentState = StartingState;

		if (StartingState == State.Idle)
		{
			_enemy.EnterIdle();
		}
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

			if (_enemy.ThisAttackType == Enemy.AttackType.Static)
				{
					return;
				}

			if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) < _enemy.AttackRange && _enemy.HasLineOfSight())
			{
				CurrentState = State.Attack;

				if (_enemy.ThisAttackType == Enemy.AttackType.Dash)
					{
						_enemy.SetAttackDirection();
					}
			}

			if (_enemy.Player != null && _enemy.CanRangedAttack)
			{
				if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) > _enemy.GetDetectionRange() && _enemy.HasLineOfSight())
				{
					CurrentState = State.Ranged;
				}
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

		GameManager.Instance.OnKillsChanged(_enemy.ThisEnemyType.ToString());
	}

	public void AfterDamaged()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(OutOfCombatState);

			_enemy.SetPlayerTarget();

			CurrentCombatState = CombatState.InCombat;
		}
		else if (CurrentCombatState == CombatState.InCombat)
		{
			SetCurrentState(InCombatState);

			_enemy.SetPlayerTarget();
		}
	}

	public void AfterNotice()
	{
		CurrentCombatState = CombatState.InCombat;

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

	public void AfterAwake()
	{
		CurrentState = AfterAwakeState;
	}

	public void AfterAttack()
	{
		if (_enemy.Player != null)
		{
			SetCurrentState(AfterAttackState);

			CurrentCombatState = AfterAttackCombatState;
		}
	}

	public void AfterRanged()
	{
		SetCurrentState(AfterRangedState);
	}

	public void AfterSpawn()
	{
		SetCurrentState(AfterSpawnState);
	}

	public void Alarmed()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(AfterAlarmedState);

			_enemy.SetPlayerTarget();
		}
	}

	public void BulletDetected()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(AfterBulledDetected);

			_enemy.SetPlayerTarget();
		}
	}

	public void IdleTimerTimedOut()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(FromIdleTo);
		}
	}

	public void GetWakedUp()
	{
		SetCurrentState(AfterWakedUp);
	}

	public void AnimationFinished()
	{
		if (_enemy.Sprite.Animation == "Spawn")
		{
			AfterSpawn();
		}

		if (_enemy.Sprite.Animation == "Notice")
		{
			AfterNotice();
		}

		if (_enemy.Sprite.Animation == "Awake")
		{
			AfterAwake();
		}

		if (_enemy.Sprite.Animation == "TakeDmg")
		{
			AfterDamaged();
		}

		if (_enemy.Sprite.Animation == "Attack")
		{
			if (_enemy.AttackSucceed == false && _enemy.ThisEnemyType == Enemy.EnemyType.Tick)
			{
				_enemy.Sprite.Play("AttackFail");
				return;
			}

			AfterAttack();
		}

		if (_enemy.Sprite.Animation == "AttackFail")
		{
			AfterAttack();
		}

		if (_enemy.Sprite.Animation == "AttackHit")
		{
			AfterAttack();
		}

		if (_enemy.Sprite.Animation == "Ranged")
		{
			AfterRanged();
		}
	}

	public void NavigationFinished()
	{
		if (CurrentState == State.Patrol)
		{
			SetCurrentState(NavigationFinishedState);
		}
	}

	public void TargetIsNotReachable()
	{
		CurrentState = CantReachTargetState;
	}

	public void SetCurrentState(State state)
	{
		if (state != State.None)
		{
			CurrentState = state;

			_enemy.OnStateChanged(CurrentState);
		}
	}

	public void PlayerEntered()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			CurrentState = EntersAreaState;
			_enemy.SetPlayerTarget();
		}
	}
}
