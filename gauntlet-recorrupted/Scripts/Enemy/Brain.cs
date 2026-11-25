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

	#region Currents
	public State CurrentState = State.Idle;
	public CombatState CurrentCombatState = CombatState.OutOfCombat;
	#endregion Currents
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();

		SetCurrentState(StartingState);
	}

	#region Called from Enemy's _Process
	/// <summary>
	/// What to do when player is in attack distance.
	/// </summary>
	public void CanAttack()
	{
		SetCurrentState(CanAttackThePlayerState);
	}

	/// <summary>
	/// What to do when player is in ranged attack distance.
	/// </summary>
	public void CanRangedAttack()
	{
		SetCurrentState(CanRangedAttackState);
	}
	#endregion Called from Enemy's _Process

	#region Called from enemy's methods
	/// <summary>
	/// What happens after enemy gets damaged
	/// </summary>
	public void Damaged()
	{
		if (_enemy.IgnoreStateChange && CurrentCombatState == CombatState.InCombat)
		{
			return;
		}
		else
		{
			SetCurrentState(State.Damaged);
		}
	}

	/// <summary>
	/// What happens when enemy dies.
	/// Also tell for Level and Gamemanager that enemy is died.
	/// </summary>
	public void Defeat()
	{
		// Extra safety for Shotgun bullets
		if (CurrentState != State.Defeat)
		{
			SetCurrentState(State.Defeat);

			Level.Current.ChangeEnemyCount(-1);
			GameManager.Instance.OnKillsChanged(_enemy.ThisEnemyType.ToString());
		}
	}

	/// <summary>
	/// What happens when enemy get alarmed.
	/// </summary>
	public void Alarmed()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(AfterAlarmedState);
		}
	}

	/// <summary>
	/// What happens when enemy detects a bullet.
	/// </summary>
	public void BulletDetected()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(AfterBulledDetectedState);
		}
	}

	/// <summary>
	/// If enemy haves idle timer, what happens when it's timedout.
	/// </summary>
	public void IdleTimerTimedOut()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(FromIdleToState);
		}
	}

	/// <summary>
	/// If enemy can be waked up, what happens then.
	/// </summary>
	public void GetWakedUp()
	{
		SetCurrentState(AfterWakedUpState);
	}

	/// <summary>
	/// What to do when enemy reaches the navigation target.
	/// </summary>
	public void NavigationFinished()
	{
		if (CurrentState == State.Patrol)
		{
			SetCurrentState(NavigationFinishedState);
		}
	}

	/// <summary>
	/// What to do if target (Player or patrol target) can't be reached.
	/// </summary>
	public void TargetIsNotReachable()
	{
		SetCurrentState(CantReachTargetState);
	}

	/// <summary>
	/// What happens when player enters the detection area.
	/// Change state only if enemy is out of the combat.
	/// </summary>
	public void PlayerEntered()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(PlayerEntersAreaState);
		}
	}

	#endregion Called from enemy's methods

	#region After animation is finished
	/// <summary>
	/// What happens when enemy gets damaged.
	/// </summary>
	public void AfterDamaged()
	{
		if (CurrentCombatState == CombatState.OutOfCombat)
		{
			SetCurrentState(OutOfCombatDamagedState);
		}
		else if (CurrentCombatState == CombatState.InCombat)
		{
			SetCurrentState(InCombatDamagedState);
		}
	}

	/// <summary>
	/// What happens after enemy is noticed the player.
	/// </summary>
	public void AfterNotice()
	{
		SetCurrentState(AfterNoticeState);
	}

	/// <summary>
	/// What happens after enemy lefts awake state.
	/// </summary>
	public void AfterAwake()
	{
		SetCurrentState(AfterAwakeState);
	}

	/// <summary>
	/// What to do after attack state.
	/// </summary>
	public void AfterAttack()
	{
		SetCurrentState(AfterAttackState);
	}

	/// <summary>
	/// What to do after ranged attack state.
	/// </summary>
	public void AfterRanged()
	{
		SetCurrentState(AfterRangedState);
	}

	/// <summary>
	/// What to do after spawning is finished.
	/// </summary>
	public void AfterSpawn()
	{
		SetCurrentState(AfterSpawnState);
	}

	#endregion After animation is finished

	#region Set new State
	/// <summary>
	/// Changes the current state to new state.
	/// Handles also the Combat state.
	/// </summary>
	/// <param name="state">The next state</param>
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

		if (state == State.Chase || state == State.Notice || state == State.Damaged)
		{
			CurrentCombatState = CombatState.InCombat;
		}
		else if (state == State.Idle || state == State.Patrol)
		{
			CurrentCombatState = CombatState.OutOfCombat;
		}
	}

	/// <summary>
	/// Takes random state from the random state list.
	/// </summary>
	/// <returns>Random state</returns>
	private State RandomizedState()
	{
		State state = NextRandomStateList.PickRandom();
		return state;
	}
	#endregion Set new State
}
