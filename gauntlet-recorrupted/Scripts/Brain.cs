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
		Check();
	}

	public void Check()
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
	public void AfterDamaged()
	{
		if (_enemy.Player == null)
		{
			_enemy.CurrentState = Enemy.State.Notice;

			// Maybe change this logic
			_enemy.Player = Level.Current.Player;
			_enemy.SetPlayerTarget();
		}
		else if (_enemy.Player != null)
		{
			_enemy.CurrentState = Enemy.State.Chase;
		}
	}

	public void AfterNotice()
	{
		_enemy.InCombat = true;

		if (_enemy.Player != null)
		{
			_enemy.CurrentState = Enemy.State.Chase;
		}
		else
		{
			_enemy.CurrentState = Enemy.State.Idle;
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
	}

	public void PlayerEntered()
	{
		if (!_enemy.InCombat)
		{
			_enemy.CurrentState = Enemy.State.Notice;
			_enemy.SetPlayerTarget();
		}
	}
}
