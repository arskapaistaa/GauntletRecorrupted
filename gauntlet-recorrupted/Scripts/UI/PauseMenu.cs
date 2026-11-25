using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export] private Button ResumeButton;
	[Export] private Button MainMenuButton;
	[Export] private Label _playTimeLabel = null;
	[Export(PropertyHint.File, "*.tscn")] private PackedScene MainMenuScene;
	[Export] private AnimationPlayer _animationPlayer = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		ResumeButton.Pressed += OnResumeButtonPressed;
		MainMenuButton.Pressed += OnMainMenuButtonPressed;
	}

	public void ShowPauseMenu()
	{
		GetTree().Paused = true;
		Visible = true;
		ResumeButton.GrabFocus();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		UpdatePlayTimeLabel();
	}

	private void OnResumeButtonPressed()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	private void OnMainMenuButtonPressed()
	{
		GameManager.Instance.ResetStats();

		GetTree().Paused = false;
		if (MainMenuScene != null)
		{
			GetTree().ChangeSceneToPacked(MainMenuScene);
		}
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Menu"))
		{
			if (Visible)
			{
				GetTree().Paused = false;
				Visible = false;
			}
			else
			{
				ShowPauseMenu();
			}
		}
		if (!Visible)
		{
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
	}

	private void UpdatePlayTimeLabel()
	{
		if (_playTimeLabel != null)
		{
			TimeSpan time = TimeSpan.FromSeconds(GameManager.Instance.GetPlayTime());
			_playTimeLabel.Text = time.ToString();
		}
	}
}
