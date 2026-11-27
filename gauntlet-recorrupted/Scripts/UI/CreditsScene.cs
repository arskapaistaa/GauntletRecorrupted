using Godot;
using System;

public partial class CreditsScene : Control
{
	[Export] private AnimationPlayer _animation = null;
	[Export(PropertyHint.File, "*.tscn")] private string _mainMenuScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_animation != null)
		{
			_animation.AnimationFinished += OnAnimationFinished;
		}
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Menu"))
		{
			GetTree().CallDeferred("change_scene_to_file", _mainMenuScene);
		}
    }

    private void OnAnimationFinished(StringName animName)
    {
        GetTree().CallDeferred("change_scene_to_file", _mainMenuScene);
    }


	public void StartAnimation()
	{
		if (_animation != null)
		{
			_animation.Play("Start");
		}
	}
}
