using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] private AnimationPlayer _animations = null;
	public override void _Ready()
	{
		GameManager.Instance.SetGameState(GameManager.GameState.Menu);

		_animations.AnimationFinished += OnAnimationFinished;
	}

    public override void _Input(InputEvent @event)
    {
		// Open settings
        if (@event.IsActionPressed("Menu") && GameManager.Instance.CurrentGameState != GameManager.GameState.Settings)
		{
			if (_animations != null)
			{
				_animations.Play("OpenOptions");
			}
		}

		// Close settings
		if (@event.IsActionPressed("Menu") && GameManager.Instance.CurrentGameState != GameManager.GameState.Menu)
		{
			if (_animations != null)
			{
				_animations.Play("CloseOptions");
			}
		}
    }

	private void OnAnimationFinished(StringName animName)
    {
        if (animName == "OpenOptions")
		{
			GameManager.Instance.SetGameState(GameManager.GameState.Settings);
		}

		if (animName == "CloseOptions")
		{
			GameManager.Instance.SetGameState(GameManager.GameState.Menu);
		}
    }
}
