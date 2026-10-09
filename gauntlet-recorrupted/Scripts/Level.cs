using Godot;
using System;

public partial class Level : Node2D
{
	public static Level Current;

	[ExportCategory("Stats")]
	[Export] private int _maxTotalEnemyCount;
	[Export] private int _spawnerCount;

	[ExportCategory("Node preferences")]
	[Export] public NavigationRegion2D _navRegion = null;
	[Export] public PlayerCharacter Player = null;

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
	public override void _Ready()
	{
		// Maybe change this logic
		Current = this;

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
