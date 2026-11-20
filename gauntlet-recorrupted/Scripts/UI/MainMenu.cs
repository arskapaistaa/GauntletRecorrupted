using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] private AnimationPlayer _animations = null;
	[Export] private VideoStreamPlayer _video = null;
	public override void _Ready()
	{
		GameManager.Instance.SetGameState(GameManager.GameState.Menu);

		if (_animations != null)
		{
			_animations.AnimationFinished += OnAnimationFinished;
		}

		if (_video != null)
		{
			_video.Finished += OnVideoFinished;
		}
	}

    public override void _ExitTree()
    {
		if (_animations != null)
		{
			_animations.AnimationFinished -= OnAnimationFinished;
		}

		if (_video != null)
		{
			_video.Finished -= OnVideoFinished;
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Menu") && GameManager.Instance.CurrentGameState != GameManager.GameState.Settings)
		{
			if (_animations != null)
			{
				_animations.Play("OpenExitPanel");
			}
		}
    }

	private void OnAnimationFinished(StringName animName)
    {
        if (animName == "OpenSettings")
		{
			GameManager.Instance.SetGameState(GameManager.GameState.Settings);
		}

		if (animName == "CloseSettings")
		{
			GameManager.Instance.SetGameState(GameManager.GameState.Menu);
		}
    }

	private void OnVideoFinished()
    {

    }
}
