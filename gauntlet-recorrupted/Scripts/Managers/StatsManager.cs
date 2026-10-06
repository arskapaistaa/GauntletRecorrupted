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

	private string _statsString = "Stats";
	private string _killsString = "Kills";
	private string _deathString = "Deaths";
	private string _shotsFiredString = "ShotsFired";

	public void OnKillsChanged()
	{
		_kills++;
		_statsConfig.SetValue(_statsString, _killsString, _kills);
	}

	public void OnDeathsChanged()
	{
		_deaths++;
		_statsConfig.SetValue(_statsString, _deathString, _deaths);
	}

	public void OnShotsFiredChanged()
	{
		_shotsFired++;
		_statsConfig.SetValue(_statsString, _shotsFiredString, _shotsFired);
	}

	public int GetKills()
	{
		return (int)_statsConfig.GetValue(_statsString, _killsString, 0);
	}

	public int GetDeaths()
	{
		return (int)_statsConfig.GetValue(_statsString, _deathString, 0);
	}

	public int GetShotsFired()
	{
		return (int)_statsConfig.GetValue(_statsString, _shotsFiredString, 0);
	}
}
