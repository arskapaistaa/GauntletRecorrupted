using Godot;
using System;

public partial class Brain : Node
{
	[ExportCategory("States")]
	[Export] public State StartingState = State.Idle;

	[ExportGroup("Randomized behaviour")]
	[Export] public bool _isBehaviourRandomized = false;
	[Export] public Godot.Collections.Array<State> AfterThisStateGetRandomizedList { get; set; }
	[Export] public Godot.Collections.Array<State> NextRandomStateList { get; set; }

	[ExportGroup("Interaction with Player")]
	[Export] public State PlayerEntersAreaState = State.None;
	[Export] public State CanAttackThePlayerState = State.None;
	[ExportSubgroup("If enemy can Ranged Attack")]
	[Export] public State CanRangedAttackState = State.None;

	[ExportSubgroup("After Damaged")]
	[Export] public State InCombatDamagedState = State.None;
	[Export] public State OutOfCombatDamagedState = State.None;
	[ExportSubgroup("Detect Bullet")]
	[Export] public State AfterBulledDetectedState = State.None;

	[ExportGroup("Own behaviour")]
	[ExportSubgroup("Notice")]
	[Export] public State AfterNoticeState = State.None;

	[ExportSubgroup("Attack & Ranged")]
	[Export] public State AfterAttackState = State.None;
	[Export] public CombatState AfterAttackCombatState = CombatState.InCombat;
	[Export] public State AfterRangedState = State.None;

	[ExportSubgroup("Movement")]
	[Export] public State NavigationFinishedState = State.None;
	[Export] public State CantReachTargetState = State.None;

	[ExportSubgroup("Spawn")]
	[Export] public State AfterSpawnState = State.None;

	[ExportSubgroup("Alarmed")]
	[Export] public State AfterAlarmedState = State.None;
	[ExportSubgroup("Waked Up")]
	[Export] public State AfterWakedUpState = State.None;
	[ExportSubgroup("Awake")]
	[Export] public State AfterAwakeState = State.None;

	[ExportSubgroup("After Idle timer timeout")]
	[Export] public State FromIdleToState = State.None;



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

		SetCurrentState(StartingState);
	}

	public void CanAttack()
	{
		SetCurrentState(CanAttackThePlayerState);
	}

	public void CanRangedAttack()
	{
		SetCurrentState(CanRangedAttackState);
	}
	public void Damaged()
	{
		if (_enemy.IgnoreStateChange && CurrentCombatState == CombatState.InCombat)
		{
			return;
		}
		else
		{
			CurrentState = State.Damaged;
		}
	}

	public void Defeat()
	{
		// Extra safety for Shotgun bullets
		if (CurrentState != State.Defeat)
		{
			CurrentState = State.Defeat;

			Level.Current.ChangeEnemyCount(-1);
			GameManager.Instance.OnKillsChanged(_enemy.ThisEnemyType.ToString());
		}
	}

	public void AfterDamaged()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(OutOfCombatDamagedState);

			CurrentCombatState = CombatState.InCombat;
		}
		else if (CurrentCombatState == CombatState.InCombat)
		{
			SetCurrentState(InCombatDamagedState);
		}
	}

	public void AfterNotice()
	{
		CurrentCombatState = CombatState.InCombat;

		SetCurrentState(AfterNoticeState);
	}

	public void AfterAwake()
	{
		SetCurrentState(AfterAwakeState);
	}

	public void AfterAttack()
	{
		CurrentCombatState = AfterAttackCombatState;

		SetCurrentState(AfterAttackState);
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
		}
	}

	public void BulletDetected()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(AfterBulledDetectedState);
		}
	}

	public void IdleTimerTimedOut()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(FromIdleToState);
		}
	}

	public void GetWakedUp()
	{
		SetCurrentState(AfterWakedUpState);
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
		SetCurrentState(CantReachTargetState);
	}

	public void SetCurrentState(State state)
	{
		if (_isBehaviourRandomized)
		{
			if (AfterThisStateGetRandomizedList.Contains(CurrentState))
			{
				state = RandomizedState();
			}
		}

		if (state != State.None)
		{
			CurrentState = state;

			_enemy.OnStateChanged(CurrentState);
		}
	}

	private State RandomizedState()
	{
		State state = NextRandomStateList.PickRandom();
		return state;
	}

	public void PlayerEntered()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(PlayerEntersAreaState);
		}
	}
}
