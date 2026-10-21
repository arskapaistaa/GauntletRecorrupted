using Godot;
using System;

public partial class Level : Node2D
{
	public static Level Current;

	[ExportCategory("Stats")]
	[Export] private int _maxTotalEnemyCount;
	/// <summary>
	/// How many spawners there are in the level.
	/// </summary>
	[Export] private int _spawnerCount;
	/// <summary>
	/// What is the enemy count in Editor -> How many enemies there are at the beggining.
	/// </summary>
	[Export(PropertyHint.Range, "0,100,suffix:in editor")]
	private int _presetEnemyCount;
	[ExportGroup("Conditions")]
	[Export] private bool _isThereWurmInThisLevel = false;
	[Export(PropertyHint.Range, "0,1.0,0.01,suffix:*100%")]
	private float _wurmPatrolStartChange = 0.1f;

	[ExportCategory("Node preferences")]
	[Export] public NavigationRegion2D _navRegion = null;
	[Export] public PlayerCharacter Player = null;
	[Export] private Timer _eventCheckTimer = null;

	private bool _wurmAweken = false;

	/// <summary>
	/// Intensity level indicates how intensive the level is currently.
	/// 0.0f - 2.5f LOW
	/// 2.6f - 5.0f MID
	/// 5.1f - 7.5f HIGH
	/// 7.6f - 10.0f INSANE
	/// </summary>
	private float _intensityLevel = 0.0f;
	public float IntensityLevel
	{
		get { return _intensityLevel; }
		private set { Mathf.Clamp(_intensityLevel, 0.0f, 10.0f); }
	}

	public int MaxTotalEnemyCount
	{
		get { return _maxTotalEnemyCount; }
	}

	public int SpawnerCount
	{
		get { return _spawnerCount; }
	}
	public int CurrentTotalEnemyCount = 0;

	[Signal] public delegate void OnZeroSpawnerLeftEventHandler();
	[Signal] public delegate void EnemyCountChangedEventHandler();
	public override void _Ready()
	{
		// Maybe change this logic
		Current = this;

		GameManager.Instance.SetGameState(GameManager.GameState.Game);

		ChangeEnemyCount(_presetEnemyCount);

		if (_eventCheckTimer != null)
		{
			_eventCheckTimer.Start();
			_eventCheckTimer.Timeout += OnTimerTimeOut;
		}
	}

    public override void _ExitTree()
    {
        if (_eventCheckTimer != null)
		{
			_eventCheckTimer.Timeout -= OnTimerTimeOut;
		}
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Menu"))
		{
			GetTree().CallDeferred("change_scene_to_file", "res://Scenes/PlayTestScene/TestMenu.tscn");
		}
    }

	public void ChangeEnemyCount(int amount)
	{
		CurrentTotalEnemyCount += amount;

		EmitSignal(SignalName.EnemyCountChanged);
	}

	public void ChangeSpawnerCount()
	{
		if (_spawnerCount > 0)
		{
			_spawnerCount--;
		}

		if (_spawnerCount == 0)
		{
			EmitSignal(SignalName.OnZeroSpawnerLeft);
		}

	}

	public void UpdateNavRegion()
	{
		if (_navRegion != null)
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
