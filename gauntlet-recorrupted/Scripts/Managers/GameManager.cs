using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance
	{
		get;
		private set;
	}

	private int _kills;
	private int _deaths;
	private int _shotsFired;

	private int _currentLevel;
	private bool _usingController = false;

	public int CurrentLevel
	{
		set { Mathf.Clamp(_currentLevel, 1, 10); }
		get { return _currentLevel; }
	}

	// Even this is a public, set it with SetGameState();
	public GameState CurrentGameState = GameState.Menu;
	public enum GameState
	{
		Menu, // Call from MainMenu _Ready(),
		Game, 	  // Call when the Player enter in main game. Also when exits from Pause menu
		Pause,	  // Call when player presses Pause input.
		End		  // Call when game ends -> Player dies or finishes the game.
	}
    public override sealed void _Ready()
    {
        if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			QueueFree();
			return;
		}
    }

	/// <summary>
	/// Call "On" methods when you want to collect stats.
	/// Like after shooting, enemy dies or player dies.
	/// </summary>
	/// <returns></returns>
	public void OnKillsChanged(string enemyType)
	{
		_kills++;
		StatsManager.Instance.OnKillsChanged(enemyType);
	}

	public void OnDeathsChanged()
	{
		_deaths++;
		StatsManager.Instance.OnDeathsChanged();
	}

	public void OnShotsFiredChanged()
	{
		_shotsFired++;
		StatsManager.Instance.OnShotsFiredChanged();
	}

	/// <summary>
	/// Use Get methods for UI etc.
	/// </summary>
	/// <returns></returns>
	public int GetKills()
	{
		return _kills;
	}

	/// <summary>
	/// Use Get methods for UI etc.
	/// </summary>
	/// <returns></returns>
	public int GetDeaths()
	{
		return _deaths;
	}

	/// <summary>
	/// Use Get methods for UI etc.
	/// </summary>
	/// <returns></returns>
	public int GetShotsFired()
	{
		return _shotsFired;
	}

	public void SetGameState(GameState state)
	{
		CurrentGameState = state;
		GD.Print(CurrentGameState.ToString());
		if (CurrentGameState != GameState.Game)
			Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	/// <summary>
	/// Call this method from main menu or restart button.
	/// </summary>
	public void ResetStats()
	{
		int resetToZero = 0;

		_kills = resetToZero;
		_deaths = resetToZero;
		_shotsFired = resetToZero;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventJoypadMotion joypadMotion && Mathf.Abs(joypadMotion.AxisValue) > 0.3f)
		{
			_usingController = true;
		}
		else if (@event is InputEventJoypadButton)
		{
			_usingController = true;
		}
		else if (@event is InputEventMouseMotion || @event is InputEventMouseButton)
		{
			_usingController = false;
		}
	}

	public bool IsUsingController()
	{
		return _usingController;
	}
}
