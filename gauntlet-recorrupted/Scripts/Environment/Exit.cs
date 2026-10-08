using Godot;
using System;

public partial class Exit : Area2D
{
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private string _nextLevelPath;

	public enum State
	{
		Unready,
		Ready
	}

	public State CurrentState = State.Unready;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter player)
		{
			if (_nextLevelPath != null)
			{
				GetTree().CallDeferred("change_scene_to_file", _nextLevelPath);
			}
			else
			{
				GD.PrintErr("Exit misses Next Level Path");
				return;
			}

		}
    }
}
