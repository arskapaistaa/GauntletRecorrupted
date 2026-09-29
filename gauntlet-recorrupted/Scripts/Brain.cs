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
				if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) < _enemy.AttackRange)
				{
					_enemy.CurrentState = Enemy.State.Attack;
				}
				break;

			case Enemy.State.Attack:
				if (_enemy.GlobalPosition.DistanceTo(_enemy.PlayerPosition) > _enemy.AttackRange)
				{
					if (_enemy.Player != null)
					{
						_enemy.CurrentState = Enemy.State.Chase;
					}
				}
				break;
		}
	}

	public void AnimationFinished()
	{
		if (_enemy.Sprite.Animation == "Defeat")
		{
			_enemy.QueueFree();
		}

		if (_enemy.Sprite.Animation == "TakeDmg")
		{
			if (_enemy.Player != null)
			{
				_enemy.CurrentState = Enemy.State.Chase;
			}
			else
			{
				_enemy.CurrentState = Enemy.State.Idle;
			}
		}
	}

	public void PlayerEntered()
	{
		_enemy.SetPlayerTarget();
		_enemy.CurrentState = Enemy.State.Chase; // In future this might be battlecry.
	}
}
