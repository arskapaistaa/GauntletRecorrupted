using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export] private Button ResumeButton;
	[Export] private Button MainMenuButton;
	[Export(PropertyHint.File, "*.tscn")] private PackedScene MainMenuScene;
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
	}

	private void OnResumeButtonPressed()
	{
		GetTree().Paused = false;
		Visible = false;
	}

	private void OnMainMenuButtonPressed()
	{
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
	}
}
