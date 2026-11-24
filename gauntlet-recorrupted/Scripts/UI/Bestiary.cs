using Godot;
using System;

public partial class Bestiary : NinePatchRect
{
	[Export] private BeastContainer[] _beastContainers;
	[Export] private Button _leftBtn = null;
	[Export] private Button _rightBtn = null;

	private int _currentContainer = 0;

	public override void _Ready()
	{
		CheckBestiaryStatus();
		UpdateCurrentContainer();

		if (_leftBtn != null)
		{
			_leftBtn.Pressed += OnLeftBtnPressed;
		}

		if (_rightBtn != null)
		{
			_rightBtn.Pressed += OnRightBtnPressed;
		}
	}

    public void CheckBestiaryStatus()
	{
		// Roach
		if (StatsManager.Instance.IsRoachKilled())
		{
			_beastContainers[0].EnemyKnown();
		}
		else
		{
			_beastContainers[0].EnemyUnknown();
		}

		// Tick
		if (StatsManager.Instance.IsTickKilled())
		{
			_beastContainers[1].EnemyKnown();
		}
		else
		{
			_beastContainers[1].EnemyUnknown();
		}

		//Baby
		if (StatsManager.Instance.IsBabyKilled())
		{

		}
		else
		{

		}
	}

	private void UpdateCurrentContainer()
	{
		foreach (BeastContainer beastContainer in _beastContainers)
		{
			if (beastContainer != _beastContainers[_currentContainer])
			{
				beastContainer.Hide();
			}
			else
			{
				_beastContainers[_currentContainer].Show();
			}
		}
	}

	private void OnLeftBtnPressed()
    {
        if (_currentContainer == 0)
		{
			_currentContainer = _beastContainers.Length - 1;
		}
		else
		{
			_currentContainer--;
		}

		UpdateCurrentContainer();

		GameManager.Instance.PlayButtonSFX();
    }

	private void OnRightBtnPressed()
    {
        if (_currentContainer == _beastContainers.Length - 1)
		{
			_currentContainer = 0;
		}
		else
		{
			_currentContainer++;
		}

		UpdateCurrentContainer();

		GameManager.Instance.PlayButtonSFX();
    }
}
