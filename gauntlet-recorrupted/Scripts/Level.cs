using Godot;
using System;

public partial class Level : Node2D
{
	public static Level Current;

	[Export] public bool IsThisTutorialLevel = false;
	[Export] public bool IsThisFinalLevel = false;

	[ExportCategory("Stats")]
	[Export] public EndingCondition CurrentEndingCondition = EndingCondition.SpawnersDestroyed;
	[Export] private int _maxTotalEnemyCount;
	/// <summary>
	/// How many spawners there are in the level.
	/// </summary>
	[Export] private int _spawnerCount;
	/// <summary>
	/// What is the enemy count in Editor -> How many enemies there are at the beggining.
	/// </summary>
	[Export(PropertyHint.Range, "0,100,suffix:in editor")] private int _presetEnemyCount;
	[ExportGroup("Conditions")]
	[Export] private bool _isThereWurmInThisLevel = false;
	[Export(PropertyHint.Range, "0,1.0,0.01,suffix:*100%")] private float _wurmPatrolStartChange = 0.1f;

	[ExportCategory("Node preferences")]
	[Export] public NavigationRegion2D _navRegion = null;
	[Export] public PlayerCharacter Player = null;
	[Export] public DiggableTiles DiggableTiles = null;
	[Export] private Timer _eventCheckTimer = null;

	public enum EndingCondition
	{
		SpawnersDestroyed,
		BearKilled,
		PlayerCloneKilled
	}

	private bool _wurmAweken = false;

	public int MaxTotalEnemyCount
	{
		get { return _maxTotalEnemyCount; }
	}

	public int SpawnerCount
	{
		get { return _spawnerCount; }
	}

	private int _enemyAudioCount = 0;

	public int EnemyAudioCount
	{
		get { return _enemyAudioCount; }
		set { _enemyAudioCount = Mathf.Clamp(_enemyAudioCount, 0, 10); }
	}

	public int CurrentSpawnerCount = 0;
	public int CurrentTotalEnemyCount = 0;

	[Signal] public delegate void ExitReadyEventHandler();
	[Signal] public delegate void OnZeroSpawnerLeftEventHandler();
	[Signal] public delegate void OnBearKilledEventHandler();
	[Signal] public delegate void EnemyCountChangedEventHandler();

	public override void _EnterTree()
	{
		Current = this;
	}
	public override void _Ready()
	{
		GameManager.Instance.SetGameState(GameManager.GameState.Game);

		ChangeEnemyCount(_presetEnemyCount);

		if (_eventCheckTimer != null)
		{
			_eventCheckTimer.Start();
			_eventCheckTimer.Timeout += OnTimerTimeOut;
		}

		CurrentSpawnerCount = SpawnerCount;
	}

	public override void _ExitTree()
	{
		if (_eventCheckTimer != null)
		{
			_eventCheckTimer.Timeout -= OnTimerTimeOut;
		}
	}

	public void ChangeEnemyCount(int amount)
	{
		CurrentTotalEnemyCount += amount;

		EmitSignal(SignalName.EnemyCountChanged);
	}

	public void ChangeSpawnerCount()
	{
		if (CurrentSpawnerCount > 0)
		{
			CurrentSpawnerCount--;
		}

		if (CurrentSpawnerCount == 0)
		{
			EmitSignal(SignalName.ExitReady);
			EmitSignal(SignalName.OnZeroSpawnerLeft);
		}
	}

	public void BearKilled()
	{
		if (CurrentEndingCondition == EndingCondition.BearKilled)
		{
			EmitSignal(SignalName.ExitReady);
			EmitSignal(SignalName.OnBearKilled);
		}
	}

	public void UpdateNavRegion()
	{
		if (_navRegion != null && !_navRegion.IsBaking())
		{
			_navRegion.BakeNavigationPolygon();
		}
		else
		{
			GD.PrintErr("Level is missing NavRegion");
		}
	}

	private void OnTimerTimeOut()
	{
		// Check Wurm
		if (_isThereWurmInThisLevel && !_wurmAweken)
		{
			if (RandomEventChange(_wurmPatrolStartChange))
			{
				foreach (Enemy enemy in GetTree().GetNodesInGroup("Wurm"))
				{
					_wurmAweken = true;

					enemy.WakeUp();
					GD.Print("Wurm wake");
				}
			}
		}
	}

	/// <summary>
	/// Method check should this event happen.
	/// </summary>
	/// <param name="change">Give value of change, like how many %. </param>
	/// <returns>If event should happen.</returns>
	private bool RandomEventChange(float change)
	{
		float randomF = GD.Randf();

		if (randomF > change)
		{
			return false;
		}

		return true;
	}
}
