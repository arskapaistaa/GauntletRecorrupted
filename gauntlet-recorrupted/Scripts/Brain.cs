using Godot;
using System;

public partial class Brain : Node
{
	[ExportCategory("States")]

	[ExportGroup("Interaction with Player")]
	[Export] public State EntersDetectionArea = State.None;

	[ExportGroup("Damaged")]
	[Export] public State InCombat = State.None;
	[Export] public State OutOfCombat = State.None;

	[ExportGroup("Notice")]
	[Export] public State AfterNoticePlayer = State.None;

	[ExportGroup("Attack")]
	[Export] public State AfterAttackPlayer = State.None;
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

	public State CurrentState = State.Idle;
	public CombatState CurrentCombatState = CombatState.None;
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();
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
			if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) < _enemy.AttackRange)
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
	}

	public void AfterDamaged()
	{
		if (_enemy.Player == null)
		{
			if (OutOfCombat != State.None)
			{
				CurrentState = OutOfCombat;
			}
			// Maybe change this logic
			_enemy.Player = Level.Current.Player;
			_enemy.SetPlayerTarget();
		}
		else if (_enemy.Player != null)
		{
			if (InCombat != State.None)
			{
				CurrentState = InCombat;
			}
		}
	}

	public void AfterNotice()
	{
		CurrentCombatState = CombatState.Combat;

		if (_enemy.Player != null)
		{
			if (AfterNoticePlayer != State.None)
			{
				CurrentState = AfterNoticePlayer;
			}
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
			if (AfterAttackPlayer != State.None)
			{
				CurrentState = AfterAttackPlayer;
			}
		}
	}

	public void AnimationFinished()
	{
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

	public void PlayerEntered()
	{
		if (CurrentCombatState == CombatState.None)
		{
			if (EntersDetectionArea != State.None)
			{
				CurrentState = EntersDetectionArea;
			}

			_enemy.SetPlayerTarget();
		}
	}
}
