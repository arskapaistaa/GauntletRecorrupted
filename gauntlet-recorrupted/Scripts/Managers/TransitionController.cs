using Godot;
using System;

public partial class TransitionController : Node
{
	public static TransitionController Instance
	{
		get;
		private set;
	}

	private string _pathToNextScene;
	private AnimationPlayer _animationPlayer = null;

	[Signal] public delegate void TransitionFinishedEventHandler();

    public override void _Ready()
    {
        if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			QueueFree();
			return;
		}

		if (_animationPlayer == null)
		{
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		}

		_animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    /// <summary>
    /// Plays transitions.
    /// List of animations: FadeInAndOut
    /// </summary>
    /// <param name="animationName">Set animation name here</param>
    public void PlayTransition(string animationName, string path)
	{
		_pathToNextScene = path;

		switch (animationName)
		{
			case "FadeInAndOut":
				_animationPlayer.Play("FadeIn");
				break;
		}
	}

	private void OnAnimationFinished(StringName animName)
    {
		if (animName == "FadeIn")
		{
			GetTree().CallDeferred("change_scene_to_file", _pathToNextScene);

			_animationPlayer.Play("FadeOut");
			EmitSignal(SignalName.TransitionFinished);
		}


    }
}
