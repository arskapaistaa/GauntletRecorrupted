using Godot;
using System;

public partial class BeastContainer : Control
{
	[Export] private Control _enemyUnknown = null;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	public void EnemyKnown()
	{
		if (_enemyUnknown != null)
		{
			_enemyUnknown.Hide();
		}
	}

	public void EnemyUnknown()
	{
		if (_enemyUnknown != null)
		{
			_enemyUnknown.Show();
		}
	}
}
