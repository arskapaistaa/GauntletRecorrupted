using Godot;
using System;

public partial class StatsMenu : MarginContainer
{
	[Export] private Label _killsLabel = null;
	[Export] private Label _deathsLabel = null;
	[Export] private Label _shotsFiredLabel = null;

	public override void _Ready()
	{
		UpdateLabels();
	}

	/// <summary>
	/// Call this when labels need update.
	/// </summary>
	public void UpdateLabels()
	{
		if (_killsLabel != null)
		{
			_killsLabel.Text = StatsManager.Instance.GetKills().ToString();
		}
		else
		{
			GD.PrintErr("Missing kills label");
		}

		if (_deathsLabel != null)
		{
			_deathsLabel.Text = StatsManager.Instance.GetDeaths().ToString();
		}
		else
		{
			GD.PrintErr("Missing Deaths label");
		}

		if (_shotsFiredLabel != null)
		{
			_shotsFiredLabel.Text = StatsManager.Instance.GetShotsFired().ToString();
		}
		else
		{
			GD.PrintErr("Missing ShotsFired label");
		}
	}
}
