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
	[Export(PropertyHint.Range, "0, 100, suffix:in editor")]
	private int _presetEnemyCount;

	[ExportCategory("Node preferences")]
	[Export] public NavigationRegion2D _navRegion = null;
	[Export] public PlayerCharacter Player = null;

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
}
