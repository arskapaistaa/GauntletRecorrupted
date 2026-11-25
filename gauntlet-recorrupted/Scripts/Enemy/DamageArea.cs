using Godot;
using System;

public partial class DamageArea : Area2D
{
	private Enemy _enemy;

    [Signal] public delegate void AttackSucceedEventHandler();
	public override void _Ready()
	{
		_enemy = (Enemy)GetParent();

		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;
	}

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
		BodyExited -= OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Extra safety
        if (body is PlayerCharacter player)
        {
            if (_enemy.Brain.CurrentState == Brain.State.Ranged)
            {
                player.TakeDmg(_enemy.RangedDmg);
            }
            else
            {
                player.TakeDmg(_enemy.Dmg);
            }

            EmitSignal(SignalName.AttackSucceed);
        }

        if (body is Destroyable destroyable)
        {
            destroyable.TakeDmg(_enemy.Dmg);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        // Something
    }
}
