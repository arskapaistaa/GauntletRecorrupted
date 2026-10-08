using Godot;
using System;

public partial class Exit : Area2D
{
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private string _nextLevelPath;
	[Export] private Level _currentLevel = null;

	public enum State
	{
		Unready,
		Ready
	}

	public State CurrentState = State.Unready;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_currentLevel != null)
		{
			_currentLevel.OnZeroSpawnerLeft += OnZeroSpawnerLeft;
		}
		else
		{
			GD.PrintErr("Exit is missin Level reference");
		}

		BodyEntered += OnBodyEntered;
	}


    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
		_currentLevel.OnZeroSpawnerLeft -= OnZeroSpawnerLeft;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter player)
		{
			if (CurrentState == State.Ready)
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

	/// <summary>
	/// Level manager emits this signal, that is how Exit knows Player can use it.
	/// </summary>
	private void OnZeroSpawnerLeft()
    {
        CurrentState = State.Ready;
    }
}
