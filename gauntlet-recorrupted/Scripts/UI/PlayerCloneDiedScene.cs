using Godot;
using System;

public partial class PlayerCloneDiedScene : Control
{
	[Export] private VideoStreamPlayer _video = null;
	[Export(PropertyHint.File, "*.tscn")] private string _nextScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_video != null)
		{
			_video.Finished += OnVideoFinished;
		}
	}

    private void OnVideoFinished()
    {
        GetTree().CallDeferred("change_scene_to_file", _nextScene);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
