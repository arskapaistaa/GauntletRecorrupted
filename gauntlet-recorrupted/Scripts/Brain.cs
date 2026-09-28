using Godot;
using System;

public partial class Brain : Node
{
	private Enemy _enemy;
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();
	}

	public override void _Process(double delta)
	{
		MakeDecision();
	}

	public void Check()
	{
		// TODO: If i want to do this before MakeDecision()
	}
	public void MakeDecision()
	{
		switch(_enemy.CurrentState)
		{
			case Enemy.State.Chase:
				if (_enemy.GlobalPosition.DistanceTo(_enemy.Player.GlobalPosition) < _enemy.AttackRange)
				{
					_enemy.CurrentState = Enemy.State.Attack;
				}
				break;
		}
	}

	public void PlayerEntered()
	{
		// is player alive

		_enemy.SetPlayerTarget();
		_enemy.CurrentState = Enemy.State.Chase; // In future this might be battlecry.
	}
}
