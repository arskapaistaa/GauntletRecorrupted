using Godot;
using System;

public partial class Bestiary : MarginContainer
{
	[Export] private Label _roachLabel = null;
	[Export] private Label _tickLabel = null;
	[Export] private Label _babyLabel = null;
	public override void _Ready()
	{
		CheckBestiaryStatus();
	}

	public void CheckBestiaryStatus()
	{
		// Roach
		if (StatsManager.Instance.IsRoachKilled() && _roachLabel != null)
		{
			_roachLabel.Text = "Yes";
		}
		else
		{
			_roachLabel.Text = "No";
		}

		// Tick
		if (StatsManager.Instance.IsTickKilled() && _tickLabel != null)
		{
			_tickLabel.Text = "Yes";
		}
		else
		{
			_tickLabel.Text = "No";
		}

		//Baby
		if (StatsManager.Instance.IsBabyKilled() && _babyLabel != null)
		{
			_babyLabel.Text = "Yes";
		}
		else
		{
			_tickLabel.Text = "No";
		}

	}
}
