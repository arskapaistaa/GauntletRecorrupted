using Godot;
using System;
using System.Collections.Generic;

public partial class DiggableTiles : TileMapLayer
{
	[ExportCategory("Enemy Spawning")]
	[Export(PropertyHint.Range, "0,1,0.01")] public float EnemySpawnChance = 0.08f;
	[Export] public PackedScene TickScene;
	[Export] public PackedScene BabyScene;
	[Export] public PackedScene RoachScene;
	[ExportCategory("Tile Data")]
	[Export] public string MaxHealthDataString = "MaxHealth";
	[Export] public PackedScene BreakEffectScene;
	private readonly Dictionary<Vector2I, int> _tileHealth = new Dictionary<Vector2I, int>();

	public void DamageTile(Vector2I coords, int damage)
	{
		if (GetCellSourceId(coords) == -1)
			return;
		int currentHealth;
		if (!_tileHealth.TryGetValue(coords, out currentHealth))
		{
			currentHealth = GetMaxHealth(coords);
		}
		currentHealth -= damage;
		if (currentHealth <= 0)
		{
			_tileHealth.Remove(coords);
			EraseCell(coords);
			SpawnBreakEffect(coords);
			RandomlySpawnEnemy(coords);
		}
		else
        {
            _tileHealth[coords] = currentHealth;
        }
	}

	private int GetCurrentHealth(Vector2I coords)
	{
		int currentHealth;
		if (!_tileHealth.TryGetValue(coords, out currentHealth))
		{
			currentHealth = GetMaxHealth(coords);
		}
		return currentHealth;
	}

	private int GetMaxHealth(Vector2I coords)
	{
		TileData tileData = GetCellTileData(coords);
        return tileData.GetCustomData(MaxHealthDataString).AsInt32();
	}

	private void SpawnBreakEffect(Vector2I cell)
	{
		if (BreakEffectScene == null)
		{
			return;
		}

		Node2D effect = BreakEffectScene.Instantiate<Node2D>();
		GetTree().CurrentScene.AddChild(effect);
		effect.GlobalPosition = ToGlobal(MapToLocal(cell));
	}

	private void RandomlySpawnEnemy(Vector2I cell)
	{
		if (TickScene == null || BabyScene == null || RoachScene == null)
			return;

		if (GD.Randf() >= EnemySpawnChance)
			return;

		PackedScene[] enemyScenes = { TickScene, BabyScene, RoachScene };
		PackedScene chosenScene = enemyScenes[GD.Randi() % enemyScenes.Length];
		Enemy enemy = chosenScene.Instantiate<Enemy>();
		enemy.Position = ToGlobal(MapToLocal(cell));
		GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, enemy);
	}
}
