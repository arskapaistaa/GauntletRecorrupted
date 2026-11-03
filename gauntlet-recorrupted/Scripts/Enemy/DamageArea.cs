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
            player.TakeDmg(_enemy.Dmg);
            EmitSignal(SignalName.AttackSucceed);
        }
    }

    private void OnBodyExited(Node2D body)
    {
        // Something
    }
}
