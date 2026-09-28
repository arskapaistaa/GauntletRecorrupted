using Godot;
using System;

public partial class DamageArea : Area2D
{
	private Enemy _enemy;
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
        if (body is PlayerCharacter player)
		{
			// Take dmg
		}
    }

    private void OnBodyExited(Node2D body)
    {
        // Something
    }
}
