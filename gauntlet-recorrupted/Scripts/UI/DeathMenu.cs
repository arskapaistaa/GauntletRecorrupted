using Godot;
using System;

public partial class DeathMenu : Control
{
	[Export] private Button RestartButton;
	[Export] private Button MainMenuButton;
	[Export] private Label _killsLabel = null;
	[Export(PropertyHint.File, "*.tscn")] private string MainMenuScene;

	private PlayerCharacter _player = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
		RestartButton.Pressed += OnRestartButtonPressed;
		MainMenuButton.Pressed += OnMainMenuButtonPressed;

		if (_player == null)
		{
			_player = Level.Current.Player;
			_player.PlayerDied += OnPlayerDied;
		}
	}

    public override void _ExitTree()
    {
    	if (_player != null)
		{
			_player.PlayerDied -= OnPlayerDied;
		}
    }

    public void ShowDeathMenu()
	{
		Visible = true;
		RestartButton.GrabFocus();
		Input.MouseMode = Input.MouseModeEnum.Visible;

		UpdateKillsLabel();
	}

	private void UpdateKillsLabel()
	{
		if (_killsLabel != null)
		{
			int kills = GameManager.Instance.GetKills();
			_killsLabel.Text = "Kills: " + kills.ToString();
		}
	}

	private void OnRestartButtonPressed()
	{
		GameManager.Instance.ResetStats();

		GetTree().ReloadCurrentScene();
		Visible = false;
	}

	private void OnMainMenuButtonPressed()
	{
		GameManager.Instance.ResetStats();

		if (MainMenuScene != null)
		{
			GetTree().ChangeSceneToFile(MainMenuScene);
		}
	}

	private void OnPlayerDied()
    {
		if (!Visible)
		{
			ShowDeathMenu();
		}
    }
}
