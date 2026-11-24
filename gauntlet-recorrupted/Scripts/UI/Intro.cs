using Godot;
using System;

public partial class Intro : Control
{
	[Export] private VideoStreamPlayer _video = null;
	[Export(PropertyHint.File, "*.tscn")] private string MainMenuScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_video != null)
		{
			_video.Finished += OnVideoFinished;
		}
	}

	public override void _Input(InputEvent @event)
    {
		if (_video.IsPlaying())
		{
			if (@event.IsActionPressed("Menu"))
			{
				_video.Stop();

				GetTree().CallDeferred("change_scene_to_file", MainMenuScene);
				return;
			}
		}
    }

	private void OnVideoFinished()
    {
		GetTree().CallDeferred("change_scene_to_file", MainMenuScene);
    }
}
