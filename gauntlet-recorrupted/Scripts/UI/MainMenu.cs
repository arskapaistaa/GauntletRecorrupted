using Godot;
using System;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GameManager.Instance.SetGameState(GameManager.GameState.Menu);
	}
}
