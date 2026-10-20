using Godot;
using System;

public partial class StatsManager : Node
{
	public static StatsManager Instance
	{
		get;
		private set;
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

		LoadSettings();
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
	private string _roachKilledString = "RoachKilled";
	private string _tickKilledString = "TickKilled";
	private string _babyKilledString = "BabyKilled";

	private bool _roachKilled = false;
	private bool _tickKilled = false;
	private bool _babyKilled = false;

	public void OnKillsChanged(string enemyType)
	{
		_kills++;
		_statsConfig.SetValue(_statsString, _killsString, _kills);

		CheckEnemyKilledStatus(enemyType);
		SaveStats();
	}

	public void OnDeathsChanged()
	{
		_deaths++;
		_statsConfig.SetValue(_statsString, _deathString, _deaths);
		SaveStats();
	}

	public void OnShotsFiredChanged()
	{
		_shotsFired++;
		_statsConfig.SetValue(_statsString, _shotsFiredString, _shotsFired);
		SaveStats();
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

	public bool IsRoachKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _roachKilledString, false);
	}
	public bool IsTickKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _tickKilledString, false);
	}
	public bool IsBabyKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _babyKilledString, false);
	}

	public void CheckEnemyKilledStatus(string type)
	{
		switch (type)
		{
			case "Roach":
				if (!_roachKilled)
				{
					_roachKilled = true;
					_statsConfig.SetValue(_statsString, _roachKilledString, _roachKilled);
				}
				break;

			case "Tick":
				if (!_tickKilled)
				{
					_tickKilled = true;
					_statsConfig.SetValue(_statsString, _tickKilledString, _tickKilled);
				}
				break;

			case "Baby":
				if (!_babyKilled)
				{
					_babyKilled = true;
					_statsConfig.SetValue(_statsString, _babyKilledString, _babyKilled);
				}
				break;
		}
	}

	private void LoadSettings()
	{
		// Check if there is save file, else use defaults.
		Error err = _statsConfig.Load(StatsFilePath);
		if (err != Error.Ok)
		{
			return;
		}

		// Basic stats
		_kills = (int)_statsConfig.GetValue(_statsString, _killsString, 0);
		_deaths = (int)_statsConfig.GetValue(_statsString, _deathString, 0);
		_shotsFired = (int)_statsConfig.GetValue(_statsString, _shotsFiredString, 0);

		// Bestiary
		_roachKilled = (bool)_statsConfig.GetValue(_statsString, _roachKilledString, false);
		_tickKilled = (bool)_statsConfig.GetValue(_statsString, _tickKilledString, false);
		_babyKilled = (bool)_statsConfig.GetValue(_statsString, _babyKilledString, false);

	}

	/// <summary>
	/// Currently this is called always when stat is changed.
	/// Maybe change it to save after the "Run" (One game loop) is over.
	/// </summary>
	public void SaveStats()
	{
		_statsConfig.Save(StatsFilePath);
	}

	public void ResetStats()
	{
		_statsConfig.Clear();
		// _statsConfig.SetValue(_statsString, _killsString, 0);
		// _statsConfig.SetValue(_statsString, _deathString, 0);
		// _statsConfig.SetValue(_statsString, _shotsFiredString, 0);
		SaveStats();
		LoadSettings();
	}
}
