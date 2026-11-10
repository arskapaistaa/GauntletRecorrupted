using Godot;
using System;

public partial class GUI : Control
{
	[Export] private Label _fpsLabel = null;
	[Export] private Label _healthLabel = null;
	[Export] private AnimatedSprite2D _healthIndicator = null;
	[Export] private AnimatedSprite2D _exitIndicator = null;

	private PlayerCharacter _player = null;
	private Level _currentLevel = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_player == null)
		{
			_player = Level.Current.Player;
			_player.PlayerHealthChanged += UpdateHealthIndicators;
		}

		if (_currentLevel == null)
		{
			_currentLevel = Level.Current;
			_currentLevel.OnZeroSpawnerLeft += OnZeroSpawnerLeft;
		}

		if (_player != null && _healthLabel != null)
		{
			UpdateHealthIndicators();
		}

		if (_exitIndicator != null)
		{
			_exitIndicator.Play("Closed");
			_exitIndicator.AnimationFinished += OnExitIndicatorAnimationFinished;
		}
	}

    public override void _ExitTree()
    {
        if (_player != null)
		{
			_player.PlayerHealthChanged -= UpdateHealthIndicators;
		}
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (_fpsLabel != null)
		{
			_fpsLabel.Text = Engine.GetFramesPerSecond().ToString();
		}
	}

	public void UpdateHealthIndicators()
	{
		if (_player != null && _healthLabel != null)
		{
			_healthLabel.Text = _player.GetHealth().ToString();
		}

		// Hardcoded shit
		if (_player != null && _healthIndicator != null)
		{
			if (_player.GetHealth() > 75)
			{
				_healthIndicator.Frame = 0;
				return;
			}
			else if (_player.GetHealth() > 50)
			{
				_healthIndicator.Frame = 1;
				return;
			}
			else if (_player.GetHealth() > 25)
			{
				_healthIndicator.Frame = 2;
				return;
			}
			else
			{
				_healthIndicator.Frame = 3;
			}
		}
	}

	private void OnZeroSpawnerLeft()
    {
		if (_exitIndicator == null)
		{
			GD.PrintErr("GUI is missing Exit Indicator");
			return;
		}

		_exitIndicator.Play("Opening");
    }

	private void OnExitIndicatorAnimationFinished()
    {
        if (_exitIndicator == null)
		{
			GD.PrintErr("GUI is missing Exit Indicator");
			return;
		}

		_exitIndicator.Play("Open");
    }
}
