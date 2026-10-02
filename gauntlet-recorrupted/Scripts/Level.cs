using Godot;
using System;

public partial class Level : Node2D
{
	public static Level Current;
	[Export] public PlayerCharacter Player;
	[Export] private int _maxTotalEnemyCount;

	public int MaxTotalEnemyCount
	{
		get { return _maxTotalEnemyCount; }
	}
	public int CurrentTotalEnemyCount = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Maybe change this logic
		Current = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ChangeEnemyCount(int amount)
	{
		CurrentTotalEnemyCount += amount;
	}
}
