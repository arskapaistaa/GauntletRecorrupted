using Godot;
using System;

public partial class StatsManager : Node
{
	public static StatsManager Instance
	{
		get;
		private set;
	}

	public StatsManager()
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

	private const string StatsFilePath = "user://stats.cfg";
	private ConfigFile _statsConfig = new ConfigFile();

	private int _kills;
	private int _deaths;
	private int _shotsFired;

	public void OnKillsChanged(int value)
	{
		if (value > 0)
		{
			_kills += value;
			_statsConfig.SetValue("Stats", "Kills", _kills);
		}
	}

	public void OnDeathsChanged(int value)
	{
		if (value > 0)
		{
			_deaths += value;
			_statsConfig.SetValue("Stats", "Deaths", _deaths);
		}
	}

	public void OnShotsFiredChanged(int value)
	{
		if (value > 0)
		{
			_deaths += value;
			_statsConfig.SetValue("Stats", "ShotsFired", _shotsFired);
		}
	}

	public int GetKills()
	{
		return (int)_statsConfig.GetValue("Stats", "Kills", 0);
	}

	public int GetDeaths()
	{
		return (int)_statsConfig.GetValue("Stats", "Deaths", 0);
	}

	public int GetShotsFired()
	{
		return (int)_statsConfig.GetValue("Stats", "ShotsFired", 0);
	}
}
