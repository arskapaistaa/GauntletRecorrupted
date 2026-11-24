using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance
	{
		get;
		private set;
	}

	[Export] private AudioStreamPlayer _buttonSFX;
	[Export] private AudioStreamPlayer _gameplayMusic;

	private int _kills;
	private int _deaths;
	private int _shotsFired;
	private int _spawnersDestroyed;

	private int _currentLevel;
	private bool _usingController = false;

	public int CurrentLevel
	{
		set { _currentLevel = Mathf.Clamp(_currentLevel, 1, 10); }
		get { return _currentLevel; }
	}

	// Even this is a public, set it with SetGameState();
	public GameState CurrentGameState = GameState.Menu;
	public enum GameState
	{
		Menu, // Call from MainMenu _Ready()
		Settings, // Call in MainMenu
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

		ResetStats();
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

	public void OnSpawnerDestroyedChanged()
	{
		_spawnersDestroyed++;
		StatsManager.Instance.OnSpawnerDestroyed();
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

	public int GetSpawner()
	{
		return _spawnersDestroyed;
	}

	/// <summary>
	/// Set current game state with this method.
	/// </summary>
	/// <param name="state">Next state</param>
	public void SetGameState(GameState state)
	{
		CurrentGameState = state;
		GD.Print(CurrentGameState.ToString());
		if (CurrentGameState != GameState.Game && !_usingController)
			Input.MouseMode = Input.MouseModeEnum.Visible;

		if (state == GameState.Game)
		{
			PlayGameplayMusic();
		}
		else
		{
			StopGameplayMusic();
		}
	}

	public void SetTutoriLevelComplited()
	{
		StatsManager.Instance.SetTutoriLevelComplited();
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
		_spawnersDestroyed = resetToZero;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventJoypadMotion joypadMotion && Mathf.Abs(joypadMotion.AxisValue) > 0.3f)
		{
			_usingController = true;
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
		else if (@event is InputEventJoypadButton)
		{
			_usingController = true;
			Input.MouseMode = Input.MouseModeEnum.Hidden;
		}
		else if (@event is InputEventMouseMotion || @event is InputEventMouseButton || @event is InputEventKey)
		{
			_usingController = false;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public bool IsUsingController()
	{
		return _usingController;
	}

	public void PlayButtonSFX()
	{
		if (_buttonSFX != null)
		{
			_buttonSFX.Play();
		}
	}

	public void PlayGameplayMusic()
	{
		if (_gameplayMusic != null)
		{
			if (_gameplayMusic.Playing == false)
			{
				_gameplayMusic.Play();
			}
		}
	}

	public void StopGameplayMusic()
	{
		if (_gameplayMusic != null)
		{
			if (_gameplayMusic.Playing == true)
			{
				_gameplayMusic.Stop();
			}
		}
	}
}
