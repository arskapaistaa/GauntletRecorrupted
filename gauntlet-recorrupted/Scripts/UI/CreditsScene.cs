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

    private void OnAnimationFinished(StringName animName)
    {
        GetTree().CallDeferred("change_scene_to_file", _mainMenuScene);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
