using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager Instance
	{
		get;
		private set;
	}

	public GameManager()
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

	private int _kills;
	private int _deaths;
	private int _shotsFired;

	private int _currentLevel;

	public int CurrentLevel
	{
		set { Mathf.Clamp(_currentLevel, 1, 10); }
		get { return _currentLevel; }
	}

	/// <summary>
	/// Call "On" methods when you want to collect stats.
	/// Like after shooting, enemy dies or player dies.
	/// </summary>
	/// <returns></returns>
	public void OnKillsChanged()
	{
		_kills++;
		StatsManager.Instance.OnKillsChanged();
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
}
