using Godot;
using System;

public partial class GUI : Control
{
	[Export] private Label _fpsLabel = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_fpsLabel != null)
		{
			_fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
		}
	}
}
