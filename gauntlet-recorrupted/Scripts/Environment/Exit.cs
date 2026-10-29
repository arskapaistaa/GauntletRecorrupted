using Godot;
using System;

public partial class Exit : Area2D
{
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private string _nextLevelPath;
	[Export] private Level _currentLevel = null;
	[Export(PropertyHint.Enum, "FadeInAndOut")]
	private string _transitionName;

	public enum State
	{
		UnReady,
		Ready
	}

	public State CurrentState = State.UnReady;

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

		UpdateSpawnerCountLabel();
	}


    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
		_currentLevel.OnZeroSpawnerLeft -= OnZeroSpawnerLeft;
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateAnimations();
    }

	public void UpdateAnimations()
	{
		switch (CurrentState)
		{
			case State.UnReady:
				_sprite.Play("UnReady");
				break;

			case State.Ready:
				_sprite.Play("Ready");
				break;
		}
	}

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter player)
		{
			if (CurrentState == State.Ready)
			{
				if (_nextLevelPath != null)
				{
					TransitionController.Instance.PlayTransition(_transitionName, _nextLevelPath);
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

	public void UpdateSpawnerCountLabel()
	{
		//if (_spawnerCountLabel != null)
		//{
		//	_spawnerCountLabel.Text = Level.Current.CurrentSpawnerCount.ToString() + "/" + Level.Current.SpawnerCount.ToString();
		// }
	}
}
