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
	private int _spawnersDestroyed;
	private int _fastestPlayTime;

	private string _statsString = "Stats";
	private string _killsString = "Kills";
	private string _deathString = "Deaths";
	private string _spawnersString = "Spawners";
	private string _fastestPlayTimeString = "FastestPlayTime";
	private string _shotsFiredString = "ShotsFired";
	private string _roachKilledString = "RoachKilled";
	private string _tickKilledString = "TickKilled";
	private string _babyKilledString = "BabyKilled";
	private string _adultKilledString = "AdultKilled";
	private string _lieroKilledString = "LieroKilled";
	private string _spiderKilledString = "SpiderKilled";
	private string _wurmKilledString = "WurmKilled";
	private string _bearKilledString = "BearKilled";
	private string _tutorialLevelComplitedString = "TutorialComplited";

	private bool _roachKilled = false;
	private bool _tickKilled = false;
	private bool _babyKilled = false;
	private bool _adultKilled = false;
	private bool _lieroKilled = false;
	private bool _spiderKilled = false;
	private bool _wurmKilled = false;
	private bool _bearKilled = false;

	private bool _tutorialLevelComplited = false;

	/// <summary>
	/// Every "On" method is called from GameManager.
	/// To actually save data to config file.
	/// </summary>
	/// <param name="enemyType">Enemy to check if the player is ever kille it</param>
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

	public void OnSpawnerDestroyed()
	{
		_spawnersDestroyed++;
		_statsConfig.SetValue(_statsString, _spawnersString, _spawnersDestroyed);
		SaveStats();
	}

	public void OnGameFinished()
	{
		_fastestPlayTime = GameManager.Instance.GetPlayTime();
		_statsConfig.SetValue(_statsString, _fastestPlayTimeString, _fastestPlayTime);
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

	public int GetSpawnersDestroyed()
	{
		return (int)_statsConfig.GetValue(_statsString, _spawnersString, 0);
	}

	public int GetFastestPlayTime()
	{
		return (int)_statsConfig.GetValue(_statsString, _fastestPlayTimeString, 0);
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

	public bool IsAdultKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _adultKilledString, false);
	}

	public bool IsSpiderKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _spiderKilledString, false);
	}

	public bool IsLieroKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _lieroKilledString, false);
	}

	public bool IsWurmKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _wurmKilledString, false);
	}

	public bool IsBearKilled()
	{
		return (bool)_statsConfig.GetValue(_statsString, _bearKilledString, false);
	}

	public bool IsTuotorialFinished()
	{
		return (bool)_statsConfig.GetValue(_statsString, _tutorialLevelComplitedString, false);
	}

	/// <summary>
	/// Check if the player is killed this enemy type.
	/// </summary>
	/// <param name="type"></param>
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

			case "Adult":
				if (!_adultKilled)
				{
					_adultKilled = true;
					_statsConfig.SetValue(_statsString, _adultKilledString, _adultKilled);
				}
				break;

			case "Spider":
				if (!_spiderKilled)
				{
					_spiderKilled = true;
					_statsConfig.SetValue(_statsString, _spiderKilledString, _spiderKilled);
				}
				break;

			case "Liero":
				if (!_lieroKilled)
				{
					_lieroKilled = true;
					_statsConfig.SetValue(_statsString, _lieroKilledString, _lieroKilled);
				}
				break;

			case "Wurm":
				if (!_wurmKilled)
				{
					_wurmKilled = true;
					_statsConfig.SetValue(_statsString, _wurmKilledString, _wurmKilled);
				}
				break;

			case "Bear":
				if (!_bearKilled)
				{
					_bearKilled = true;
					_statsConfig.SetValue(_statsString, _bearKilledString, _bearKilled);
				}
				break;
		}

		SaveStats();
	}

	public void SetTutoriLevelComplited()
	{
		_tutorialLevelComplited = true;
		_statsConfig.SetValue(_statsString, _tutorialLevelComplitedString, _tutorialLevelComplited);

		SaveStats();
	}

	/// <summary>
	/// Load settings from config file.
	/// </summary>
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
		_spawnersDestroyed = (int)_statsConfig.GetValue(_statsString, _spawnersString, 0);
		_fastestPlayTime = (int)_statsConfig.GetValue(_statsString, _fastestPlayTimeString, 0);

		// Bestiary
		_roachKilled = IsRoachKilled();
		_tickKilled = IsTickKilled();
		_babyKilled = IsBabyKilled();
		_adultKilled = IsAdultKilled();
		_lieroKilled = IsLieroKilled();
		_bearKilled = IsBearKilled();

		// Tutorial level
		_tutorialLevelComplited = (bool)_statsConfig.GetValue(_statsString, _tutorialLevelComplitedString, false);

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

		SaveStats();
		LoadSettings();
	}
}
