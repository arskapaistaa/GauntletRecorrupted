using Godot;
using System;

public partial class DeathMenu : Control
{
	[Export] private Button RestartButton;
	[Export] private Button MainMenuButton;
	[Export(PropertyHint.File, "*.tscn")] private string MainMenuScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		RestartButton.Pressed += OnRestartButtonPressed;
		MainMenuButton.Pressed += OnMainMenuButtonPressed;
	}

	public void ShowDeathMenu()
	{
		Visible = true;
		RestartButton.GrabFocus();
		Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	private void OnRestartButtonPressed()
	{
		GetTree().ReloadCurrentScene();
		Visible = false;
	}

	private void OnMainMenuButtonPressed()
	{
		if (MainMenuScene != null)
		{
			GetTree().ChangeSceneToFile(MainMenuScene);
		}
	}

	public override void _Process(double delta)
	{
		if (!Level.Current.Player.isAlive() && !Visible)
		{
			ShowDeathMenu();
		}
	}
}
