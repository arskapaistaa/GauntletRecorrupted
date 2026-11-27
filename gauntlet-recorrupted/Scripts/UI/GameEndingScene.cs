using Godot;
using System;

public partial class GameEndingScene : Control
{
	[Export] private VideoStreamPlayer _video = null;
	[Export] private AnimationPlayer _animation = null;
	[Export(PropertyHint.File, "*.tscn")] private string _nextScene;
	[Export] private Label _killsLabel = null;
	[Export] private Label _timeLabel = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_video != null)
		{
			_video.Finished += OnVideoFinished;
		}

		if (_killsLabel != null)
		{
			_killsLabel.Text = GameManager.Instance.GetKills().ToString();
		}

		if (_timeLabel != null)
		{
			TimeSpan time = TimeSpan.FromSeconds(StatsManager.Instance.GetFastestPlayTime());
			_timeLabel.Text = time.ToString();
		}
	}

    private void OnVideoFinished()
    {
        if (_animation != null)
		{
			_animation.Play("Start");
		}
    }

}
